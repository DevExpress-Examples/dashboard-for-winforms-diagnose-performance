namespace DashboardDiagnosticTool {
    public interface IFileController {
        bool TryOpenFile(out string openName, string fileName = "");
        bool TrySaveFile(out string outFileName, string fileName = "");
    }
}
