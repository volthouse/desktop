using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers.Provider;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace ACast
{
    public sealed partial class FeedUrlDialog : ContentDialog
    {
        public string FeedUrl;

        public FeedUrlDialog()
        {
            this.InitializeComponent();

            //feedUrl.Text = "http://rss.golem.de/rss.php?feed=ATOM1.0";
            feedUrl.Text = "http://www.cczwei.de/rss_issues_all.php";
            //feedUrl.Text = "http://deimhart.net/index.php?/feeds/index.rss2";
            //feedUrl.Text = "http://chaosradio.ccc.de/chaosradio-latest.rss";
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

            Uri uri;
            if (!Uri.TryCreate(feedUrl.Text, UriKind.Absolute, out uri))
            {
                //rootPage.NotifyUser("Error: Invalid URI.", NotifyType.ErrorMessage);

                args.Cancel = true;
            }
            else
            {
                args.Cancel = true;
                FeedUrl = feedUrl.Text;
                FeedManager.Instance.AddFeedCompletedAsync += AddFeedCompletedAsync;
                FeedManager.Instance.AddFeed(feedUrl.Text);
            }            
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            FeedUrl = "";
        }

        private void AddFeedCompletedAsync()
        {
            FeedManager.Instance.AddFeedCompletedAsync -= AddFeedCompletedAsync;
            this.Hide();            
        }
    }
}
