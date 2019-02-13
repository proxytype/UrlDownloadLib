using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrlDownloadLib.Model
{
    public class UrlDownloadSettings
    {
        public const int DEFAULT_BUFFER_SIZE = 1024;
        public const int DEFAULT_TIMER_INTERVAL = 1000;
        public const int DEFAULT_TIMEOUT = 30000;
        public const bool DEFAULT_AUTO_RESUME = true;
        
        public string downloadDirectory = String.Empty;
        public int downloadBufferSize = DEFAULT_BUFFER_SIZE;
        public int downloadReportInterval = DEFAULT_TIMER_INTERVAL;
        public int downloadTimeout = DEFAULT_TIMEOUT;
        public bool downloadAutoResume = DEFAULT_AUTO_RESUME;
    }
}
