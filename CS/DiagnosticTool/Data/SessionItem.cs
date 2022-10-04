using System.Xml.Serialization;

namespace DashboardDiagnosticTool.Data {
    public class SessionItem : ItemBase {
        [XmlAttribute]
        public int ID => SessionId;
        public int ParentID => SessionId;

        [XmlAttribute]
        public string Name { get; set; }
        public SessionItem() { }
        public SessionItem(int sessionId) : base(sessionId, -1) {
            Name = $"Session{sessionId}";
        }

        public override int GetHashCode() {
            return ID.GetHashCode();
        }
    }
}
