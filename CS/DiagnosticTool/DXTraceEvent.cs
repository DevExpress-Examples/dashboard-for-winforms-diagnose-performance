using System;
using System.Diagnostics;
using Microsoft.Diagnostics.Tracing;

namespace DiagnosticTool.LinuxTools {
    public class DXTraceEvent {
        public EventKind EventKind { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Name { get; set; }
        public int ThreadID { get; set; }
        public TraceEventType EventType { get; set; }

        EventKind GetEvent(string eventName) {
            if(eventName.StartsWith("Enter"))
                return EventKind.Enter;
            else if(eventName.StartsWith("Leave"))
                return EventKind.Leave;
            else if(eventName.StartsWith("Trace"))
                return EventKind.Trace;
            else
                return EventKind.Ignore;
        }

        string GetName(Func<string, object> payloadByName) {
            if(EventKind == EventKind.Enter || EventKind == EventKind.Leave)
                return (string)payloadByName("Name");
            if(EventKind == EventKind.Trace)
                return (string)payloadByName("Data");
            return "";
        }
        public DXTraceEvent(TraceEvent traceData)
            : this(traceData.EventName, traceData.TimeStamp, name => traceData.PayloadByName(name)) { }

        public DXTraceEvent(string eventName, DateTime timeStamp, Func<string, object> payloadByName) {
            EventKind = GetEvent(eventName);
            TimeStamp = timeStamp;
            ThreadID = EventKind != EventKind.Ignore ? (int)payloadByName("Id") : 0;
            Name = GetName(payloadByName);
            EventType = EventKind == EventKind.Trace ? (TraceEventType)Convert.ToInt32(payloadByName("EventType")) : TraceEventType.Verbose;
        }
    }
}
