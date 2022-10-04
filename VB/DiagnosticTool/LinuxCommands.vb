Namespace DiagnosticTool.LinuxTools

    Public Class LinuxCommands

        Public Shared StartSession As String = $"lttng create dx-session -o./dx-diagnostic-trace 
 lttng enable-channel --userspace DotNetCoreChannel 
 lttng add-context --userspace --channel=DotNetCoreChannel --type=vtid 
 lttng enable-event --userspace --tracepoint DotNETRuntime:EventSource --channel=DotNetCoreChannel --filter ""EventSourceName=='DX-Telemetry'"" 
 lttng start"

        Public Shared Function StopSession(ByVal path As String) As String
            Return $"lttng stop 
 lttng view >> {path} 
 lttng destroy dx-session 
 rm -r dx-diagnostic-trace"
        End Function
    End Class
End Namespace
