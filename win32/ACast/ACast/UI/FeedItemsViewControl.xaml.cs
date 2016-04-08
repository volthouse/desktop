using ACast.Database;
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

namespace ACast.UI
{
    public sealed partial class FeedItemsViewControl : UserControl
    {
        public event EventHandler<ButtonFlags> EnableButtons;

        public event EventHandler<PivotView> ActivateView;

        public event EventHandler<FeedItem> Play;

        public FeedItemsViewControl()
        {
            this.InitializeComponent();
        }

        public void ActiveViewChanged(object sender, PivotView view)
        {
            if (view == PivotView.FeedItems)
            {
                listView.ItemsSource = FeedManager.FeedItems;
                OnEnableButtons(ButtonFlags.Multiselect | ButtonFlags.Search);
            }

        }

        private void pickerButton_Click(object sender, RoutedEventArgs e)
        {
            Control control = sender as Control;
            if (control != null)
            {
                FeedItem feedItem = control.DataContext as FeedItem;
                if (feedItem != null)
                {
                    FeedManager.Instance.DownloadFeedItemMedia(feedItem);
                }
            }
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            OnActivateView(PivotView.Player);

            Control control = sender as Control;
            if (control != null)
            {
                FeedItem feedItem = control.DataContext as FeedItem;
                if (feedItem != null)
                {
                    OnPlay(feedItem);
                }
            }
        }

        private void serachFeedTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
        }

        private void listView_ItemClick(object sender, ItemClickEventArgs e)
        {
        }

        private void OnEnableButtons(ButtonFlags buttonFlags)
        {
            if (EnableButtons != null)
            {
                EnableButtons(this, buttonFlags);
            }
        }

        private void OnActivateView(PivotView view)
        {
            if (ActivateView != null)
            {
                ActivateView(this, view);
            }
        }

        private void OnPlay(FeedItem feedItem)
        {
            if (Play != null)
            {
                Play(this, feedItem);
            }
        }
    }
}
