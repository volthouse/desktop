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
        public static MediaPlaybackItem Create(object source, object title, object position)
        {
            MediaPlaybackItem item = new MediaPlaybackItem();

            if(source != null && !string.IsNullOrEmpty(source.ToString()))
            {
                item.Source = new Uri(source.ToString());
                item.Title = title == null ? string.Empty : title.ToString();
                TimeSpan.TryParse(position.ToString(), out item.Position);
            }

            return item;
        }

        public static bool TryCreate(object source, object title, object position, out MediaPlaybackItem item)
        {
            item = MediaPlaybackItem.Create(source, title, position);

            return item.Source != null;
        }

        public static MediaPlaybackItem Create(object source, string title)
        {
            MediaPlaybackItem item = new MediaPlaybackItem();

            if (source != null && !string.IsNullOrEmpty(source.ToString()))
            {
                item.Source = new Uri(source.ToString());
                item.Title = title;
            }

            return item;
        }

        public Uri Source;
        public string Title;
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
