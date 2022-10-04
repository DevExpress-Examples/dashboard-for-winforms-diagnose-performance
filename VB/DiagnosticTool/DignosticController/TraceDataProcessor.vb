Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports DashboardDiagnosticTool.Data
Imports DiagnosticTool.LinuxTools

Namespace DashboardDiagnosticTool

    Public Class TraceDataProcessor

        Friend Class Vertex

            Public ReadOnly Property Item As BenchmarkItem

            Public ReadOnly Property Start As DateTime

            Public ReadOnly Property Children As Dictionary(Of Integer, DashboardDiagnosticTool.Data.BenchmarkItem) = New System.Collections.Generic.Dictionary(Of Integer, DashboardDiagnosticTool.Data.BenchmarkItem)()

            Public ReadOnly Property Order As List(Of Integer) = New System.Collections.Generic.List(Of Integer)()

            Private Counts As System.Collections.Generic.Dictionary(Of Integer, Integer) = New System.Collections.Generic.Dictionary(Of Integer, Integer)()

            Public Sub New(ByVal item As DashboardDiagnosticTool.Data.BenchmarkItem, ByVal timeStamp As System.DateTime)
                Me.Item = item
                Me.Start = timeStamp
            End Sub

            Public Sub Add(ByVal child As DashboardDiagnosticTool.Data.BenchmarkItem, ByVal methodId As Integer)
                Dim real As DashboardDiagnosticTool.Data.BenchmarkItem
                If Me.Children.TryGetValue(methodId, real) Then
                    Me.Counts(methodId) += 1
                    real.Count += 1
                    real.MSecs += child.MSecs
                    real.[End] = child.[End]
                Else
                    Me.Children.Add(methodId, child)
                    Me.Counts.Add(methodId, 1)
                    Me.Order.Add(methodId)
                End If
            End Sub

            Public Sub Process(ByVal [end] As Integer, ByVal [stop] As System.DateTime)
                Me.Item.MSecs =([stop] - Me.Start).TotalMilliseconds
                Me.Item.[End] = [end]
            End Sub

            Public Function ProcessChildren() As String
                Dim id As String = Me.Item.Name
                For Each index As Integer In Me.Order
                    Me.Item.Children.Add(Me.Children(index))
                    id += "+" & index & "." & Me.Counts(index)
                Next

                Return id
            End Function
        End Class

        Private ReadOnly sessionId As Integer

        Private ReadOnly current As System.Collections.Generic.Dictionary(Of Integer, System.Collections.Generic.Stack(Of DashboardDiagnosticTool.TraceDataProcessor.Vertex)) = New System.Collections.Generic.Dictionary(Of Integer, System.Collections.Generic.Stack(Of DashboardDiagnosticTool.TraceDataProcessor.Vertex))()

        Private ReadOnly methodIds As System.Collections.Generic.Dictionary(Of String, Integer) = New System.Collections.Generic.Dictionary(Of String, Integer)()

        Public ReadOnly Property TraceItems As Dictionary(Of Integer, System.Collections.Generic.List(Of DashboardDiagnosticTool.Data.TraceItem)) = New System.Collections.Generic.Dictionary(Of Integer, System.Collections.Generic.List(Of DashboardDiagnosticTool.Data.TraceItem))()

        Private benchmarksField As System.Collections.Generic.List(Of DashboardDiagnosticTool.Data.BenchmarkItem)

        Public ReadOnly Property Benchmarks As List(Of DashboardDiagnosticTool.Data.BenchmarkItem)
            Get
                If Me.benchmarksField Is Nothing Then
                    Me.benchmarksField = Me.current.Where(Function(element) element.Value.Count = 1).[Select](Function(element)
                        Dim thread As DashboardDiagnosticTool.TraceDataProcessor.Vertex = element.Value.Pop()
                        thread.ProcessChildren()
                        Return thread.Item
                    End Function).ToList()
                End If

                Return Me.benchmarksField
            End Get
        End Property

        Public Sub New(ByVal newSessionId As Integer)
            Me.sessionId = newSessionId
        End Sub

        Private Sub AddItem(ByVal threadID As Integer, ByVal name As String, ByVal timeStamp As System.DateTime)
            Dim item = New DashboardDiagnosticTool.Data.BenchmarkItem(name, Me.sessionId, threadID)
            item.Start = Me.TraceItems(CInt((threadID))).Count
            Me.current(CInt((threadID))).Push(New DashboardDiagnosticTool.TraceDataProcessor.Vertex(item, timeStamp))
        End Sub

        Private Sub Enter(ByVal threadID As Integer, ByVal name As String, ByVal timeStamp As System.DateTime)
            Me.ProcessThread(threadID, timeStamp)
            Me.AddItem(threadID, name, timeStamp)
        End Sub

        Private Sub FixEnter(ByVal name As String, ByVal timeStamp As System.DateTime, ByVal threadId As Integer)
            If Not Me.current.ContainsKey(threadId) OrElse Not Equals(Me.current(CInt((threadId))).Peek().Item.Name, name) Then Me.Enter(threadId, name, timeStamp)
        End Sub

        Public Sub Process(ByVal traceData As DiagnosticTool.LinuxTools.DXTraceEvent)
            Dim timeStamp As System.DateTime = traceData.TimeStamp
            Dim threadId As Integer = traceData.ThreadID
            Dim methodId As Integer = Nothing
            If traceData.EventKind = DiagnosticTool.LinuxTools.EventKind.Enter Then
                Me.Enter(threadId, traceData.Name, timeStamp)
            ElseIf traceData.EventKind = DiagnosticTool.LinuxTools.EventKind.Leave Then
                Me.FixEnter(traceData.Name, timeStamp, threadId)
                Dim last As DashboardDiagnosticTool.TraceDataProcessor.Vertex = Me.current(CInt((threadId))).Pop()
                last.Process(Me.TraceItems(CInt((threadId))).Count, timeStamp)
                Dim id As String = last.ProcessChildren()
                If Not Me.methodIds.TryGetValue(id, methodId) Then
                    methodId = Me.methodIds.Count
                    Me.methodIds.Add(id, methodId)
                End If

                Me.current(CInt((threadId))).Peek().Add(last.Item, methodId)
                Me.ProcessThread(threadId, timeStamp)
            ElseIf traceData.EventKind = DiagnosticTool.LinuxTools.EventKind.Trace Then
                Me.ProcessThread(threadId, timeStamp)
                Me.TraceItems(CInt((threadId))).Add(New DashboardDiagnosticTool.Data.TraceItem(Me.sessionId, threadId) With {.EventType = traceData.EventType, .Data = traceData.Name})
            End If
        End Sub

        Private Sub ProcessThread(ByVal threadID As Integer, ByVal timeStamp As System.DateTime)
            Dim threadName As String = $"Thread{threadID}"
            Dim items As System.Collections.Generic.Stack(Of DashboardDiagnosticTool.TraceDataProcessor.Vertex) = Nothing
            If Me.current.TryGetValue(threadID, items) Then
                If items.Count = 1 Then items.Peek().Process(Me.TraceItems(CInt((threadID))).Count, timeStamp)
            Else
                Me.TraceItems.Add(threadID, New System.Collections.Generic.List(Of DashboardDiagnosticTool.Data.TraceItem)())
                Me.current.Add(threadID, New System.Collections.Generic.Stack(Of DashboardDiagnosticTool.TraceDataProcessor.Vertex)())
                Me.AddItem(threadID, threadName, timeStamp)
            End If
        End Sub
    End Class
End Namespace
