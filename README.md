# UrlDownloadLib

Simple HTTP/S file download library supporting multi parts connections.

## Installation
Download from nuget or build from source, this package designed using .Net framework 4.6.1 but you can change to any framework you need.

## Usage
```
UrlDownloadSettings settings = new UrlDownloadSettings()
{
    //download directory
    downloadDirectory = @"E:\UrlDownloaderCache\",
};


//new file instnace, set number of workers
UrlDownloadFile file = new UrlDownloadFile("<LinkToFile>") { numberOfWorkers = 1};

//signup to reports events
DownloadWorker worker = new DownloadWorker(settings);
worker.PartProgressEvent += Worker_PartProgressEvent;
worker.WorkerProgressEvent += Worker_WorkerProgressEvent;

//start downloading
worker.executeWorker(file);

//dont kill the application
while (worker.isRunning) {
    Thread.Sleep(1000);
}
```
