using System;
using System.IO;
using System.Text.RegularExpressions;

namespace DiagnosticTool.LinuxTools {
    public class DXTraceEventParser {
        object GetPayload(string payload, string name) {
            Regex regex = new Regex(@"(.*)[\\]+""" + Regex.Escape(name) + @"[\\]+"":[\\]*""?(?<payload>[^ \\,""]+)[\\]*""?(.*)");
            return regex.Match(payload).Groups["payload"].Value;
        }
        public DXTraceEvent Parse(string log) {
            string pattern = @"\[(?<Timestamp>[0-9\.:]+)\]" +
            @" \(([0-9\+\.\?]+)\) (\w+) ([\w:]+) { cpu_id = (?<CPU_ID>[0-9]+) }, { vtid = (?<ThreadID>[0-9]+) }," +
            @" { ([\w\d\s=]+), EventName = ""(?<EventName>\w+)"", (\w+) = ""([\w\s-]+)"", Payload = (""""|""{(?<Payload>.+)}"") }";
            Match match = Regex.Match(log, pattern);
            return new DXTraceEvent(
                match.Groups["EventName"].Value,
                DateTime.Parse(match.Groups["Timestamp"].Value),
                name => GetPayload(match.Groups["Payload"].Value, name));
        }

        public void Process(Action<DXTraceEvent> process, string path) {
            using(StreamReader reader = new StreamReader(path)) {
                while(reader.Peek() > 0) {
                    process(Parse(reader.ReadLine()));
                }
            }
        }
    }
}
