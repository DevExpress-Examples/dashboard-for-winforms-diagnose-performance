using System;
using System.Collections.Generic;
using System.Linq;
using DashboardDiagnosticTool.Data;
using DiagnosticTool.LinuxTools;

namespace DashboardDiagnosticTool {
    public class TraceDataProcessor {
        internal class Vertex {
            public BenchmarkItem Item { get; }
            public DateTime Start { get; }
            public Dictionary<int, BenchmarkItem> Children { get; } = new Dictionary<int, BenchmarkItem>();
            public List<int> Order { get; } = new List<int>();
            Dictionary<int, int> Counts = new Dictionary<int, int>();

            public Vertex(BenchmarkItem item, DateTime timeStamp) {
                Item = item;
                Start = timeStamp;
            }

            public void Add(BenchmarkItem child, int methodId) {
                BenchmarkItem real;
                if(Children.TryGetValue(methodId, out real)) {
                    Counts[methodId]++;
                    real.Count++;
                    real.MSecs += child.MSecs;
                    real.End = child.End;
                } else {
                    Children.Add(methodId, child);
                    Counts.Add(methodId, 1);
                    Order.Add(methodId);
                }
            }
            public void Process(int end, DateTime stop) {
                Item.MSecs = (stop - Start).TotalMilliseconds;
                Item.End = end;
            }
            public string ProcessChildren() {
                string id = Item.Name;
                foreach(int index in Order) {
                    Item.Children.Add(Children[index]);
                    id += "+" + index + "." + Counts[index];
                }
                return id;
            }
        }
        readonly int sessionId;
        readonly Dictionary<int, Stack<Vertex>> current = new Dictionary<int, Stack<Vertex>>();
        readonly Dictionary<string, int> methodIds = new Dictionary<string, int>();
        public Dictionary<int, List<TraceItem>> TraceItems { get; } = new Dictionary<int, List<TraceItem>>();
        List<BenchmarkItem> benchmarks;
        public List<BenchmarkItem> Benchmarks {
            get {
                if(benchmarks == null) {
                    benchmarks = current
                        .Where(element => element.Value.Count == 1)
                        .Select(element => {
                            Vertex thread = element.Value.Pop();
                            thread.ProcessChildren();
                            return thread.Item;
                        })
                        .ToList();
                }
                return benchmarks;
            }
        }
        public TraceDataProcessor(int newSessionId) {
            sessionId = newSessionId;
        }

        void AddItem(int threadID, string name, DateTime timeStamp) {
            var item = new BenchmarkItem(name, sessionId, threadID);
            item.Start = TraceItems[threadID].Count;
            current[threadID].Push(new Vertex(item, timeStamp));
        }
        void Enter(int threadID, string name, DateTime timeStamp) {
            ProcessThread(threadID, timeStamp);
            AddItem(threadID, name, timeStamp);
        }
        void FixEnter(string name, DateTime timeStamp, int threadId) {
            if(!current.ContainsKey(threadId) || current[threadId].Peek().Item.Name != name)
                Enter(threadId, name, timeStamp);
        }
        public void Process(DXTraceEvent traceData) {
            DateTime timeStamp = traceData.TimeStamp;
            int threadId = traceData.ThreadID;
            if(traceData.EventKind == EventKind.Enter) {
                Enter(threadId, traceData.Name, timeStamp);
            } else if(traceData.EventKind == EventKind.Leave) {
                FixEnter(traceData.Name, timeStamp, threadId);

                Vertex last = current[threadId].Pop();
                last.Process(TraceItems[threadId].Count, timeStamp);
                string id = last.ProcessChildren();
                if(!methodIds.TryGetValue(id, out int methodId)) {
                    methodId = methodIds.Count;
                    methodIds.Add(id, methodId);
                }
                current[threadId].Peek().Add(last.Item, methodId);
                ProcessThread(threadId, timeStamp);
            } else if(traceData.EventKind == EventKind.Trace) {
                ProcessThread(threadId, timeStamp);
                TraceItems[threadId].Add(new TraceItem(sessionId, threadId) {
                    EventType = traceData.EventType,
                    Data = traceData.Name
                });
            }
        }

        void ProcessThread(int threadID, DateTime timeStamp) {
            string threadName = $"Thread{threadID}";
            if(current.TryGetValue(threadID, out Stack<Vertex> items)) {
                if(items.Count == 1)
                    items.Peek().Process(TraceItems[threadID].Count, timeStamp);
            } else {
                TraceItems.Add(threadID, new List<TraceItem>());
                current.Add(threadID, new Stack<Vertex>());
                AddItem(threadID, threadName, timeStamp);
            }
        }
    }
}
