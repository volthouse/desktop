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
            //feedUrl.Text = "http://www.cczwei.de/rss_issues_all.php";
            //feedUrl.Text = "http://deimhart.net/index.php?/feeds/index.rss2";
            //feedUrl.Text = "http://chaosradio.ccc.de/chaosradio-latest.rss";
            //feedUrl.Text = "http://www.br-online.de/podcast/radiowissen/cast.xml";

            urlCombo.Items.Add("http://rss.golem.de/rss.php?feed=ATOM1.0");
            urlCombo.Items.Add("http://www.cczwei.de/rss_issues_all.php");
            urlCombo.Items.Add("http://deimhart.net/index.php?/feeds/index.rss2");
            urlCombo.Items.Add("http://chaosradio.ccc.de/chaosradio-latest.rss");
            urlCombo.Items.Add("http://www.br-online.de/podcast/radiowissen/cast.xml");
            urlCombo.Items.Add("http://raumzeit-podcast.de/feed/mp3/");
            urlCombo.Items.Add("http://sternengeschichten.podspot.de/rss");
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

            //Uri uri;
            //if (!Uri.TryCreate(feedUrl.Text, UriKind.Absolute, out uri))
            //{
            //    //rootPage.NotifyUser("Error: Invalid URI.", NotifyType.ErrorMessage);

            //    args.Cancel = true;
            //}
            //else
            //{
                args.Cancel = true;
                //FeedUrl = feedUrl.Text;
                FeedUrl = urlCombo.SelectedItem.ToString();
                FeedManager.Instance.AddFeed(FeedUrl, AddFeedCompletedAsync);
            //}            
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            FeedUrl = "";
        }

        private void AddFeedCompletedAsync(object sender)
        {
            this.Hide();            
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
