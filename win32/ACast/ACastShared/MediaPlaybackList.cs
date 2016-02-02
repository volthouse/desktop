using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace ACastShared
{
    public class MediaPlaybackList
    {
        private List<MediaPlaybackItem> items;
        private int currentItemIndex = 0;

        private void OnCurrentItemChanged(MediaPlaybackItem newItem)
        {
            if (CurrentItemChanged != null)
            {
                var eventArgs = new CurrentMediaPlaybackItemChangedEventArgs();
                eventArgs.NewItem = newItem;
                CurrentItemChanged(this, eventArgs);
            }
        }

        private MediaPlaybackItem InternalMoveTo(int newItemIndex)
        {
            if (newItemIndex >= 0 && newItemIndex < items.Count)
            {
                currentItemIndex = newItemIndex;

                var newItem = items[currentItemIndex];
                OnCurrentItemChanged(newItem);
                return newItem;
            }
            return null;
        }

        public event TypedEventHandler<MediaPlaybackList, CurrentMediaPlaybackItemChangedEventArgs> CurrentItemChanged;

        public MediaPlaybackList()
        {
            items = new List<MediaPlaybackItem>();
        }

        public int CurrentItemIndex 
        {
            get { return currentItemIndex; }
        }

        public IList<MediaPlaybackItem> Items
        {
            get { return items; }
        }

        public bool AutoRepeatEnabled { get; set; }

        public MediaPlaybackItem CurrentItem {
            get {
                if (items.Count > 0 && currentItemIndex >= 0)
                {
                    return items[currentItemIndex]; 
                }
                return null;
            }
        }

        public MediaPlaybackItem MoveTo(uint itemIndex)
        {
            return InternalMoveTo((int)itemIndex);
        }

        public MediaPlaybackItem MovePrevious()
        {
            return InternalMoveTo(currentItemIndex + 1);
        }

        public MediaPlaybackItem MoveNext()
        {
            return InternalMoveTo(currentItemIndex - 1);
        }
    }

    public sealed class CurrentMediaPlaybackItemChangedEventArgs {
        public MediaPlaybackItem NewItem;        
    }

}
