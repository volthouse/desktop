using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
    public sealed partial class FeedDetailsListViewItem : UserControl
    {
        private SynchronizationContext context;

        public FeedItem FeedItem { get; private set; }


        public FeedDetailsListViewItem(FeedItem feedItem)
        {
            this.InitializeComponent();

            context = SynchronizationContext.Current;

            FeedItem = feedItem;
            textBox.Text = feedItem.Title;
            pickButton.Icon = new SymbolIcon(FeedItem.DownloadState == FeedDownloadState.None ? Symbol.Pin : Symbol.UnPin);
            playButton.Visibility = FeedItem.DownloadState == FeedDownloadState.DownloadCompleted ? Visibility.Visible : Visibility.Collapsed;

            FeedItem.StateChanged += stateChanged;
            feedItem.DownloadProgressChanged += progressChanged;
        }

        private void pickButton_Click(object sender, RoutedEventArgs e)
        {
            switch (FeedItem.DownloadState)
            {
                case FeedDownloadState.None:
                    FeedManager.Instance.StartDownloadMedia(FeedItem);
                    pickButton.Icon = new SymbolIcon(Symbol.UnPin);
                    break;
                case FeedDownloadState.DownloadStarted:
                    break;
                case FeedDownloadState.DownloadCompleted:
                    break;
                default:
                    break;
            }
        }

        private void stateChanged(object sender, EventArgs args)
        {
            context.Post(new SendOrPostCallback((o) =>
            {
                pickButton.Icon = new SymbolIcon(FeedItem.DownloadState == FeedDownloadState.None ? Symbol.Pin : Symbol.UnPin);
                playButton.Visibility = FeedItem.DownloadState == FeedDownloadState.DownloadCompleted ? Visibility.Visible : Visibility.Collapsed;
            }), null);
        }

        private void progressChanged(object sender, float progress)
        {
            context.Post(new SendOrPostCallback((o) =>
            {
                pickButton.Label = progress.ToString("#") + "%";
            }), null);
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            Player.Instance.Play(FeedItem);
            //string path = ApplicationData.Current.LocalFolder.Path + @"\" + FeedItem.FileName;
            //mediaElement.Source = new Uri(path);
            //mediaElement.Play();
        }
        
    }
}
