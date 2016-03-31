using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ACast
{
    public sealed partial class FeedListViewItem : UserControl 
    {
        public FeedListViewItem()
        {
            this.InitializeComponent();         
        }

        public FeedListViewItem(Feed item)
        {
            this.InitializeComponent();
            SetItem(item);
        }

        public void SetText(string text)
        {
            titleTextBox.Text = text;
        }

        public void SetItem(Feed item)
        {
            titleTextBox.Text = item.Title;// +" dies ist ein Test";
            image.Source = new BitmapImage(new Uri(item.ImageUri));

            update(item);
            item.PropertyChanged += feedPropertyChanged;
        }

        private void update(Feed item)
        {
            infoTextBox.Text = string.Format("Downloaded {0}", item.MediaDownloadCount);
        }

        private void feedPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Feed feed = sender as Feed;
            if(feed != null)
                update(feed);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            titleTextBox.Width = availableSize.Width * mainGrid.ColumnDefinitions[1].Width.Value / 100;
            infoTextBox.Width = titleTextBox.Width;
            return base.MeasureOverride(availableSize);
        }
    }
}
