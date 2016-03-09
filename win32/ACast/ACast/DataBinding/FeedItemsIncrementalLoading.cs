using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace ACast.DataBinding
{

    public class FeedItemsIncrementalLoading : ObservableCollection<FeedDetailsListViewItem>, ISupportIncrementalLoading
    {
        private int last = 0;

        public bool HasMoreItems
        {
            get { return Count < FeedManager.Instance.CurrentFeedItems.Count; }
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return LoadDataAsync(count).AsAsyncOperation();
        }

        private async Task<LoadMoreItemsResult> LoadDataAsync(uint count)
        {
            Task task = Task.Delay(1);
            await task;

            for (int i = 0; i < count; i++)
            {
                Add(new FeedDetailsListViewItem(FeedManager.Instance.CurrentFeedItems[last]));
                last++;
            }

            return new LoadMoreItemsResult { Count = count };
        }

    }
}
