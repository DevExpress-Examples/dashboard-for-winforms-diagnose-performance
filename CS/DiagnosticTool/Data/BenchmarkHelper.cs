using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace DashboardDiagnosticTool.Data {
    class BenchmarkHelper {
        public static void SaveBenchmarkItems(Dictionary<int, List<BenchmarkItem>> benchmarks, Dictionary<int, Dictionary<int, List<TraceItem>>> traceItems, IEnumerable<SessionItem> sessions, Stream stream) {
            var serializer = new XmlSerializer(typeof(BenchmarkItemSet));
            serializer.Serialize(stream, new BenchmarkItemSet(benchmarks, traceItems, sessions));
        }
        public static BenchmarkItemSet LoadBenchmarkSet(Stream stream) {
            var serializer = new XmlSerializer(typeof(BenchmarkItemSet), null, null, null, null);
            return serializer.Deserialize(stream) as BenchmarkItemSet;
        }
    }
}
