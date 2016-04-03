using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace ACast.DataBinding
{
    public class FeedsIncrementalLoading : ObservableCollection<FeedListViewItem>
    {
        private int last = 0;

        public void Load()
        {
            //FeedManager.Instance.DeserializeFeedsAsync(onfeedsDeserializedAsync);
        }

        public bool HasMoreItems
        {
            get { return Count < FeedManager.Instance.CurrentFeed.Items.Count; }
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return LoadDataAsync(count).AsAsyncOperation();
        }

        private async Task<LoadMoreItemsResult> LoadDataAsync(uint count)
        {
            Task task = Task.Delay(1);
            await task;

            for (int i = 0; i < count && last < FeedManager.Instance.FeedsObsolet.Count; i++)
            {
                Add(new FeedListViewItem(FeedManager.Instance.FeedsObsolet[last]));
                last++;
            }

            return new LoadMoreItemsResult { Count = count };
        }

        private void onfeedsDeserializedAsync(object sender)
        {
            SynchronizationContext.Current.Post((o) => {
                this.Clear();
                foreach (var item in FeedManager.Instance.FeedsObsolet)
                {
                    this.Add(new FeedListViewItem(item));
                }
            }, null);
        }
    }
}
