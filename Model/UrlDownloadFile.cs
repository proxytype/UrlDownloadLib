using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrlDownloadLib.Model
{
    public class UrlDownloadFile
    {
        public const int DEFAULT_NUMBER_OF_WORKERS = 1;

        public string url { get; set; }
        public long size { get; set; }
        public int numberOfWorkers { get; set; } = DEFAULT_NUMBER_OF_WORKERS;
        public string localFilename { get; set; }
        public URL_DOWNLOAD_EVENT_STATUS downloaderEvent;
        public bool isRangable { get; set; }
        public string exception { get; set; }
        public string tempDirectory { get; set; }

        public UrlDownloadFile(string _url)
        {
            url = _url;
        }
    }
}
