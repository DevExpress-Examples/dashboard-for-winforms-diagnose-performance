Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports DashboardDiagnosticTool.Data
Imports DiagnosticTool.LinuxTools
Imports Microsoft.Diagnostics.Tracing
Imports Microsoft.Diagnostics.Tracing.Session

Namespace DashboardDiagnosticTool

    Public Class DiagnosticController
        Implements IDisposable

        Private fileName As String = ""

        Private runningSession As Integer = 0

        Private path As String

        Private OnStop As Action(Of TraceDataProcessor)

        Private ReadOnly fileController As IFileController

        Private ReadOnly place As String = "Session"

        Private ReadOnly telemetry As String = "DX-Dashboard-Telemetry"

        Private ReadOnly traceItemsField As Dictionary(Of Integer, Dictionary(Of Integer, List(Of TraceItem))) = New Dictionary(Of Integer, Dictionary(Of Integer, List(Of TraceItem)))()

        Private ReadOnly benchmarksField As Dictionary(Of Integer, List(Of BenchmarkItem)) = New Dictionary(Of Integer, List(Of BenchmarkItem))()

        Private ReadOnly sessionsField As List(Of SessionItem) = New List(Of SessionItem)()

        Public InProcess As Boolean = False

        Public ReadOnly Property FileExists As Boolean
            Get
                Return File.Exists(fileName)
            End Get
        End Property

        Public ReadOnly Property TraceItems As Dictionary(Of Integer, Dictionary(Of Integer, List(Of TraceItem)))
            Get
                Return traceItemsField
            End Get
        End Property

        Public ReadOnly Property Benchmarks As Dictionary(Of Integer, List(Of BenchmarkItem))
            Get
                Return benchmarksField
            End Get
        End Property

        Public ReadOnly Property Sessions As List(Of SessionItem)
            Get
                Return sessionsField
            End Get
        End Property

        Public Event ThrowException As Action(Of Exception)

        Public Event SessionsChanged As Action

        Public Event StartProcessing As Action

        Public Event EndProcessing As Action

        Public Sub New(ByVal fileController As IFileController)
            Me.fileController = fileController
        End Sub

        Public Sub New()
            fileController = New DefaultFileController()
        End Sub

        Public Function CanHandleCommand(ByVal command As ControllerCommand) As Boolean
            Select Case command
                Case ControllerCommand.Open
                    Return Not InProcess
                Case ControllerCommand.Save
                    Return Not InProcess AndAlso Sessions.Count <> 0
                Case ControllerCommand.SaveAs
                    Return Not InProcess AndAlso FileExists
                Case ControllerCommand.Start
                    Return Not InProcess
                Case ControllerCommand.Stop
                    Return InProcess
                Case ControllerCommand.Delete
                    Return Not InProcess AndAlso Sessions.Count <> 0
            End Select

            Return False
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            [Stop]()
            If CanHandleCommand(ControllerCommand.Save) Then
                Save()
            ElseIf CanHandleCommand(ControllerCommand.SaveAs) Then
                SaveAs()
            End If
        End Sub

        Public Sub [Stop]()
            If CanHandleCommand(ControllerCommand.Stop) Then
                Try
                    Dim processor As TraceDataProcessor = New TraceDataProcessor(runningSession)
                    OnStop.Invoke(processor)
                    Update(processor)
                Finally
                    OnStop = Nothing
                    If File.Exists(path) Then File.Delete(path)
                    path = ""
                    InProcess = False
                End Try
            End If
        End Sub

        Public Function GetBenchmarks(ByVal session As Integer) As List(Of BenchmarkItem)
            Return Benchmarks(session)
        End Function

        Public Function GetTraceEvents(ByVal session As Integer) As List(Of TraceItem)
            Return TraceItems(session).Values.SelectMany(Function(x) x).ToList()
        End Function

        Public Function GetTraceEvents(ByVal session As Integer, ByVal current As BenchmarkItem) As List(Of TraceItem)
            Dim length As Integer = current.End - current.Start
            Return TraceItems(session)(current.ThreadId).GetRange(current.Start, length)
        End Function

        Private Sub WorkWithFile(ByVal command As ControllerCommand, ByVal process As Action)
            If CanHandleCommand(command) Then
                Try
                    process()
                Catch e As Exception
                    RaiseEvent ThrowException(e)
                End Try
            End If
        End Sub

        Public Sub Open(ByVal Optional openName As String = "")
            WorkWithFile(ControllerCommand.Open, Sub()
                Dim fileName As String
                If fileController.TryOpenFile(fileName, openName) Then
                    Clear()
                    Me.fileName = fileName
                    Using stream = New MemoryStream(File.ReadAllBytes(fileName))
                        Dim [set] = BenchmarkHelper.LoadBenchmarkSet(stream)
                        Load([set])
                    End Using
                End If
            End Sub)
        End Sub

        Public Sub Save()
            WorkWithFile(ControllerCommand.Save, Sub()
                If FileExists OrElse fileController.TrySaveFile(fileName) Then
                    SaveBenchmarks(fileName)
                Else
                    SaveAs()
                End If
            End Sub)
        End Sub

        Public Sub SaveAs(ByVal Optional saveName As String = "")
            WorkWithFile(ControllerCommand.SaveAs, Sub()
                Dim fileName As String
                If fileController.TrySaveFile(fileName, If(String.IsNullOrEmpty(Me.fileName), saveName, IO.Path.GetFileName(Me.fileName))) Then
                    SaveBenchmarks(fileName)
                    Me.fileName = fileName
                End If
            End Sub)
        End Sub

        Private Sub SaveBenchmarks(ByVal fileName As String)
            Using stream = File.Create(fileName)
                BenchmarkHelper.SaveBenchmarkItems(Benchmarks, TraceItems, Sessions, stream)
            End Using
        End Sub

        Private Sub Load(ByVal [set] As BenchmarkItemSet)
            Loading(Benchmarks, [set].GetBenchmarks())
            LoadingTraceItems(TraceItems, [set].GetTraceItems())
            Sessions.AddRange([set].Sessions)
            runningSession = 0
            Dim number As Integer = Sessions.Count
            For i As Integer = 0 To number - 1
                Dim sessionId As Integer = Sessions(i).ID
                runningSession = Math.Max(sessionId, runningSession)
            Next

            RaiseEvent SessionsChanged()
        End Sub

        Private Sub Loading(Of T As ItemBase)(ByVal current As Dictionary(Of Integer, List(Of T)), ByVal [get] As Dictionary(Of Integer, List(Of T)))
            For Each pair As KeyValuePair(Of Integer, List(Of T)) In [get]
                Dim key As Integer = pair.Key
                current.Add(key, New List(Of T)())
                current(key).AddRange(pair.Value)
            Next
        End Sub

        Private Sub LoadingTraceItems(ByVal current As Dictionary(Of Integer, Dictionary(Of Integer, List(Of TraceItem))), ByVal [get] As Dictionary(Of Integer, Dictionary(Of Integer, List(Of TraceItem))))
            For Each session As KeyValuePair(Of Integer, Dictionary(Of Integer, List(Of TraceItem))) In [get]
                Dim key As Integer = session.Key
                current.Add(key, New Dictionary(Of Integer, List(Of TraceItem))())
                Loading(current(key), session.Value)
            Next
        End Sub

        Private Sub Clear()
            fileName = ""
            TraceItems.Clear()
            Benchmarks.Clear()
            Sessions.Clear()
            runningSession = 0
            RaiseEvent SessionsChanged()
        End Sub

        Public Sub Delete(ByVal item As SessionItem)
            If item Is Nothing Then Return
            Dim id As Integer = item.SessionId
            TraceItems.Remove(id)
            Benchmarks.Remove(id)
            For i As Integer = 0 To Sessions.Count - 1
                If Sessions(i).ID = id Then Sessions.RemoveAt(i)
            Next

            RaiseEvent SessionsChanged()
            runningSession = Enumerable.DefaultIfEmpty(Of Integer)(Enumerable.Select(Of SessionItem, Global.System.Int32)(Sessions, CType(Function(s) CInt(s.ID), Func(Of SessionItem, Integer))), CInt(0)).Max()
        End Sub

        Public Sub Start()
            InProcess = True
            Sessions.Add(New SessionItem(Interlocked.Increment(runningSession)))
            Benchmarks.Add(runningSession, New List(Of BenchmarkItem)())
            TraceItems.Add(runningSession, New Dictionary(Of Integer, List(Of TraceItem))())
            RaiseEvent SessionsChanged()
            OnStop = ReadBenchmarks()
        End Sub

        Private Function ReadBenchmarks() As Action(Of TraceDataProcessor)
            path = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"DX-Diagnostic.etl")
            If File.Exists(path) Then File.Delete(path)
            If Environment.OSVersion.Platform = PlatformID.Unix Then
                Return ReadBenchmarksLinux()
            Else
                Return ReadBenchmarksWindows()
            End If
        End Function

        Private Function ReadBenchmarksWindows() As Action(Of TraceDataProcessor)
            Dim session = New TraceEventSession(place, path)
            Dim id As Guid = TraceEventProviders.GetEventSourceGuidFromName(telemetry)
            session.EnableProvider(id)
            Thread.Sleep(100)
            Return Sub(processor)
                session.DisableProvider(id)
                session.Stop(True)
                Using source = New ETWTraceEventSource(path)
                     ''' Cannot convert LocalFunctionStatementSyntax, CONVERSION ERROR: Conversion for LocalFunctionStatement not implemented, please report this issue in 'void action(Microsoft.Diagn...' at character 12292
''' 
''' 
''' Input:
'''                     void action(Microsoft.Diagnostics.Tracing.TraceEvent data) => processor.Process(new DiagnosticTool.LinuxTools.DXTraceEvent(data));
''' 
'''  AddHandler source.Dynamic.All, Me.action
                    source.Process()
                    RemoveHandler source.Dynamic.All, Me.action
                End Using

                session.Dispose()
            End Sub
        End Function

        Private Sub ExecuteBash(ByVal cmd As String)
            Dim escapedArgs = cmd.Replace("""", "\""")
            Dim process = New Process With {.StartInfo = New ProcessStartInfo With {.FileName = "bash", .Arguments = $"-c ""{escapedArgs}""", .UseShellExecute = False, .CreateNoWindow = True}, .EnableRaisingEvents = True}
            process.Start()
            process.WaitForExit()
            process.Kill()
        End Sub

        Private Function ReadBenchmarksLinux() As Action(Of TraceDataProcessor)
            ExecuteBash(LinuxCommands.StartSession)
            Return Sub(processor)
                ExecuteBash(LinuxCommands.StopSession(path))
                Dim parser As DXTraceEventParser = New DXTraceEventParser()
                parser.Process(Sub(data) processor.Process(data), path)
            End Sub
        End Function

        Private Sub Update(ByVal processor As TraceDataProcessor)
            RaiseEvent StartProcessing()
            Try
                Benchmarks(runningSession) = New List(Of BenchmarkItem)(processor.Benchmarks)
                TraceItems(runningSession) = New Dictionary(Of Integer, List(Of TraceItem))(processor.TraceItems)
            Finally
                RaiseEvent EndProcessing()
                RaiseEvent SessionsChanged()
            End Try
        End Sub
    End Class
End Namespace
