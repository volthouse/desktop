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
        private FeedDialog viewController;

        public FeedItemsViewControl()
        {
            this.InitializeComponent();
        }

        public FeedDialog ViewController
        {
            get { return viewController; }
            set
            {
                viewController = value;
                viewController.OnActivate += ViewController_OnActivate;
            }
        }

        private void ViewController_OnActivate(object sender, EventArgs e)
        {
            listView.ItemsSource = FeedManager.Feeds;
            viewController.CommandBar.PrimaryCommands.Clear();
            viewController.CommandBar.PrimaryCommands.Add(addButton);
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
        }
    }
}
