Imports System
Imports System.IO
Imports System.Text.RegularExpressions

Namespace DiagnosticTool.LinuxTools

    Public Class DXTraceEventParser

        Private Function GetPayload(ByVal payload As String, ByVal name As String) As Object
            Dim regex As Regex = New Regex("(.*)[\\]+""" & Regex.Escape(name) & "[\\]+"":[\\]*""?(?<payload>[^ \\,""]+)[\\]*""?(.*)")
            Return regex.Match(payload).Groups("payload").Value
        End Function

        Public Function Parse(ByVal log As String) As DXTraceEvent
            Dim pattern As String = "\[(?<Timestamp>[0-9\.:]+)\]" & " \(([0-9\+\.\?]+)\) (\w+) ([\w:]+) { cpu_id = (?<CPU_ID>[0-9]+) }, { vtid = (?<ThreadID>[0-9]+) }," & " { ([\w\d\s=]+), EventName = ""(?<EventName>\w+)"", (\w+) = ""([\w\s-]+)"", Payload = (""""|""{(?<Payload>.+)}"") }"
            Dim match As Match = Regex.Match(log, pattern)
            Return New DXTraceEvent(match.Groups("EventName").Value, Date.Parse(match.Groups("Timestamp").Value), Function(name) GetPayload(match.Groups("Payload").Value, name))
        End Function

        Public Sub Process(ByVal pProcess As Action(Of DXTraceEvent), ByVal path As String)
            Using reader As StreamReader = New StreamReader(path)
                While reader.Peek() > 0
                    pProcess(Parse(reader.ReadLine()))
                End While
            End Using
        End Sub
    End Class
End Namespace
