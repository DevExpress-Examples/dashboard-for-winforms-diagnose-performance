Imports System.IO
Imports System.Runtime.InteropServices

Namespace DashboardDiagnosticTool

    Friend Class DefaultFileController
        Implements IFileController

        Private fileExtension As String = ".xml"

        Public Function TryOpenFile(<Out> ByRef openName As String, ByVal Optional fileName As String = "") As Boolean Implements IFileController.TryOpenFile
            If Equals(Path.GetExtension(fileName), fileExtension) Then
                openName = fileName
                Return True
            End If

            openName = ""
            Return False
        End Function

        Public Function TrySaveFile(<Out> ByRef outFileName As String, ByVal Optional fileName As String = "") As Boolean Implements IFileController.TrySaveFile
            If Equals(Path.GetExtension(fileName), fileExtension) Then
                outFileName = fileName
                Return True
            End If

            outFileName = ""
            Return False
        End Function
    End Class
End Namespace
