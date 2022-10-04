using System.Collections.Generic;
using System.Xml.Serialization;

namespace DashboardDiagnosticTool.Data {
    public class BenchmarkItem : ItemBase {
        public int ID { get; set; }
        [XmlArray]
        public List<BenchmarkItem> Children { get; set; }
        [XmlAttribute]
        public int Start { get; set; }
        [XmlAttribute]
        public int End { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public int Count { get; set; }
        [XmlAttribute]
        public double MSecs { get; set; }
        public BenchmarkItem() { }
        public BenchmarkItem(string name, int sessionId, int threadId) : base(sessionId, threadId) {
            Children = new List<BenchmarkItem>();
            Name = name;
            Count = 1;
        }
        public override int GetHashCode() {
            return (SessionId + "." + ID).GetHashCode();
        }
    }
}
