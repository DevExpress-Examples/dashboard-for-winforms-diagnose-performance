namespace DiagnosticTool.LinuxTools {
    public class LinuxCommands {
        public static string StartSession = $"lttng create dx-session -o./dx-diagnostic-trace \n lttng enable-channel --userspace DotNetCoreChannel \n lttng add-context --userspace --channel=DotNetCoreChannel --type=vtid \n lttng enable-event --userspace --tracepoint DotNETRuntime:EventSource --channel=DotNetCoreChannel --filter \"EventSourceName==\'DX-Telemetry\'\" \n lttng start";

        public static string StopSession(string path) => $"lttng stop \n lttng view >> {path} \n lttng destroy dx-session \n rm -r dx-diagnostic-trace";
    }
}
