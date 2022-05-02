Imports DashboardDiagnosticTool
Imports DevExpress.DashboardWin
Imports DevExpress.Utils.Svg
Imports DevExpress.XtraBars
Imports DevExpress.XtraBars.Ribbon
Imports System.Windows.Forms

Namespace DashboardDiagnostis
	Partial Public Class DesignerForm1
		Inherits DevExpress.XtraBars.Ribbon.RibbonForm

		Private barItem As BarCheckItem
		Private controller As New DiagnosticController(New FileController())
		Public Sub New()
			InitializeComponent()
			dashboardDesigner.CreateRibbon()
			dashboardDesigner.LoadDashboard("Dashboards\dashboard1.xml")
			Dim ribbon_ As RibbonControl = dashboardDesigner.Ribbon
			Dim page As RibbonPage = ribbon_.GetDashboardRibbonPage(DashboardBarItemCategory.None, DashboardRibbonPage.Home)
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
			Dim barItem As New BarCheckItem()
			barItem.Caption = caption
			barItem.Name = "Inspect"
			barItem.ImageOptions.SvgImage = svgImage
			Return barItem
		End Function
		Private Sub barItem_itemclick(ByVal sender As Object, ByVal e As ItemClickEventArgs)
			For Each item As BarItem In dashboardDesigner.Ribbon.Items
				If item.Name = "Inspect" Then
					updateButton()
				End If
			Next item
		End Sub
		Private Sub updateButton()
			If barItem.Checked = True Then
				controller.Start()
			Else
				controller.Stop()
				controller.Save()
				MessageBox.Show("Diagnostic is over")
			End If
		End Sub
	End Class
	Public Class FileController
		Implements IFileController

		Public Function TryOpenFile(<System.Runtime.InteropServices.Out()> ByRef openName As String, Optional ByVal fileName As String = "") As Boolean Implements IFileController.TryOpenFile
			openName = ""
			Return False
		End Function
		Public Function TrySaveFile(<System.Runtime.InteropServices.Out()> ByRef outFileName As String, Optional ByVal fileName As String = "") As Boolean Implements IFileController.TrySaveFile
			outFileName = "..\..\Logs\PerformanceLogs.xml"
			Return True
		End Function
	End Class
End Namespace
