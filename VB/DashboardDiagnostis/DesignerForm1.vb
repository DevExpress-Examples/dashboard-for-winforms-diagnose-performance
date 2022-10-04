Imports DashboardDiagnosticTool
Imports DevExpress.DashboardWin
Imports DevExpress.Utils.Svg
Imports DevExpress.XtraBars
Imports DevExpress.XtraBars.Ribbon
Imports System.Windows.Forms
Imports System.Runtime.InteropServices

Namespace DashboardDiagnostis

    Public Partial Class DesignerForm1
        Inherits RibbonForm

        Private barItem As BarCheckItem

        Private controller As DiagnosticController = New DiagnosticController(New FileController())

        Public Sub New()
            InitializeComponent()
            dashboardDesigner.CreateRibbon()
            dashboardDesigner.LoadDashboard("Dashboards\dashboard1.xml")
            Dim ribbon As RibbonControl = dashboardDesigner.Ribbon
            Dim page As RibbonPage = ribbon.GetDashboardRibbonPage(DashboardBarItemCategory.None, DashboardRibbonPage.Home)
            Dim group As RibbonPageGroup = page.GetGroupByName("Performance Diagnostics")
            If group Is Nothing Then
                group = New RibbonPageGroup("Performance Diagnostics") With {.Name = "Performance Diagnostics"}
                group.AllowTextClipping = False
                page.Groups.Add(group)
            End If

            barItem = AddBarItem("Inspect", svgImageCollection1("inspect"))
            group.ItemLinks.Add(barItem)
            AddHandler barItem.ItemClick, AddressOf barItem_itemclick
        End Sub

        Private Function AddBarItem(ByVal caption As String, ByVal svgImage As SvgImage) As BarCheckItem
            Dim barItem As BarCheckItem = New BarCheckItem()
            barItem.Caption = caption
            barItem.Name = "Inspect"
            barItem.ImageOptions.SvgImage = svgImage
            Return barItem
        End Function

        Private Sub barItem_itemclick(ByVal sender As Object, ByVal e As ItemClickEventArgs)
            For Each item As BarItem In dashboardDesigner.Ribbon.Items
                If Equals(item.Name, "Inspect") Then
                    updateButton()
                End If
            Next
        End Sub

        Private Sub updateButton()
            If barItem.Checked = True Then
                controller.Start()
            Else
                controller.Stop()
                controller.Save()
                MessageBox.Show("Diagnostic is complete")
            End If
        End Sub
    End Class

    Public Class FileController
        Implements IFileController

        Public Function TryOpenFile(<Out> ByRef openName As String, ByVal Optional fileName As String = "") As Boolean Implements IFileController.TryOpenFile
            openName = ""
            Return False
        End Function

        Public Function TrySaveFile(<Out> ByRef outFileName As String, ByVal Optional fileName As String = "") As Boolean Implements IFileController.TrySaveFile
            outFileName = "..\..\Logs\PerformanceLogs.xml"
            Return True
        End Function
    End Class
End Namespace
