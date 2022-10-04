Imports System.Runtime.InteropServices

Namespace DashboardDiagnosticTool

    Public Interface IFileController

        Function TryOpenFile(<Out> ByRef openName As String, ByVal Optional fileName As String = "") As Boolean

        Function TrySaveFile(<Out> ByRef outFileName As String, ByVal Optional fileName As String = "") As Boolean

    End Interface
End Namespace
