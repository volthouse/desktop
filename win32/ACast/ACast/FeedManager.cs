using ACast.Database;
using ACastShared;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    public class FeedManager 
    {
        private CancellationTokenSource cts;
        private List<DownloadOperation> activeDownloads;
                
        public static FeedManager Instance = new FeedManager();

        public static int CurrentFeedId = -1;

        public FeedManager()
        {
            cts = new CancellationTokenSource();

            activeDownloads = new List<DownloadOperation>();
        }

        public static IEnumerable<Feed> Feeds {
            get
            {
                return SQLiteDb.GetFeeds();
            }
        }

        public static IEnumerable<FeedItem> FeedItems
        {
            get
            {
                return SQLiteDb.GetFeedItems(CurrentFeedId);
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

            Feed feed = new Feed()
            {
                Uri = uri.ToString(),
                LastUpdateDate = new DateTime(1970, 1, 1)
            };

            SQLiteDb.AddFeed(feed);

            SyndicationFeed sfeed = new SyndicationFeed();
            await UpdateFeedAsync(feed.Uri, feed, false);

            if (addFeedCompletedAsync != null)
            {
                addFeedCompletedAsync(this);
            }
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
            if (syndicationFeed.ImageUri != null)
            {
                feed.ImageUri = syndicationFeed.ImageUri.ToString();
            }
            else
            {
                var elementExtensions = from item in syndicationFeed.ElementExtensions where item.NodeName.CompareTo("image") == 0 select item;
                if (elementExtensions.Any())
                {
                    if (elementExtensions.First().AttributeExtensions.Any())
                    {
                        feed.ImageUri = elementExtensions.First().AttributeExtensions[0].Value;
                    }
                }
            }

            if (feed.ImageUri != null)
            {
                feed.ImageFilename = Guid.NewGuid().ToString() + ".img";
                await DownloadFile(feed.ImageUri.ToString(), ApplicationData.Current.LocalFolder, feed.ImageFilename, null);
            }

            DateTimeOffset offset = new DateTimeOffset(feed.LastUpdateDate);
            var newSyndicationItems = from item in syndicationFeed.Items where item.PublishedDate > offset select item;

            if (newSyndicationItems.Any())
            {
                SQLiteDb.AddFeedItems(feed.Id, newSyndicationItems);
            }

            feed.LastUpdateDate = DateTime.Now;

            SQLiteDb.UpdateFeed(feed);
        }

        public async void DownloadFeedItemMedia(FeedItem feedItem)
        {
            feedItem.FileName = Guid.NewGuid() + ".mp3";
            feedItem.Path = ApplicationData.Current.LocalFolder.Path;

            await DownloadFile(feedItem.MediaUri, ApplicationData.Current.LocalFolder, 
                feedItem.FileName, feedItem.ReportDownloadProgress);

            feedItem.MediaDownloadState = 1;

            SQLiteDb.UpdateFeedItem(feedItem);

        }

        public async Task DownloadFile(string uri, StorageFolder folder, string fileName, Action<object, int> progressDelegate)
        {

            StorageFile destinationFile;
            try
            {
                destinationFile = await folder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
            }
            catch (FileNotFoundException ex)
            {
                MarshalLog("Error while creating file: " + ex.Message);
                return;
            }

            BackgroundDownloader downloader = new BackgroundDownloader();
            DownloadOperation download = downloader.CreateDownload(new Uri(uri), destinationFile);

            await HandleDownloadAsync(download, true, progressDelegate);
        }

        private async Task HandleDownloadAsync(DownloadOperation download, bool start, Action<object, int> progressDelegate)
        {
            try
            {
                MarshalLog("Running: " + download.Guid);

                // Store the download so we can pause/resume.
                activeDownloads.Add(download);

                DownloadProgress progressCallback = new DownloadProgress(progressDelegate, SynchronizationContext.Current);
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

                MarshalLog(String.Format(CultureInfo.CurrentCulture, "Completed: {0}, Status Code: {1}",
                    download.Guid, response.StatusCode));
            }
            catch (TaskCanceledException)
            {
                MarshalLog("Canceled: " + download.Guid);
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

        private bool IsExceptionHandled(string title, Exception ex, DownloadOperation download = null)
        {
            WebErrorStatus error = BackgroundTransferError.GetStatus(ex.HResult);
            if (error == WebErrorStatus.Unknown)
            {
                return false;
            }

            if (download == null)
            {
                DebugService.Add(String.Format(CultureInfo.CurrentCulture, "Error: {0}: {1}", title, error));
            }
            else
            {
                DebugService.Add(String.Format(CultureInfo.CurrentCulture, "Error: {0} - {1}: {2}", download.Guid, title, error));
            }

            return true;
        }

        private void MarshalLog(string value)
        {
            SynchronizationContext.Current.Post((o) => { DebugService.Add(value); }, null);
        }

    }

    public class DownloadProgress :  Progress<DownloadOperation>
    {
        private Action<object, int> progressDelegate;
        private SynchronizationContext context;

        public DownloadProgress(Action<object, int> progressDelegate, SynchronizationContext context)
        {
            this.progressDelegate = progressDelegate;
            this.context = context;
        }

        protected override void OnReport(DownloadOperation value)
        {
            base.OnReport(value);

            if (progressDelegate != null)
            {
                context.Post((o) =>
                {
                    int percent = (int)(value.Progress.BytesReceived * 100 / value.Progress.TotalBytesToReceive);
                    progressDelegate(this, percent);
                }, null);
            }
        }
    }
}
