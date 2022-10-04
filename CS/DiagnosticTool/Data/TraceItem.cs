using System.Diagnostics;
using System.Xml.Serialization;

namespace DashboardDiagnosticTool.Data {
    public class TraceItem : ItemBase {
        [XmlAttribute]
        public TraceEventType EventType { get; set; }
        [XmlAttribute]
        public string Data { get; set; } = string.Empty;
        public TraceItem() { }
        public TraceItem(int sessionId, int threadId) : base(sessionId, threadId) {

        }
        public override int GetHashCode() {
            return Data.GetHashCode();
        }
    }
}
