using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UrlDownloadLib.Model;

namespace UrlDownloadLib.Logic
{
    public class DownloadWorker
    {
        //required headers
        public const string HEADER_ACCEPT_RANGE = "Accept-Ranges";
        public const string HEADER_CONTENT_DISPOSITION = "Content-Disposition";
        public const string HEADER_LAST_MODIFIED = "Last-Modified";

        //required values
        public const string CONTENT_DISPOSITION_FILENAME = "filename";
        public const string ACCEPT_RANGE = "bytes";

        //public event share data with others
        public delegate void WorkerProgress(UrlDownloadFile file);
        public event WorkerProgress WorkerProgressEvent;

        public delegate void PartProgress(UrlDownloadPart part);
        public event PartProgress PartProgressEvent;

        public bool isRunning { get; set; }

        //local members
        private BackgroundWorker worker;
        private UrlDownloadFile downloaderFile;
        private UrlDownloadSettings settings = null;

        public DownloadWorker(UrlDownloadSettings _settings)
        {
            settings = _settings;

            worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

        }

        public void executeWorker(UrlDownloadFile _downloadeFile)
        {
            //protect from over lapping
            if (!isRunning)
            {
                isRunning = true;
                downloaderFile = _downloadeFile;
                worker.RunWorkerAsync();
            }
        }

        public void suspendWorker(URL_DOWNLOAD_EVENT_STATUS dEvent)
        {
            downloaderFile.downloaderEvent = dEvent;
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (isRunning)
            {
                isRunning = false;
            }

        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {

            try
            {
                //send event to ui, download start
                downloaderFile.downloaderEvent = URL_DOWNLOAD_EVENT_STATUS.START;
                WorkerProgressEvent(downloaderFile);

                //doing preflight before start downloading
                preFlight();

                //send event to ui, dowloading...
                downloaderFile.downloaderEvent = URL_DOWNLOAD_EVENT_STATUS.DOWNLOADING;
                WorkerProgressEvent(downloaderFile);

                long mod = downloaderFile.size % downloaderFile.numberOfWorkers;
                long division = (int)(downloaderFile.size / downloaderFile.numberOfWorkers);

                downloaderFile.tempDirectory = Path.Combine(settings.downloadDirectory, Guid.NewGuid().ToString());
                Directory.CreateDirectory(downloaderFile.tempDirectory);

                Parallel.For(0, downloaderFile.numberOfWorkers, index => {

                    long startIndex = index * division;
                    long endIndex = startIndex -1 + division;

                    if (index == downloaderFile.numberOfWorkers - 1)
                    {
                        endIndex = endIndex + mod;
                    }

                    downloadFile(startIndex, endIndex, index);
                });

                string[] files = Directory.GetFiles(downloaderFile.tempDirectory);

                if (files.Length > 0)
                {
                    FilesMerge filesMerge = new FilesMerge(settings);
                    string savePath = Path.Combine(settings.downloadDirectory, downloaderFile.localFilename);

                    filesMerge.mergeParts(files, savePath);

                    Directory.Delete(downloaderFile.tempDirectory, true);

                }

            }
            catch (Exception ex)
            {
                //handle exception
                downloaderFile.exception = ex.ToString();
                downloaderFile.downloaderEvent = URL_DOWNLOAD_EVENT_STATUS.ERROR;
            }
            finally
            {
               
                WorkerProgressEvent(downloaderFile);
            }
        }

        private void preFlight()
        {
            //before starting download getting information about the file
            HttpWebRequest request = null;
            WebResponse response = null;
            try
            {
                request = WebRequest.CreateHttp(downloaderFile.url);
                response = request.GetResponse();

                //create temp filename
                string[] spliter = downloaderFile.url.Split('/');
                downloaderFile.localFilename = spliter[spliter.Length - 1];

                //checking if header exists
                if (response.Headers[HEADER_CONTENT_DISPOSITION] != null)
                {
                    //try to export the name inside the header
                    string[] splitter = response.Headers[HEADER_CONTENT_DISPOSITION].Split('=');
                    if (splitter.Length > 1)
                    {
                        downloaderFile.localFilename = splitter[splitter.Length - 1];
                    }
                }


                //check if server support range
                if (response.Headers[HEADER_ACCEPT_RANGE].ToString() == ACCEPT_RANGE)
                {
                    downloaderFile.isRangable = true;
                }
                else {
                    //not accept range, one worker only
                    downloaderFile.numberOfWorkers = 1;
                }

                //getting the file size
                downloaderFile.size = response.ContentLength;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

                if (response != null)
                {
                    response.Close();
                    response.Dispose();
                }
            }
        }

        private void downloadFile(long startIndex, long endIndex, int workerIndex) {

            UrlDownloadPart downloadPart = new UrlDownloadPart();

            downloadPart.workerIndex = workerIndex;
            downloadPart.startIndex = startIndex;
            downloadPart.currentIndex = startIndex;
            downloadPart.endIndex = endIndex;

            downloadPart.partFileName = Path.Combine(new string[] { downloaderFile.tempDirectory, downloadPart.workerIndex.ToString() + "_" + Guid.NewGuid() });

            int bytesRead = 0;
      
            Stream remoteStream = null;
            Stream localStream = null;
            WebResponse response = null;

            downloadPart.downloaderEvent = URL_DOWNLOAD_EVENT_STATUS.DOWNLOADING;
            PartProgressEvent(downloadPart);

            try
            {
                HttpWebRequest request = WebRequest.CreateHttp(downloaderFile.url);

                localStream = File.Create(downloadPart.partFileName);
               
                request.AddRange((int)downloadPart.startIndex, downloadPart.endIndex);
                response = request.GetResponse();
                remoteStream = response.GetResponseStream();

                byte[] buffer = new byte[settings.downloadBufferSize];

                do
                {
                    bytesRead = remoteStream.Read(buffer, 0, buffer.Length);

                    //write new data to local stream
                    localStream.Write(buffer, 0, bytesRead);

                    downloadPart.currentIndex += bytesRead;

                    PartProgressEvent(downloadPart);

                    //if download complete or aborted by the user, exit do
                    if (downloadPart.currentIndex >= endIndex || downloaderFile.downloaderEvent == URL_DOWNLOAD_EVENT_STATUS.STOP 
                        )
                    {
                        if (downloadPart.currentIndex >= endIndex)
                        {
                            downloaderFile.downloaderEvent = URL_DOWNLOAD_EVENT_STATUS.COMPLETE;
                        }
                        break;
                    }

                   System.Threading.Thread.Sleep(1);


                } while (true);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //cleaning everything...
                if (response != null) { response.Close(); }
                if (remoteStream != null) { remoteStream.Close(); }
                if (localStream != null) { localStream.Close(); }

             
            }

            PartProgressEvent(downloadPart);
        }

    }
}