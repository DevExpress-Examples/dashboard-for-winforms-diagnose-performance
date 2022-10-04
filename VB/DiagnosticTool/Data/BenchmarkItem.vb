Imports System.Collections.Generic
Imports System.Xml.Serialization

Namespace DashboardDiagnosticTool.Data

    Public Class BenchmarkItem
        Inherits ItemBase

        Public Property ID As Integer

        <XmlArray>
        Public Property Children As List(Of BenchmarkItem)

        <XmlAttribute>
        Public Property Start As Integer

        <XmlAttribute>
        Public Property [End] As Integer

        <XmlAttribute>
        Public Property Name As String

        <XmlAttribute>
        Public Property Count As Integer

        <XmlAttribute>
        Public Property MSecs As Double

        Public Sub New()
        End Sub

        Public Sub New(ByVal name As String, ByVal sessionId As Integer, ByVal threadId As Integer)
            MyBase.New(sessionId, threadId)
            Children = New List(Of BenchmarkItem)()
            Me.Name = name
            Count = 1
        End Sub

        Public Overrides Function GetHashCode() As Integer
            Return(SessionId & "." & ID).GetHashCode()
        End Function
    End Class
End Namespace
