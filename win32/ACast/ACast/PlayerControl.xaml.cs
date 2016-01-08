using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
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
    public sealed partial class PlayerControl : UserControl
    {
        public PlayerControl()
        {
            this.InitializeComponent();

            playButton.Click += playButton_Click;
            posSlider.ValueChanged += posSlider_ValueChanged;
        }

        void posSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {

        }

        void playButton_Click(object sender, RoutedEventArgs e)
        {
            if (Player.Instance.State == MediaPlayerState.Playing)
            {
                Player.Instance.Pause();
            }
            else if (Player.Instance.State == MediaPlayerState.Paused)
            {
                Player.Instance.Resume();
            }
        }


        
        
    }
}
