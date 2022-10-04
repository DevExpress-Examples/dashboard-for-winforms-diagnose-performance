using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using DashboardDiagnosticTool.Data;
using DiagnosticTool.LinuxTools;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;

namespace DashboardDiagnosticTool {
    public class DiagnosticController : IDisposable {
        string fileName = "";
        int runningSession = 0;
        string path;
        Action<TraceDataProcessor> OnStop;

        readonly IFileController fileController;
        readonly string place = "Session";
        readonly string telemetry = "DX-Dashboard-Telemetry";
        readonly Dictionary<int, Dictionary<int, List<TraceItem>>> traceItems = new Dictionary<int, Dictionary<int, List<TraceItem>>>();
        readonly Dictionary<int, List<BenchmarkItem>> benchmarks = new Dictionary<int, List<BenchmarkItem>>();
        readonly List<SessionItem> sessions = new List<SessionItem>();

        public bool InProcess = false;
        public bool FileExists => File.Exists(fileName);
        public Dictionary<int, Dictionary<int, List<TraceItem>>> TraceItems { get => traceItems; }
        public Dictionary<int, List<BenchmarkItem>> Benchmarks { get => benchmarks; }
        public List<SessionItem> Sessions { get => sessions; }

        public event Action<Exception> ThrowException;
        public event Action SessionsChanged;
        public event Action StartProcessing;
        public event Action EndProcessing;

        public DiagnosticController(IFileController fileController) {
            this.fileController = fileController;
        }

        public DiagnosticController() {
            this.fileController = new DefaultFileController();
        }

        public bool CanHandleCommand(ControllerCommand command) {
            switch(command) {
                case ControllerCommand.Open:
                    return !InProcess;
                case ControllerCommand.Save:
                    return !InProcess && Sessions.Count != 0;
                case ControllerCommand.SaveAs:
                    return !InProcess && FileExists;
                case ControllerCommand.Start:
                    return !InProcess;
                case ControllerCommand.Stop:
                    return InProcess;
                case ControllerCommand.Delete:
                    return !InProcess && Sessions.Count != 0;
            }
            return false;
        }
        public void Dispose() {
            Stop();
            if(CanHandleCommand(ControllerCommand.Save))
                Save();
            else if(CanHandleCommand(ControllerCommand.SaveAs))
                SaveAs();
        }
        public void Stop() {
            if(CanHandleCommand(ControllerCommand.Stop)) {
                try {
                    TraceDataProcessor processor = new TraceDataProcessor(runningSession);
                    OnStop.Invoke(processor);
                    Update(processor);
                } finally {
                    OnStop = null;
                    if(File.Exists(path))
                        File.Delete(path);
                    path = "";
                    InProcess = false;
                }
            }
        }
        public List<BenchmarkItem> GetBenchmarks(int session) {
            return Benchmarks[session];
        }
        public List<TraceItem> GetTraceEvents(int session) {
            return TraceItems[session].Values.SelectMany(x => x).ToList();
        }
        public List<TraceItem> GetTraceEvents(int session, BenchmarkItem current) {
            int length = current.End - current.Start;
            return TraceItems[session][current.ThreadId]
                .GetRange(current.Start, length);
        }
        void WorkWithFile(ControllerCommand command, Action process) {
            if(CanHandleCommand(command))
                try {
                    process();
                } catch(Exception e) {
                    ThrowException.Invoke(e);
                }
        }
        public void Open(string openName = "") {
            WorkWithFile(ControllerCommand.Open, () => {
                string fileName;
                if(fileController.TryOpenFile(out fileName, openName)) {
                    Clear();
                    this.fileName = fileName;
                    using(var stream = new MemoryStream(File.ReadAllBytes(fileName))) {
                        var set = BenchmarkHelper.LoadBenchmarkSet(stream);
                        Load(set);
                    }
                }
            });
        }
        public void Save() {
            WorkWithFile(ControllerCommand.Save, () => {
                if(FileExists || fileController.TrySaveFile(out fileName))
                    SaveBenchmarks(fileName);
                else SaveAs();
            });
        }
        public void SaveAs(string saveName = "") {
            WorkWithFile(ControllerCommand.SaveAs, () => {
                string fileName;
                if(fileController.TrySaveFile(out fileName,
                    string.IsNullOrEmpty(this.fileName) ? saveName: Path.GetFileName(this.fileName))) {
                    SaveBenchmarks(fileName);
                    this.fileName = fileName;
                }
            });
        }
        void SaveBenchmarks(string fileName) {
            using(var stream = File.Create(fileName)) {
                BenchmarkHelper.SaveBenchmarkItems(Benchmarks, TraceItems, Sessions, stream);
            }
        }
        void Load(BenchmarkItemSet set) {
            Loading(Benchmarks, set.GetBenchmarks());
            LoadingTraceItems(TraceItems, set.GetTraceItems());
            Sessions.AddRange(set.Sessions);
            runningSession = 0;
            int number = Sessions.Count;
            for(int i = 0; i < number; i++) {
                int sessionId = Sessions[i].ID;
                runningSession = Math.Max(sessionId, runningSession);
            }
            SessionsChanged?.Invoke();
        }
        void Loading<T>(Dictionary<int, List<T>> current, Dictionary<int, List<T>> get) where T : ItemBase {
            foreach(KeyValuePair<int, List<T>> pair in get) {
                int key = pair.Key;
                current.Add(key, new List<T>());
                current[key].AddRange(pair.Value);
            }
        }
        void LoadingTraceItems(Dictionary<int, Dictionary<int, List<TraceItem>>> current,
            Dictionary<int, Dictionary<int, List<TraceItem>>> get) {
            foreach(KeyValuePair<int, Dictionary<int, List<TraceItem>>> session in get) {
                int key = session.Key;
                current.Add(key, new Dictionary<int, List<TraceItem>>());
                Loading(current[key], session.Value);
            }
        }
        void Clear() {
            fileName = "";
            TraceItems.Clear();
            Benchmarks.Clear();
            Sessions.Clear();
            runningSession = 0;
            SessionsChanged?.Invoke();
        }
        public void Delete(SessionItem item) {
            if(item == null) return;
            int id = item.SessionId;
            TraceItems.Remove(id);
            Benchmarks.Remove(id);
            for(int i = 0; i < Sessions.Count; i++) {
                if(Sessions[i].ID == id)
                    Sessions.RemoveAt(i);
            }
            SessionsChanged?.Invoke();
            runningSession = Sessions.Select(s => s.ID).DefaultIfEmpty(0).Max();
        }
        public void Start() {
            InProcess = true;
            Sessions.Add(new SessionItem(++runningSession));
            Benchmarks.Add(runningSession, new List<BenchmarkItem>());
            TraceItems.Add(runningSession, new Dictionary<int, List<TraceItem>>());
            SessionsChanged?.Invoke();
            OnStop = ReadBenchmarks();
        }

        Action<TraceDataProcessor> ReadBenchmarks() {
            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"DX-Diagnostic.etl");
            if(File.Exists(path))
                File.Delete(path);
            if(Environment.OSVersion.Platform == PlatformID.Unix)
                return ReadBenchmarksLinux();
            else
                return ReadBenchmarksWindows();
        }
        Action<TraceDataProcessor> ReadBenchmarksWindows() {
            var session = new TraceEventSession(place, path);
            Guid id = TraceEventProviders.GetEventSourceGuidFromName(telemetry);
            session.EnableProvider(id);
            Thread.Sleep(100);
            return processor => {
                session.DisableProvider(id);
                session.Stop(true);
                using(var source = new ETWTraceEventSource(path)) {
                    void action(TraceEvent data) => processor.Process(new DXTraceEvent(data));
                    source.Dynamic.All += action;
                    source.Process();
                    source.Dynamic.All -= action;
                }
                session.Dispose();
            };
        }

        void ExecuteBash(string cmd) {
            var escapedArgs = cmd.Replace("\"", "\\\"");
            var process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };
            process.Start();
            process.WaitForExit();
            process.Kill();
        }

        Action<TraceDataProcessor> ReadBenchmarksLinux() {
            ExecuteBash(LinuxCommands.StartSession);
            return processor => {
                ExecuteBash(LinuxCommands.StopSession(path));
                DXTraceEventParser parser = new DXTraceEventParser();
                parser.Process(data => processor.Process(data), path);
            };
        }

        void Update(TraceDataProcessor processor) {
            StartProcessing?.Invoke();
            try {
                Benchmarks[runningSession] = new List<BenchmarkItem>(processor.Benchmarks);
                TraceItems[runningSession] = new Dictionary<int, List<TraceItem>>(processor.TraceItems);
            } finally {
                EndProcessing?.Invoke();
                SessionsChanged?.Invoke();
            }
        }
    }
}
