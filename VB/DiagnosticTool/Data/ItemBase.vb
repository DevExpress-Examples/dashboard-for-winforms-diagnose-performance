Imports System.Xml.Serialization

Namespace DashboardDiagnosticTool.Data

    Public Class ItemBase

        <XmlAttribute>
        Public Property SessionId As Integer

        <XmlAttribute>
        Public Property ThreadId As Integer

        Public Sub New()
        End Sub

        Public Sub New(ByVal sessionId As Integer, ByVal threadId As Integer)
            Me.SessionId = sessionId
            Me.ThreadId = threadId
        End Sub
    End Class
End Namespace
