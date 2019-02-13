using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrlDownloadLib.Model
{
    public class UrlDownloadPart
    {
        public int workerIndex { get; set; }
        public double size { get; set; }
        public string partFileName { get; set; }
        public URL_DOWNLOAD_EVENT_STATUS downloaderEvent;
        public long startIndex { get; set; }
        public long currentIndex { get; set; }
        public long endIndex { get; set; }
        public string exception { get; set; }
    }
}
