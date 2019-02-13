using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrlDownloadLib.Model
{
    public enum URL_DOWNLOAD_EVENT_STATUS
    {
        START,
        DOWNLOADING,
        COMPLETE,
        RESUMING,
        STOP,
        PAUSE,
        ERROR
    }
}
