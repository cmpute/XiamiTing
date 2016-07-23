using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace JacobC.Xiami
{
    //需要用到增量加载的内容：歌手的歌曲列表，评论，排行榜等
    public abstract class IncrementalLoadingBase<T> : ObservableCollection<T>, ISupportIncrementalLoading
    {
        public IncrementalLoadingBase() : base() { }
        public IncrementalLoadingBase(IEnumerable<T> collection) : base(collection) { }

        public bool HasMoreItems
        {
            get { return HasMoreItemsOverride(); }
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            if (_busy)
            {
                throw new InvalidOperationException("Only one operation in flight at a time");
            }
            _busy = true;
            return AsyncInfo.Run((c) => LoadMoreItemsAsync(c, count));
        }

        protected async Task<LoadMoreItemsResult> LoadMoreItemsAsync(CancellationToken c, uint count)
        {
            try
            {
                // 加载开始事件
                OnLoadMoreStarted?.Invoke(this, count);
                
                var items = await LoadMoreItemsOverrideAsync(c, count);
                AddRange(items);

                // 加载完成事件
                OnLoadMoreCompleted?.Invoke(this, (uint)(items?.Count() ?? 0));
                
                return new LoadMoreItemsResult { Count = (uint)(items?.Count() ?? 0) };
            }
            finally
            {
                _busy = false;
            }
        }

        /// <summary>
        /// 在加载更多项时发生, uint参数为希望加载的数目
        /// </summary>
        public event EventHandler<uint> OnLoadMoreStarted;
        /// <summary>
        /// 在加载更多项完成后发生, uint参数为加载到的数目
        /// </summary>
        public event EventHandler<uint> OnLoadMoreCompleted;

        /// <summary>
        /// 将<see cref="IEnumerable{T}"/>集合添加进来
        /// </summary>
        /// <remarks>之所以是virtual的是为了方便特殊要求，比如不重复之类的</remarks>
        protected virtual void AddRange(IEnumerable<T> items)
        {
            if (items != null)
            {
                foreach (var item in items)
                    this.Add(item);
            }
        }

        /// <summary>
        /// 加载更多项目的实现方法
        /// </summary>
        protected abstract Task<IEnumerable<T>> LoadMoreItemsOverrideAsync(CancellationToken c, uint count);
        /// <summary>
        /// 检查是否有更多项目的实现方法
        /// </summary>
        protected abstract bool HasMoreItemsOverride();
        
        bool _busy = false;
    }
}
