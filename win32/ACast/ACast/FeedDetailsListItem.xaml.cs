using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ACast
{
    public sealed partial class FeedDetailsListItem : UserControl
    {
        public FeedItem FeedItem { get; private set; }

        public EventHandler StartDownloadClick;        

        public FeedDetailsListItem(FeedItem feedItem)
        {
            this.InitializeComponent();
            FeedItem = feedItem;
            textBox.Text = feedItem.Title;
            pickButton.Icon = new SymbolIcon(feedItem.MediaState == FeedState.None ? Symbol.Pin : Symbol.UnPin);
        }

        private void pickButton_Click(object sender, RoutedEventArgs e)
        {
            if (StartDownloadClick != null)
            {
                FeedItem.MediaState = FeedState.DownloadStarted;
                pickButton.Icon = new SymbolIcon(Symbol.UnPin);
                StartDownloadClick(this, EventArgs.Empty);
            }
        }
        
    }
}
