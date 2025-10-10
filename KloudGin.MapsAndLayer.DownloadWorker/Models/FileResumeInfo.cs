using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KGWin.WorkerProcess.Models
{
    /// <summary>
    /// FileResumeInfo
    /// </summary>
    public class FileResumeInfo
    {
        public string ETag { get; set; }
        public long BytesDownloaded { get; set; }
    }
}
