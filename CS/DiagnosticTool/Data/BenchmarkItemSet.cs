using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace DashboardDiagnosticTool.Data {
    [XmlRoot]
    public class BenchmarkItemSet {
        public List<BenchmarkItem> Benchmarks { get; private set; } = new List<BenchmarkItem>();
        public List<TraceItem> TraceItems { get; private set; } = new List<TraceItem>();
        public List<SessionItem> Sessions { get; private set; } = new List<SessionItem>();

        public BenchmarkItemSet() {
        }
        public BenchmarkItemSet(Dictionary<int, List<BenchmarkItem>> benchmarks, Dictionary<int, Dictionary<int, List<TraceItem>>> traceItems, IEnumerable<SessionItem> sessions) {
            Benchmarks = benchmarks.Values.SelectMany(x => x).ToList();
            TraceItems = traceItems.Values.SelectMany(dict => dict.Values.SelectMany(x => x)).ToList();
            Sessions.AddRange(sessions);
        }

        public Dictionary<int, List<BenchmarkItem>> GetBenchmarks() {
            Dictionary<int, List<BenchmarkItem>> benchmarksBySessionId = new Dictionary<int, List<BenchmarkItem>>();
            foreach(var session in Sessions)
                benchmarksBySessionId.Add(session.ID, new List<BenchmarkItem>());
            Benchmarks.ForEach(benchmark => {
                int sessionId = benchmark.SessionId;
                if(!benchmarksBySessionId.TryGetValue(sessionId, out var benchmarkItems)) {
                    benchmarkItems = new List<BenchmarkItem>();
                    benchmarksBySessionId.Add(sessionId, benchmarkItems);
                }
                benchmarkItems.Add(benchmark);
            });
            return benchmarksBySessionId;
        }

        public Dictionary<int, Dictionary<int, List<TraceItem>>> GetTraceItems() {
            Dictionary<int, Dictionary<int, List<TraceItem>>> traceItemsBySessionId = new Dictionary<int, Dictionary<int, List<TraceItem>>>();
            foreach(var session in Sessions)
                traceItemsBySessionId.Add(session.ID, new Dictionary<int, List<TraceItem>>());
            TraceItems.ForEach(traceItem => {
                int sessionId = traceItem.SessionId;
                int threadId = traceItem.ThreadId;
                if(!traceItemsBySessionId.TryGetValue(sessionId, out var traceItemsByThreadId)) {
                    traceItemsByThreadId = new Dictionary<int, List<TraceItem>>();
                    traceItemsBySessionId.Add(sessionId, traceItemsByThreadId);
                }
                if(!traceItemsByThreadId.TryGetValue(threadId, out var traceItems)) {
                    traceItems = new List<TraceItem>();
                    traceItemsByThreadId.Add(threadId, traceItems);
                }
                traceItems.Add(traceItem);
            });
            return traceItemsBySessionId;
        }
    }
}
