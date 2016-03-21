using ACastShared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
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
    public class FeedManager : Windows.ApplicationModel.Background.IBackgroundTask
    {
        private CancellationTokenSource cts;
        private List<DownloadOperation> activeDownloads;
        private List<FeedDownload> activeFeedItemDownloads;
        private List<Feed> feeds = new List<Feed>();
                
        public static FeedManager Instance = new FeedManager();

        public Feed CurrentFeed;

        public FeedManager()
        {
            cts = new CancellationTokenSource();

            activeDownloads = new List<DownloadOperation>();
            activeFeedItemDownloads = new List<FeedDownload>();
                        
            CurrentFeed = new Feed();
        }

        public IReadOnlyList<Feed> Feeds
        {
            get { return feeds; }
        }

        public async void ActivateFeedAsync(int feedIdx, SendOrPostCallback feedActivatedAsync)
        {
            CurrentFeed = feeds[feedIdx];

            await CurrentFeed.Activate();

            if (feedActivatedAsync != null)
            {
                feedActivatedAsync(this);
            }
        }             

        public async void AddFeed(string stringUri, SendOrPostCallback addFeedCompletedAsync)
        {
            Uri uri;
            if (!Uri.TryCreate(stringUri.Trim(), UriKind.Absolute, out uri))
            {
                //rootPage.NotifyUser("Error: Invalid URI.", NotifyType.ErrorMessage);
                return;
            }

            var existingItem = from item in feeds where item.Uri.CompareTo(uri.ToString()) == 0 select item;

            if (existingItem.Count() == 0)
            {              

                try
                {
                    string newId = Guid.NewGuid().ToString();
                    Feed feed = new Feed() { Id = newId, Uri = uri.ToString(), FileName = newId + ".dat", ItemsFilename = newId + "_items.dat" };

                    SyndicationFeed sfeed = new SyndicationFeed();
                    await UpdateFeedAsync(uri.ToString(), feed, false);

                    if (feed.ImageUri != null)
                    {
                        //feed.ImageUri = ApplicationData.Current.LocalFolder.Path + "\\" + feed.FileName + ".img";
                        await DownloadFile(feed.ImageUri.ToString(), ApplicationData.Current.LocalFolder, feed.FileName + ".img");
                    }

                    feeds.Add(feed);

                    SerializeFeeds();
                }
                catch /*(Exception)*/
                {
                    
                    //throw;
                }
                
            }

            if (addFeedCompletedAsync != null)
            {
                addFeedCompletedAsync(this);
            }
        }

        public async void RemoveFeed(int feedIdx)
        {
            if(feedIdx >= 0 && feedIdx < feeds.Count) {
                await feeds[feedIdx].Delete();
            }

            SerializeFeeds();
        }

        public async void DeserializeFeedsAsync(SendOrPostCallback feedListLoadedAsync)
        {
            //todo:20160321
            //if (await FileExtensions.FileExist2(ApplicationData.Current.LocalFolder, "FeedList.dat"))
            //{
            //    StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("FeedList.dat");

            //    var deserializeStream = await file.OpenStreamForReadAsync();
            //    XmlSerializer deserializer = new XmlSerializer(typeof(List<Feed>));
            //    feeds = (List<Feed>)deserializer.Deserialize(deserializeStream);
            //    deserializeStream.Dispose();
            //}

            //if (feedListLoadedAsync != null)
            //{
            //    feedListLoadedAsync(this);
            //}
        }

        public async void SerializeFeeds()
        {

            try
            {
                var serializeStream = await ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync("FeedList.dat", CreationCollisionOption.ReplaceExisting);
                XmlSerializer serializer = new XmlSerializer(typeof(List<Feed>));
                serializer.Serialize(serializeStream, feeds);
                serializeStream.Flush();
                serializeStream.Dispose();
            }
            catch /*(Exception)*/
            {

                //throw;
            }
        } 

        public async void StartDownloadMedia(FeedItem feedItem) {
            feedItem.FileName = Guid.NewGuid() + ".mp3";

            //StorageFolder externalDevices = Windows.Storage.KnownFolders.RemovableDevices;

            //IReadOnlyList<StorageFolder> subfolders = await externalDevices.GetFoldersAsync();
            //IReadOnlyList<StorageFolder> subfolders1 = await subfolders[0].GetFoldersAsync();

            //feedItem.Path = subfolders1[4].Path;

            //await DownloadFile(feedItem.Uri, subfolders1[4] /*ApplicationData.Current.LocalFolder*/, feedItem.FileName);

            feedItem.Path = ApplicationData.Current.LocalFolder.Path;

            await DownloadFile(feedItem.Uri, ApplicationData.Current.LocalFolder, feedItem.FileName);

            var feeds = from item in this.feeds where item.Id.CompareTo(feedItem.ParentId) == 0 select item;

            if (feeds.Count() > 0)
            {
                Feed feed = feeds.First();

                feed.MediaDownloadCount++;

                var feedItems = from item in CurrentFeed.Items where item.Id.CompareTo(feedItem.Id) == 0 select item;
                if (feedItems.Count() > 0)
                {
                    FeedItem item = feedItems.First();
                    item.SetState(FeedDownloadState.DownloadCompleted);
                    item.FileName = feedItem.FileName;
                    item.Path = feedItem.Path;
                }               
                
                CurrentFeed.Serialize();
                SerializeFeeds();
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


        public async Task UpdateFeedAsync(string stringUri, Feed feed, bool serializeFeedList)
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

            //feed.LastUpdateDate = new DateTimeOffset(2016, 3, 8, 0, 0, 0, TimeSpan.Zero);
            //var newSyndicationItems = from item in syndicationFeed.Items where item.PublishedDate < feed.LastUpdateDate select item;


            if(feed.Items.Count > 0)
            {
                foreach (var syndicationItem in newSyndicationItems)
                {
                    feed.Items.Insert(0, new FeedItem(feed.Id, syndicationItem));
                }
            } else
            {
                foreach (var syndicationItem in newSyndicationItems)
                {
                    feed.Items.Add(new FeedItem(feed.Id, syndicationItem));
                }
            }

            feed.Serialize();

            if (serializeFeedList)
            {
                SerializeFeeds();
            }            

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

            var feedItems = from item in CurrentFeed.Items where item.Uri.CompareTo(download.RequestedUri.ToString()) == 0 select item;

            foreach (var item in feedItems)
            {
                item.SetProgress(percent);
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

        public void Run(Windows.ApplicationModel.Background.IBackgroundTaskInstance taskInstance)
        {
            //throw new NotImplementedException();
        }
    }

    public enum NotifyType
    {
        StatusMessage,
        ErrorMessage
    };

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

    public class Feed : INotifyPropertyChanged
    {
        private int mediaDownloadCount;

        public Feed()
        {
            Title = string.Empty;
            Id = string.Empty;
            Title = string.Empty;
            Uri = string.Empty;
            FileName = string.Empty;
            ImageUri = string.Empty;
            ItemsFilename = string.Empty; // "FeedItems_" + Guid.NewGuid().ToString() +".dat";
            MediaDownloadCount = 0;
            Items = new FeedItems();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [XmlIgnore]
        public FeedItems Items;

        public string Id;
        public string Title;
        public string Uri;
        public string FileName;
        public string ImageUri;
        public string ItemsFilename;

        [XmlElement("lastUpdatedTime")]
        public string lastUpdatedTimeForXml // format: 2011-11-11T15:05:46.4733406+01:00
        {
            get { return LastUpdateDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz"); }
            set { LastUpdateDate = DateTimeOffset.Parse(value); }
        }

        [XmlIgnore]
        public DateTimeOffset LastUpdateDate;

        public int MediaDownloadCount {
            get { return mediaDownloadCount; }
            set {
                mediaDownloadCount = value;
                onPropertyChanged("MediaDownloadCount");
            }
        }

        public override string ToString()
        {
            return Uri;
        }

        public async Task Activate()
        {
           await Items.Deserialize(ItemsFilename);
        }

        public async void Serialize()
        {
            await Items.Serialize(ItemsFilename);
        }

        public async Task Delete()
        {
            await Items.Deserialize(ItemsFilename);
            await Items.DeleteMediaFiles();

            //string path = Path.GetDirectoryName(ItemsFilename);
            string path = ApplicationData.Current.LocalFolder.Path;
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(path);
            StorageFile file = await folder.GetFileAsync(ItemsFilename);
            await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
        }

        private void onPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class FeedItems : List<FeedItem>
    {
        public async Task Serialize(string fileName)
        {
            try
            {
                var serializeStream = await ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync(fileName, CreationCollisionOption.ReplaceExisting);
                XmlSerializer serializer = new XmlSerializer(typeof(FeedItems));
                serializer.Serialize(serializeStream, this);
                serializeStream.Flush();
                serializeStream.Dispose();
            }
            catch /*(Exception)*/
            {
                //throw;
            }
        }

        public async Task Deserialize(string fileName)
        {
            //todo:20160321
            //try
            //{
            //    if (await FileExtensions.FileExist2(ApplicationData.Current.LocalFolder, fileName))
            //    {
            //        StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);

            //        var deserializeStream = await file.OpenStreamForReadAsync();
            //        XmlSerializer deserializer = new XmlSerializer(typeof(FeedItems));
            //        FeedItems feedItems = (FeedItems)deserializer.Deserialize(deserializeStream);

            //        this.AddRange(feedItems);

            //        deserializeStream.Dispose();
            //    }
            //}
            //catch /*(Exception)*/
            //{
            //    //throw;
            //}
        }

        public async Task DeleteMediaFiles()
        {
            try
            {
                foreach (var item in this)
                {
                    await item.DeleteMediaFile();
                }
            }
            catch /*(Exception)*/
            {
                //throw;
            }
        }
                
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
        public string Path;
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

        public async Task DeleteMediaFile()
        {
            if (!string.IsNullOrEmpty(Path))
            {
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(Path);
                StorageFile file = await folder.GetFileAsync(FileName);
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                DownloadState = FeedDownloadState.None;
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

    
}
