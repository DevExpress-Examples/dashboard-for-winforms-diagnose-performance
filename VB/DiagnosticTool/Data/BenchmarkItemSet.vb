Imports System.Collections.Generic
Imports System.Linq
Imports System.Xml.Serialization

Namespace DashboardDiagnosticTool.Data

    <XmlRoot>
    Public Class BenchmarkItemSet

        Private _Benchmarks As List(Of DashboardDiagnosticTool.Data.BenchmarkItem), _TraceItems As List(Of DashboardDiagnosticTool.Data.TraceItem), _Sessions As List(Of DashboardDiagnosticTool.Data.SessionItem)

        Public Property Benchmarks As List(Of BenchmarkItem) = New List(Of BenchmarkItem)()
            Get
                Return _Benchmarks
            End Get

            Private Set(ByVal value As List(Of BenchmarkItem))
                _Benchmarks = value
            End Set
        End Property

        Public Property TraceItems As List(Of TraceItem) = New List(Of TraceItem)()
            Get
                Return _TraceItems
            End Get

            Private Set(ByVal value As List(Of TraceItem))
                _TraceItems = value
            End Set
        End Property

        Public Property Sessions As List(Of SessionItem) = New List(Of SessionItem)()
            Get
                Return _Sessions
            End Get

            Private Set(ByVal value As List(Of SessionItem))
                _Sessions = value
            End Set
        End Property

        Public Sub New()
        End Sub

        Public Sub New(ByVal benchmarks As Dictionary(Of Integer, List(Of BenchmarkItem)), ByVal traceItems As Dictionary(Of Integer, Dictionary(Of Integer, List(Of TraceItem))), ByVal sessions As IEnumerable(Of SessionItem))
            Me.Benchmarks = benchmarks.Values.SelectMany(Function(x) x).ToList()
            Me.TraceItems = traceItems.Values.SelectMany(Function(dict) dict.Values.SelectMany(Function(x) x)).ToList()
            Me.Sessions.AddRange(sessions)
        End Sub

        Public Function GetBenchmarks() As Dictionary(Of Integer, List(Of BenchmarkItem))
            Dim benchmarksBySessionId As Dictionary(Of Integer, List(Of BenchmarkItem)) = New Dictionary(Of Integer, List(Of BenchmarkItem))()
            For Each session In Sessions
                benchmarksBySessionId.Add(session.ID, New List(Of BenchmarkItem)())
            Next

            Dim benchmarkItems As List(Of BenchmarkItem) = Nothing
            Benchmarks.ForEach(Sub(benchmark)
                Dim sessionId As Integer = benchmark.SessionId
                Dim benchmarkItems As List(Of BenchmarkItem) = Nothing
                If Not benchmarksBySessionId.TryGetValue(sessionId, benchmarkItems) Then
                    benchmarkItems = New List(Of BenchmarkItem)()
                    benchmarksBySessionId.Add(sessionId, benchmarkItems)
                End If

                benchmarkItems.Add(benchmark)
            End Sub)
            Return benchmarksBySessionId
        End Function

        Public Function GetTraceItems() As Dictionary(Of Integer, Dictionary(Of Integer, List(Of TraceItem)))
            Dim traceItemsBySessionId As Dictionary(Of Integer, Dictionary(Of Integer, List(Of TraceItem))) = New Dictionary(Of Integer, Dictionary(Of Integer, List(Of TraceItem)))()
            For Each session In Sessions
                traceItemsBySessionId.Add(session.ID, New Dictionary(Of Integer, List(Of TraceItem))())
            Next

            Dim traceItemsByThreadId As Dictionary(Of Integer, List(Of TraceItem)) = Nothing, traceItems As List(Of TraceItem) = Nothing
            Me.TraceItems.ForEach(Sub(traceItem)
                Dim sessionId As Integer = traceItem.SessionId
                Dim threadId As Integer = traceItem.ThreadId
                Dim traceItemsByThreadId As Dictionary(Of Integer, List(Of TraceItem)) = Nothing
                If Not traceItemsBySessionId.TryGetValue(sessionId, traceItemsByThreadId) Then
                    traceItemsByThreadId = New Dictionary(Of Integer, List(Of TraceItem))()
                    traceItemsBySessionId.Add(sessionId, traceItemsByThreadId)
                End If

                Dim traceItems As List(Of TraceItem) = Nothing
                If Not traceItemsByThreadId.TryGetValue(threadId, traceItems) Then
                    traceItems = New List(Of TraceItem)()
                    traceItemsByThreadId.Add(threadId, traceItems)
                End If

                traceItems.Add(traceItem)
            End Sub)
            Return traceItemsBySessionId
        End Function
    End Class
End Namespace
