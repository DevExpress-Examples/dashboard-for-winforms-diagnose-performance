Imports System.Collections.Generic
Imports System.IO
Imports System.Xml.Serialization

Namespace DashboardDiagnosticTool.Data

    Friend Class BenchmarkHelper

        Public Shared Sub SaveBenchmarkItems(ByVal benchmarks As Dictionary(Of Integer, List(Of BenchmarkItem)), ByVal traceItems As Dictionary(Of Integer, Dictionary(Of Integer, List(Of TraceItem))), ByVal sessions As IEnumerable(Of SessionItem), ByVal stream As Stream)
            Dim serializer = New XmlSerializer(GetType(BenchmarkItemSet))
            serializer.Serialize(stream, New BenchmarkItemSet(benchmarks, traceItems, sessions))
        End Sub

        Public Shared Function LoadBenchmarkSet(ByVal stream As Stream) As BenchmarkItemSet
            Dim serializer = New XmlSerializer(GetType(BenchmarkItemSet), Nothing, Nothing, Nothing, Nothing)
            Return TryCast(serializer.Deserialize(stream), BenchmarkItemSet)
        End Function
    End Class
End Namespace
