Imports System.Diagnostics
Imports System.Xml.Serialization

Namespace DashboardDiagnosticTool.Data

    Public Class TraceItem
        Inherits ItemBase

        <XmlAttribute>
        Public Property EventType As TraceEventType

        <XmlAttribute>
        Public Property Data As String = String.Empty

        Public Sub New()
        End Sub

        Public Sub New(ByVal sessionId As Integer, ByVal threadId As Integer)
            MyBase.New(sessionId, threadId)
        End Sub

        Public Overrides Function GetHashCode() As Integer
            Return Data.GetHashCode()
        End Function
    End Class
End Namespace
