using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KGWin.WorkerProcess.Models
{
    /// <summary>
    /// TenantConfig
    /// </summary>
    public class TenantConfig
    {
        public string TenantName { get; set; }
        public string AuthUrl { get; set; }
        public string FileListUrl { get; set; }
        public  string NewFileListUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
