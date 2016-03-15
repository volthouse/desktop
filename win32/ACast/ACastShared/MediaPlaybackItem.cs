using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;

namespace ACastShared
{
    public class MediaPlaybackItem
    {
        public static MediaPlaybackItem Create(object source, object position)
        {
            MediaPlaybackItem item = new MediaPlaybackItem();

            if(source != null && !string.IsNullOrEmpty(source.ToString()))
            {
                item.Source = new Uri(source.ToString());
                TimeSpan.TryParse(position.ToString(), out item.Position);
            }

            return item;
        }

        public static MediaPlaybackItem Create(object source)
        {
            MediaPlaybackItem item = new MediaPlaybackItem();

            if (source != null && !string.IsNullOrEmpty(source.ToString()))
            {
                item.Source = new Uri(source.ToString());
            }

            return item;
        }

        public Uri Source;
        public TimeSpan Position;

        public void Play()
        {
            BackgroundMediaPlayer.Current.SetUriSource(Source);
            if (Position > TimeSpan.Zero)
            {
                BackgroundMediaPlayer.Current.Position = Position;
            } else
            {
                BackgroundMediaPlayer.Current.Play();
            }
        }

        public void Resume()
        {
            BackgroundMediaPlayer.Current.Play();
        }

        public void Pause()
        {
            BackgroundMediaPlayer.Current.Pause();
        }
    }    
}
