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
    public sealed partial class FeedViewControl : UserControl
    {
        public event EventHandler<ButtonFlags> EnableButtons;

        public event EventHandler<PivotView> ActivateView;

        public FeedViewControl()
        {
            this.InitializeComponent();

        }

        public void ActiveViewChanged(object sender, PivotView view)
        {
            if (view == PivotView.Feeds)
            {
                listView.ItemsSource = FeedManager.Feeds;
                OnEnableButtons(ButtonFlags.Add | ButtonFlags.Remove);
            }

        }

        public async void CommandBarButtonClick(object sender, ButtonFlags buttonFlags)
        {
            switch (buttonFlags)
            {
                case ButtonFlags.Add:
                    FeedUrlDialog dlg = new FeedUrlDialog();
                    await dlg.ShowAsync();
                    listView.ItemsSource = FeedManager.Feeds;
                    break;
                case ButtonFlags.Remove:
                    break;
                case ButtonFlags.Cancel:
                    break;
                case ButtonFlags.Multiselect:
                    break;
                case ButtonFlags.Search:
                    break;
                default:
                    break;
            }
        }


        private void pickerButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void serachFeedTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
        }

        private void listView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Feed feed = e.ClickedItem as Feed;
            if (feed != null)
            {
                FeedManager.CurrentFeedId = feed.Id;
                OnActivateView(PivotView.FeedItems);
            }
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
    }
}
