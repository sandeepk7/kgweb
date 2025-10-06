using System.IO;

namespace KGWin.WPF.Services
{
    public class UtilityService
    {
        public static string GetFilePathFromDataFolder(string folderName, string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", folderName, fileName);
        }
    }
}
