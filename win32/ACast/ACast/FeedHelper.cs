using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Data.Xml.Dom;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
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
    public delegate void FeedListDeletedHandler();
    
    public class FeedHelper
    {
        private List<DownloadOperation> activeDownloads;
        private CancellationTokenSource cts;
        private List<FeedInfoItem> feedList = new List<FeedInfoItem>();
                
        public DownloadProgressHandler DownloadProgressAsync;
        public DownloadCompletedHandler DownloadCompletedAsync;
        public FeedUpdateCompletedHandler FeedUpdateCompletedAsync;
        public FeedDeserializeCompletedHandler FeedDeserializeCompletedAsync;
        public AddFeedCompletedHandler AddFeedCompletedAsync;
        public FeedListLoadedHandler FeedListLoadedAsync;
        public FeedListDeletedHandler FeedListDeletedAsync;
        
        public static FeedHelper Instance = new FeedHelper();

        public FeedHelper()
        {
            activeDownloads = new List<DownloadOperation>();
            cts = new CancellationTokenSource();
        }

        public IReadOnlyList<FeedInfoItem> FeedList
        {
            get { return feedList; }
        }

        public async void LoadFeedListAsync()
        {
            if (await FileExtensions.FileExist2(ApplicationData.Current.LocalFolder, "FeedList.dat"))
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("FeedList.dat");

                var deserializeStream = await file.OpenStreamForReadAsync();
                XmlSerializer deserializer = new XmlSerializer(typeof(List<FeedInfoItem>));
                feedList = (List<FeedInfoItem>)deserializer.Deserialize(deserializeStream);
                deserializeStream.Dispose();
            }

            if (FeedListLoadedAsync != null)
            {
                FeedListLoadedAsync();
            }
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
                    FeedInfoItem feedInfo = new FeedInfoItem() { Uri = uri.ToString(), FileName = Guid.NewGuid().ToString() };

                    SyndicationFeed feed = new SyndicationFeed();
                    await UpdateFeedAsync(uri.ToString(), feed);
                    await SerializeFeedAsync(feed, feedInfo.FileName);

                    if (feed.ImageUri != null)
                    {
                        feedInfo.ImageUri = ApplicationData.Current.LocalFolder.Path + "\\" + feedInfo.FileName + ".img";
                        await DownloadFile(feed.ImageUri.ToString(), ApplicationData.Current.LocalFolder, feedInfo.FileName + ".img");
                    }

                    feedList.Add(feedInfo);

                    var serializeStream = await ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync("FeedList.dat", CreationCollisionOption.ReplaceExisting);
                    XmlSerializer serializer = new XmlSerializer(typeof(List<FeedInfoItem>));
                    serializer.Serialize(serializeStream, feedList);
                    serializeStream.Flush();
                    serializeStream.Dispose();
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

        public async void LoadFeed(int i, SyndicationFeed feed)
        {
            if (feedList.Count == 0)
            {
                return;
            }

            FeedInfoItem infoItem = feedList[i];

            await DeserializeFeed(feed, infoItem.FileName);
        }

        public async Task DownloadFile(string uri, StorageFolder folder, string fileName)
        {

            StorageFile destinationFile;
            try
            {
                destinationFile = await folder.CreateFileAsync(
                    fileName, CreationCollisionOption.GenerateUniqueName);
            }
            catch (FileNotFoundException ex)
            {
                //rootPage.NotifyUser("Error while creating file: " + ex.Message, NotifyType.ErrorMessage);
                return;
            }

            BackgroundDownloader downloader = new BackgroundDownloader();
            DownloadOperation download = downloader.CreateDownload(new Uri(uri), destinationFile);

            await HandleDownloadAsync(download, true);
        }

        public async Task UpdateFeedAsync(string stringUri, SyndicationFeed feed)
        {
            SyndicationClient client = new SyndicationClient();

            client.BypassCacheOnRetrieve = true;

            // Although most HTTP servers do not require User-Agent header, others will reject the request or return
            // a different response if this header is missing. Use SetRequestHeader() to add custom headers.
            client.SetRequestHeader("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");


            SyndicationFeed currentFeed = await client.RetrieveFeedAsync(new Uri(stringUri));

            var lastItems = from item in feed.Items orderby item.PublishedDate descending select item;

            var lastItem = lastItems.FirstOrDefault();

            if (lastItem != null)
            {
                var newItems = from item in currentFeed.Items where item.PublishedDate > lastItem.PublishedDate select item;

                foreach (var item in newItems)
                {
                    feed.Items.Add(item);
                }
            }
            else
            {
                feed.LoadFromXml(currentFeed.GetXmlDocument(currentFeed.SourceFormat));
            }
            

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
    }

    public enum NotifyType
    {
        StatusMessage,
        ErrorMessage
    };

    public class FeedInfoItem
    {        
        public string Uri;
        public string FileName;
        public string ImageUri;

        public override string ToString()
        {
            return Uri;
        }
    }

    public enum SyndicationItemState
    {
        None,
        Downloaded,
        Playing,
        Paused,
        Played
    }

    public class SyndicationInfoItem
    {
        public string Uri;
        public string FileName;
        public string Text;
        public SyndicationItemState State;

        public override string ToString()
        {
            return Text;
        }
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
