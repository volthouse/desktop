using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.Web;
using Windows.Web.Syndication;

namespace ACast
{
    public delegate void DownloadProgressHandler(float percent);
    public delegate void DownloadCompletedHandler();
    public delegate void FeedUpdateCompletedHandler();
    public delegate void FeedDeserializeCompletedHandler();
    public delegate void AddFeedCompletedHandler();
    public delegate void FeedListLoadedHandler();
    public delegate void FeedActivatedHandler();
    public delegate void FeedListDeletedHandler();

    public class FeedManager : IBackgroundTask
    {
        private CancellationTokenSource cts;

        private List<DownloadOperation> activeDownloads;
        private List<FeedDownload> activeFeedItemDownloads;
        
        private List<Feed> feedList = new List<Feed>();
                
        public DownloadProgressHandler DownloadProgressAsync;
        public DownloadCompletedHandler DownloadCompletedAsync;
        public FeedUpdateCompletedHandler FeedUpdateCompletedAsync;
        public FeedDeserializeCompletedHandler FeedDeserializeCompletedAsync;
        public AddFeedCompletedHandler AddFeedCompletedAsync;
        public FeedListLoadedHandler FeedListLoadedAsync;
        public FeedActivatedHandler FeedActivatedAsync;
        public FeedListDeletedHandler FeedListDeletedAsync;
        
        public static FeedManager Instance = new FeedManager();

        public List<FeedItem> CurrentFeedItems;
        public Feed CurrentFeed;

        public FeedManager()
        {
            cts = new CancellationTokenSource();

            activeDownloads = new List<DownloadOperation>();
            activeFeedItemDownloads = new List<FeedDownload>();
                        
            CurrentFeed = new Feed();
            CurrentFeedItems = new List<FeedItem>();                        
        }

        public IReadOnlyList<Feed> FeedList
        {
            get { return feedList; }
        }

        public async void ActiveFeedAsync(int feedIdx)
        {
            CurrentFeed = feedList[feedIdx];
            CurrentFeedItems = await LoadFeedItemsAsync(CurrentFeed.ItemsFilename);

            if (FeedActivatedAsync != null)
            {
                FeedActivatedAsync();
            }
        }

        public void DeactiveCurrentFeed()
        {
            if (!string.IsNullOrEmpty(CurrentFeed.ItemsFilename))
            {
                SerializeFeedItems(CurrentFeed.ItemsFilename, CurrentFeedItems);
            }            

            CurrentFeed = new Feed();
            CurrentFeedItems = new List<FeedItem>();           
        }

        public async void LoadFeedListAsync()
        {
            if (await FileExtensions.FileExist2(ApplicationData.Current.LocalFolder, "FeedList.dat"))
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("FeedList.dat");

                var deserializeStream = await file.OpenStreamForReadAsync();
                XmlSerializer deserializer = new XmlSerializer(typeof(List<Feed>));
                feedList = (List<Feed>)deserializer.Deserialize(deserializeStream);
                deserializeStream.Dispose();
            }

            if (FeedListLoadedAsync != null)
            {
                FeedListLoadedAsync();
            }
        }

        public async Task<List<FeedItem>> LoadFeedItemsAsync(string fileName)
        {
            List<FeedItem> feedItems = new List<FeedItem>();

            if (await FileExtensions.FileExist2(ApplicationData.Current.LocalFolder, fileName))
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);

                var deserializeStream = await file.OpenStreamForReadAsync();
                XmlSerializer deserializer = new XmlSerializer(typeof(List<FeedItem>));
                feedItems = (List<FeedItem>)deserializer.Deserialize(deserializeStream);
                deserializeStream.Dispose();
            }

            return feedItems;
        }

        public async void DeleteFeedList()
        {
            if (await FileExtensions.FileExist2(ApplicationData.Current.LocalFolder, "FeedList.dat"))
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("FeedList.dat");

                await file.DeleteAsync();

                // TODO: delete feed files

                feedList.Clear();
            }

            if (FeedListDeletedAsync != null)
            {
                FeedListDeletedAsync();
            }
        }

        public async void AddFeed(string stringUri)
        {
            Uri uri;
            if (!Uri.TryCreate(stringUri.Trim(), UriKind.Absolute, out uri))
            {
                //rootPage.NotifyUser("Error: Invalid URI.", NotifyType.ErrorMessage);
                return;
            }

            var existingItem = from item in feedList where item.Uri.CompareTo(uri.ToString()) == 0 select item;

            if (existingItem.Count() == 0)
            {              

                try
                {
                    string newId = Guid.NewGuid().ToString();
                    Feed feed = new Feed() { Id = newId, Uri = uri.ToString(), FileName = newId + ".dat", ItemsFilename = newId + "_items.dat" };

                    SyndicationFeed sfeed = new SyndicationFeed();
                    await UpdateFeedAsync(uri.ToString(), feed);

                    if (feed.ImageUri != null)
                    {
                        //feed.ImageUri = ApplicationData.Current.LocalFolder.Path + "\\" + feed.FileName + ".img";
                        await DownloadFile(feed.ImageUri.ToString(), ApplicationData.Current.LocalFolder, feed.FileName + ".img");
                    }

                    feedList.Add(feed);

                    SerializeFeedList();
                }
                catch /*(Exception)*/
                {
                    
                    //throw;
                }
                
            }

            if (AddFeedCompletedAsync != null)
            {
                AddFeedCompletedAsync();
            }
        }

        public async void SerializeFeedList()
        {

            try
            {
                var serializeStream = await ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync("FeedList.dat", CreationCollisionOption.ReplaceExisting);
                XmlSerializer serializer = new XmlSerializer(typeof(List<Feed>));
                serializer.Serialize(serializeStream, feedList);
                serializeStream.Flush();
                serializeStream.Dispose();
            }
            catch /*(Exception)*/
            {

                //throw;
            }          
        }

        public async void SerializeFeedItems(string fileName, List<FeedItem> items)
        {

            try
            {
                var serializeStream = await ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync(fileName, CreationCollisionOption.ReplaceExisting);
                XmlSerializer serializer = new XmlSerializer(typeof(List<FeedItem>));
                serializer.Serialize(serializeStream, items);
                serializeStream.Flush();
                serializeStream.Dispose();
            }
            catch /*(Exception)*/
            {

                //throw;
            }
        }

        public async void LoadFeed(int i, SyndicationFeed feed)
        {
            if (feedList.Count == 0)
            {
                return;
            }

            Feed infoItem = feedList[i];

            await DeserializeFeed(feed, infoItem.FileName);
        }

        public async void StartDownloadMedia(FeedItem feedItem) {
            feedItem.FileName = Guid.NewGuid() + ".mp3";
            await DownloadFile(feedItem.Uri, ApplicationData.Current.LocalFolder, feedItem.FileName);
                        
            var feeds = from item in feedList where item.Id.CompareTo(feedItem.ParentId) == 0 select item;

            if (feeds.Count() > 0)
            {
                Feed feed = feeds.First();

                List<FeedItem> items = await LoadFeedItemsAsync(feed.ItemsFilename);
                var feedItems = from item in items where item.Id.CompareTo(feedItem.Id) == 0 select item;
                if (feedItems.Count() > 0)
                {
                    FeedItem item = feedItems.First();
                    item.DownloadState = FeedDownloadState.DownloadCompleted;
                    item.FileName = feedItem.FileName;
                }

                SerializeFeedItems(feed.ItemsFilename, items);
                
                var currentFeedItems = from item in CurrentFeedItems where item.Id.CompareTo(feedItem.Id) == 0 select item;
                if (currentFeedItems.Count() > 0)
                {
                    FeedItem item = currentFeedItems.First();
                    item.SetState(FeedDownloadState.DownloadCompleted);
                }
            }

        }

        public async Task DownloadFile(string uri, StorageFolder folder, string fileName)
        {

            StorageFile destinationFile;
            try
            {
                destinationFile = await folder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
            }
            catch /*(FileNotFoundException ex)*/
            {
                //rootPage.NotifyUser("Error while creating file: " + ex.Message, NotifyType.ErrorMessage);
                return;
            }

            BackgroundDownloader downloader = new BackgroundDownloader();
            DownloadOperation download = downloader.CreateDownload(new Uri(uri), destinationFile);

            await HandleDownloadAsync(download, true);
        }


        public async Task UpdateFeedAsync(string stringUri, Feed feed)
        {
            SyndicationClient client = new SyndicationClient();

            client.BypassCacheOnRetrieve = true;

            // Although most HTTP servers do not require User-Agent header, others will reject the request or return
            // a different response if this header is missing. Use SetRequestHeader() to add custom headers.
            client.SetRequestHeader("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");


            SyndicationFeed syndicationFeed = await client.RetrieveFeedAsync(new Uri(stringUri));


            feed.Title = syndicationFeed.Title.Text;
            feed.Uri = stringUri;
            if (syndicationFeed.ImageUri != null) {
                feed.ImageUri = syndicationFeed.ImageUri.ToString();
            } else {
                var elementExtensions = from item in syndicationFeed.ElementExtensions where item.NodeName.CompareTo("image") == 0 select item;
                if (elementExtensions.Count() > 0)
                {
                    if (elementExtensions.First().AttributeExtensions.Count > 0)
                    {
                        feed.ImageUri = elementExtensions.First().AttributeExtensions[0].Value;
                    }
                }
            }           

                        
            var newSyndicationItems = from item in syndicationFeed.Items where item.PublishedDate > feed.LastUpdateDate select item;


            List<FeedItem> feedItems = await LoadFeedItemsAsync(feed.ItemsFilename);

            foreach (var syndicationItem in newSyndicationItems)
            {
                feedItems.Add(new FeedItem(feed.Id, syndicationItem));
                
            }

            SerializeFeedItems(feed.ItemsFilename, feedItems);

            feed.LastUpdateDate = DateTimeOffset.Now;

            if (FeedUpdateCompletedAsync != null)
            {
                FeedUpdateCompletedAsync();
            }

            return;
        }

        public async Task SerializeFeedAsync(SyndicationFeed feed, string fileName)
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);

            var doc = feed.GetXmlDocument(feed.SourceFormat);

            await doc.SaveToFileAsync(file);
        }

        public async Task DeserializeFeed(SyndicationFeed feed, string fileName)
        {
            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
                XmlDocument doc = await XmlDocument.LoadFromFileAsync(file);
                feed.LoadFromXml(doc);
            }
            catch (Exception)
            {
                // ignore
            }
            finally
            {
                if (FeedDeserializeCompletedAsync != null)
                {
                    FeedDeserializeCompletedAsync();
                }
            }   
        }
        private AutoResetEvent SererInitialized;
        class Constants
        {
            public const string CurrentTrack = "trackname";
            public const string BackgroundTaskStarted = "BackgroundTaskStarted";
            public const string BackgroundTaskRunning = "BackgroundTaskRunning";
            public const string BackgroundTaskCancelled = "BackgroundTaskCancelled";
            public const string AppSuspended = "appsuspend";
            public const string AppResumed = "appresumed";
            public const string StartPlayback = "startplayback";
            public const string SkipNext = "skipnext";
            public const string Position = "position";
            public const string AppState = "appstate";
            public const string BackgroundTaskState = "backgroundtaskstate";
            public const string SkipPrevious = "skipprevious";
            public const string Trackchanged = "songchanged";
            public const string ForegroundAppActive = "Active";
            public const string ForegroundAppSuspended = "Suspended";
        }

        public void Play(FeedItem feedItem)
        {
            BackgroundMediaPlayer.MessageReceivedFromForeground += BackgroundMediaPlayer_MessageReceivedFromForeground;

            string path = ApplicationData.Current.LocalFolder.Path + @"\" + feedItem.FileName;
            BackgroundMediaPlayer.Current.SetUriSource(new Uri(path));
            BackgroundMediaPlayer.Current.Play();
            

        }

        private void BackgroundMediaPlayer_MessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        void Current_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            
        }

        void Current_CurrentStateChanged(MediaPlayer sender, object args)
        {
            var x = BackgroundMediaPlayer.Current.CurrentState;
        }
                
        private async Task HandleDownloadAsync(DownloadOperation download, bool start)
        {
            try
            {
                LogStatus("Running: " + download.Guid, NotifyType.StatusMessage);

                // Store the download so we can pause/resume.
                activeDownloads.Add(download);

                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(DownloadProgressInternal);
                if (start)
                {
                    // Start the download and attach a progress handler.
                    await download.StartAsync().AsTask(cts.Token, progressCallback);
                }
                else
                {
                    // The download was already running when the application started, re-attach the progress handler.
                    await download.AttachAsync().AsTask(cts.Token, progressCallback);
                }

                ResponseInformation response = download.GetResponseInformation();

                LogStatus(String.Format(CultureInfo.CurrentCulture, "Completed: {0}, Status Code: {1}",
                    download.Guid, response.StatusCode), NotifyType.StatusMessage);
            }
            catch (TaskCanceledException)
            {
                LogStatus("Canceled: " + download.Guid, NotifyType.StatusMessage);
            }
            catch (Exception ex)
            {
                if (!IsExceptionHandled("Execution error", ex, download))
                {
                    throw;
                }
            }
            finally
            {
                activeDownloads.Remove(download);

                if (DownloadCompletedAsync != null)
                {
                    DownloadCompletedAsync();
                }
            }
        }

        private void DownloadProgressInternal(DownloadOperation download)
        {
            MarshalLog(String.Format(CultureInfo.CurrentCulture, "Progress: {0}, Status: {1}", download.Guid,
                download.Progress.Status));

            float percent = 100;
            if (download.Progress.TotalBytesToReceive > 0)
            {
                percent = download.Progress.BytesReceived * 100 / download.Progress.TotalBytesToReceive;
            }

            MarshalLog(String.Format(CultureInfo.CurrentCulture, " - Transfered bytes: {0} of {1}, {2}%",
                download.Progress.BytesReceived, download.Progress.TotalBytesToReceive, percent));

            if (download.Progress.HasRestarted)
            {
                MarshalLog(" - Download restarted");
            }

            if (download.Progress.HasResponseChanged)
            {
                // We've received new response headers from the server.
                MarshalLog(" - Response updated; Header count: " + download.GetResponseInformation().Headers.Count);

                // If you want to stream the response data this is a good time to start.
                // download.GetResultStreamAt(0);
            }

            if (download.Progress.Status == BackgroundTransferStatus.Completed)
            {

            }

            var feedItems = from item in CurrentFeedItems where item.Uri.CompareTo(download.RequestedUri.ToString()) == 0 select item;

            foreach (var item in feedItems)
            {
                item.SetProgress(percent);
            }

            if (DownloadProgressAsync != null)
            {
                DownloadProgressAsync(percent);
            }
        }

        private bool IsExceptionHandled(string title, Exception ex, DownloadOperation download = null)
        {
            WebErrorStatus error = BackgroundTransferError.GetStatus(ex.HResult);
            if (error == WebErrorStatus.Unknown)
            {
                return false;
            }

            if (download == null)
            {
                LogStatus(String.Format(CultureInfo.CurrentCulture, "Error: {0}: {1}", title, error),
                    NotifyType.ErrorMessage);
            }
            else
            {
                LogStatus(String.Format(CultureInfo.CurrentCulture, "Error: {0} - {1}: {2}", download.Guid, title,
                    error), NotifyType.ErrorMessage);
            }

            return true;
        }

        // When operations happen on a background thread we have to marshal UI updates back to the UI thread.
        private void MarshalLog(string value)
        {
            //var ignore = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            //{
            //    Log(value);
            //});
        }

        private void Log(string message)
        {
            //outputField.Text += message + "\r\n";
        }

        private void LogStatus(string message, NotifyType type)
        {
            //rootPage.NotifyUser(message, type);
            //Log(message);
        }

        private BackgroundTaskDeferral _deferral;

        public void Run(Windows.ApplicationModel.Background.IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
             taskInstance.Canceled += TaskInstance_Canceled;
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            _deferral.Complete();
        }
    }

    public enum NotifyType
    {
        StatusMessage,
        ErrorMessage
    };

    public class Feed
    {
        public Feed()
        {
            Title = string.Empty;
            Id = string.Empty;
            Title = string.Empty;
            Uri = string.Empty;
            FileName = string.Empty;
            ImageUri = string.Empty;
            ItemsFilename = string.Empty;
        }

        public string Id;
        public string Title;
        public string Uri;
        public string FileName;
        public string ImageUri;
        public string ItemsFilename;
        public DateTimeOffset LastUpdateDate;

        public override string ToString()
        {
            return Uri;
        }
    }

    public enum FeedDownloadState
    {
        None,
        DownloadStarted,
        DownloadCompleted
    }

    public enum FeedPlayerState
    {
        None,
        PlayerStarted,
        PlayerCompleted
    }

    public class FeedItem
    {
        [XmlIgnore]
        public EventHandler StateChanged;
        [XmlIgnore]
        public EventHandler<float> DownloadProgressChanged;

        public FeedItem() {  }

        public FeedItem(string parentId, SyndicationItem item)
        {
            ParentId = parentId;
            Id = item.Id;
            Title = item.Title.Text;
            Summary = item.Summary.Text;
            PublishedDate = item.PublishedDate;
           
            var x = from l in item.Links where l.MediaType.CompareTo("audio/mpeg") == 0 select l;

            if (x.Count() > 0)
            {
                Uri = x.FirstOrDefault().Uri.ToString();
            }

        }

        public string ParentId;
        public string Id;
        public string FileName;
        public string Uri;
        public FeedDownloadState DownloadState;
        public FeedPlayerState PlayerState;
        public float PlayerPos;
        public string Title;
        public string Summary;
        public DateTimeOffset PublishedDate;

        public void SetState(FeedDownloadState state)
        {
            DownloadState = state;
            if (StateChanged != null)
            {
                StateChanged(this, EventArgs.Empty);
            }
        }

        public void SetProgress(float progress)
        {
            if (DownloadProgressChanged != null)
            {
                DownloadProgressChanged(this, progress);
            }
        }
    }

    public class FeedDownload
    {
        public FeedDownload(DownloadOperation op)
        {

        }
        public string ItemsFilename;
        public int ItemIdx;
    }

    public static class FileExtensions
    {
        public static async Task<bool> FileExists(this StorageFolder folder, string fileName)
        {
            try { StorageFile file = await folder.GetFileAsync(fileName); }
            catch { return false; }
            return true;
        }

        public static async Task<bool> FileExist2(this StorageFolder folder, string fileName)
        { return (await folder.GetFilesAsync()).Any(x => x.Name.Equals(fileName)); }
    }
}
