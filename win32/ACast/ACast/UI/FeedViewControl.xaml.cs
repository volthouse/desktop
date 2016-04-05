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
        private FeedDialog viewController;

        private AddButton addButton = new AddButton();
        private RemoveButton removeButton = new RemoveButton();
        private MultiSelectButton multiSelectButton = new MultiSelectButton();
        private SearchButton searchButton = new SearchButton();
        private CancelButton cancelButton = new CancelButton();
        private RefreshButton refreshButton = new RefreshButton();


        public FeedViewControl()
        {
            this.InitializeComponent();

            addButton.Click += AddButton_Click;
        }

        public FeedDialog ViewController {
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

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            FeedUrlDialog dlg = new FeedUrlDialog();
            await dlg.ShowAsync();
            listView.ItemsSource = FeedManager.Feeds;
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
