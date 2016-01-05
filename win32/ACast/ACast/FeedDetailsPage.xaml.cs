using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Syndication;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace ACast
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FeedDetailsPage : Page
    {
        private int feedIdx;
      
        public FeedDetailsPage()
        {
            this.InitializeComponent();

            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            FeedHelper.Instance.FeedActivatedAsync += FeedActivatedAsync;
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
            FeedHelper.Instance.FeedActivatedAsync -= FeedActivatedAsync;
            FeedHelper.Instance.DeactiveCurrentFeed();

            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame != null && rootFrame.CanGoBack)
            {
                e.Handled = true;
                rootFrame.GoBack();
            }
        }

        private void FeedActivatedAsync()
        {
            foreach (var item in FeedHelper.Instance.CurrentFeedItems)
            {
                FeedDetailsListItem detailsItem = new FeedDetailsListItem(item);
                listView.Items.Add(detailsItem);
                detailsItem.StartDownloadClick += StartDownloadClick;
            }
        }

        private void StartDownloadClick(object sender, EventArgs args)
        {
            FeedDetailsListItem item = sender as FeedDetailsListItem;
            if (item != null)
            {
                FeedHelper.Instance.StartDownloadMedia(item.FeedItem);
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            feedIdx = (int)e.Parameter;
            FeedHelper.Instance.ActiveFeedAsync(feedIdx);
        }        
    }
}
