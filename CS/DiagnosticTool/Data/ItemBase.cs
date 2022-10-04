using System.Xml.Serialization;

namespace DashboardDiagnosticTool.Data {
    public class ItemBase {
        [XmlAttribute]
        public int SessionId { get; set; }
        [XmlAttribute]
        public int ThreadId { get; set; }
        public ItemBase() { }
        public ItemBase(int sessionId, int threadId) {
            SessionId = sessionId;
            ThreadId = threadId;
        }
    }
}
