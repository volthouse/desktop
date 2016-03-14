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

        public void SetText(string text)
        {
            textBox.Text = text;
        }

        public void SetItem(Feed item)
        {
            textBox.Text = item.Title;

            image.Source = new BitmapImage(new Uri(item.ImageUri));
        }

    }
}
