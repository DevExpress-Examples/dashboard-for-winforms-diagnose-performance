using System.IO;

namespace DashboardDiagnosticTool {
    class DefaultFileController : IFileController {
        string fileExtension = ".xml";
        public bool TryOpenFile(out string openName, string fileName = "") {
            if(Path.GetExtension(fileName) == fileExtension) {
                openName = fileName;
                return true;
            }
            openName = "";
            return false;
        }

        public bool TrySaveFile(out string outFileName, string fileName = "") {
            if(Path.GetExtension(fileName) == fileExtension) {
                outFileName = fileName;
                return true;
            }
            outFileName = "";
            return false;
        }
    }
}
