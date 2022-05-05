
using DashboardDiagnosticTool;
using DevExpress.DashboardWin;
using DevExpress.Utils.Svg;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using System.Windows.Forms;

namespace DashboardDiagnostis {
    public partial class DesignerForm1 : DevExpress.XtraBars.Ribbon.RibbonForm {
        BarCheckItem barItem;
        DiagnosticController controller = new DiagnosticController(new FileController());
        public DesignerForm1() {
            InitializeComponent();
            dashboardDesigner.CreateRibbon();
            dashboardDesigner.LoadDashboard(@"Dashboards\dashboard1.xml");
            RibbonControl ribbon = dashboardDesigner.Ribbon;
            RibbonPage page = ribbon.GetDashboardRibbonPage(DashboardBarItemCategory.None, DashboardRibbonPage.Home);
            RibbonPageGroup group = page.GetGroupByName("Performance Diagnostics");
            if (group == null) {
                group = new RibbonPageGroup("Performance Diagnostics") { Name = "Performance Diagnostics" };
                group.AllowTextClipping = false;
                page.Groups.Add(group);
            }
            barItem = AddBarItem("Inspect", svgImageCollection1["inspect"]);
            group.ItemLinks.Add(barItem);
            barItem.ItemClick += barItem_itemclick;
        }
        BarCheckItem AddBarItem(string caption, SvgImage svgImage) {
            BarCheckItem barItem = new BarCheckItem();
            barItem.Caption = caption;
            barItem.Name = "Inspect";
            barItem.ImageOptions.SvgImage = svgImage;
            return barItem;
        }
        private void barItem_itemclick(object sender, ItemClickEventArgs e) {
            foreach (BarItem item in dashboardDesigner.Ribbon.Items) {
                if (item.Name == "Inspect") {
                    updateButton();
                }
            }
        }
        void updateButton() { 
            if (barItem.Checked == true) {
                controller.Start();
            }
            else{
                controller.Stop();
                controller.Save();
                MessageBox.Show("Diagnostic is complete");
            }
        }
    }
    public class FileController : IFileController {
        public bool TryOpenFile(out string openName, string fileName = "") {
            openName = "";
            return false;
        }
        public bool TrySaveFile(out string outFileName, string fileName = "") {
            outFileName = @"..\..\Logs\PerformanceLogs.xml";
            return true;
        }
    }
}
