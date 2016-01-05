using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Syndication;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace ACast
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {        
        private ApplicationDataContainer localSettings = null;
        private SynchronizationContext context;

        public MainPage()
        {
            this.InitializeComponent();

            context = SynchronizationContext.Current;

            this.NavigationCacheMode = NavigationCacheMode.Required;

            localSettings = ApplicationData.Current.LocalSettings;

            FeedHelper.Instance.FeedListLoadedAsync += FeedListLoadedAsync;
            FeedHelper.Instance.LoadFeedListAsync();
 
        }

        private void DownloadProgressAsync(float percent) 
        {
            var ignored = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                progressBar.Value = percent;
            });
        }

        private void DownloadCompletedAsync()
        {
            var ignored = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                progressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            });
        }

       
        private void FeedListLoadedAsync()
        {
            FeedHelper.Instance.FeedListLoadedAsync += FeedListLoadedAsync;

            context.Post(new SendOrPostCallback((o) => {
                listView.Items.Clear();

                foreach (var item in FeedHelper.Instance.FeedList)
                {
                    CustomListItem customItem = new CustomListItem();
                    customItem.SetItem(item);
                    listView.Items.Add(customItem);
                }
            }), null);
        }           

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }          

        private void listView_ItemClick(object sender, ItemClickEventArgs e)
        {
            int i = listView.Items.IndexOf(e.ClickedItem);
            Frame.Navigate(typeof(FeedDetailsPage), i);
        }

        private async void addFeedButton_Click(object sender, RoutedEventArgs e)
        {
            var newFeedUrlDlg = new FeedUrlDialog();
            await newFeedUrlDlg.ShowAsync();

            FeedHelper.Instance.LoadFeedListAsync();
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            FeedHelper.Instance.FeedListDeletedAsync += FeedListDeletedAsync;
            FeedHelper.Instance.DeleteFeedList();
        }

        private void FeedListDeletedAsync()
        {
            FeedHelper.Instance.FeedListDeletedAsync -= FeedListDeletedAsync;
            FeedHelper.Instance.LoadFeedListAsync();
        }
    }
}
