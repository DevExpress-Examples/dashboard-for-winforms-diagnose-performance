Imports System
Imports System.Diagnostics
Imports Microsoft.Diagnostics.Tracing

Namespace DiagnosticTool.LinuxTools

    Public Class DXTraceEvent

        Public Property EventKind As EventKind

        Public Property TimeStamp As Date

        Public Property Name As String

        Public Property ThreadID As Integer

        Public Property EventType As TraceEventType

        Private Function GetEvent(ByVal eventName As String) As EventKind
            If eventName.StartsWith("Enter") Then
                Return EventKind.Enter
            ElseIf eventName.StartsWith("Leave") Then
                Return EventKind.Leave
            ElseIf eventName.StartsWith("Trace") Then
                Return EventKind.Trace
            Else
                Return EventKind.Ignore
            End If
        End Function

        Private Function GetName(ByVal payloadByName As Func(Of String, Object)) As String
            If EventKind = EventKind.Enter OrElse EventKind = EventKind.Leave Then Return CStr(payloadByName("Name"))
            If EventKind = EventKind.Trace Then Return CStr(payloadByName("Data"))
            Return ""
        End Function

        Public Sub New(ByVal traceData As TraceEvent)
            Me.New(traceData.EventName, traceData.TimeStamp, Function(name) traceData.PayloadByName(name))
        End Sub

        Public Sub New(ByVal eventName As String, ByVal timeStamp As Date, ByVal payloadByName As Func(Of String, Object))
            EventKind = GetEvent(eventName)
            Me.TimeStamp = timeStamp
            ThreadID = If(EventKind <> EventKind.Ignore, CInt(payloadByName("Id")), 0)
            Name = GetName(payloadByName)
            EventType = If(EventKind = EventKind.Trace, CType(Convert.ToInt32(payloadByName("EventType")), TraceEventType), TraceEventType.Verbose)
        End Sub
    End Class
End Namespace
