Imports System.Xml.Serialization

Namespace DashboardDiagnosticTool.Data

    Public Class SessionItem
        Inherits ItemBase

        <XmlAttribute>
        Public ReadOnly Property ID As Integer
            Get
                Return SessionId
            End Get
        End Property

        Public ReadOnly Property ParentID As Integer
            Get
                Return SessionId
            End Get
        End Property

        <XmlAttribute>
        Public Property Name As String

        Public Sub New()
        End Sub

        Public Sub New(ByVal sessionId As Integer)
            MyBase.New(sessionId, -1)
            Name = $"Session{sessionId}"
        End Sub

        Public Overrides Function GetHashCode() As Integer
            Return ID.GetHashCode()
        End Function
    End Class
End Namespace
