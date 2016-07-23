using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace JacobC.Xiami
{
    /// <summary>
    /// 按页面加载项目的增量集合
    /// </summary>
    /// <typeparam name="T">集合项目类型</typeparam>
    public class PageItemsCollection<T> : IncrementalLoadingBase<T>
    {
        //if = null / count<pagecapacity => hasmore = false
        public delegate Task<IEnumerable<T>> FetchPageDelegate(uint pageIndex, CancellationToken token);

        FetchPageDelegate _fetchPage;
        bool _hasMore;
        uint _current = 1;

        public uint PageCapacity { get; set; }

        public PageItemsCollection(uint capacity, IEnumerable<T> firstpage, FetchPageDelegate fetchPage) : base(firstpage)
        {
            PageCapacity = capacity;
            _fetchPage = fetchPage;
            _hasMore = true;
        }
        public PageItemsCollection(uint capacity, FetchPageDelegate fetchPage) : base()
        {
            PageCapacity = capacity;
            _fetchPage = fetchPage;
            _GetFirst();
        }
        internal async void _GetFirst()
        {
            var result = await Run((c) => _fetchPage(1, c));
            if (result == null)
                _hasMore = false;
            else if (result.Count() < PageCapacity)
                _hasMore = false;
            else
                _hasMore = true;
            AddRange(result);
        }

        protected override bool HasMoreItemsOverride()
        {
            return _hasMore;
        }

        protected override async Task<IEnumerable<T>> LoadMoreItemsOverrideAsync(CancellationToken c, uint count)
        {
            uint icount = (uint)count;
            uint pages = icount / PageCapacity + _current;
            if (icount % PageCapacity != 0) pages++;
            var returnlist = new List<T>();
            for (uint i = _current + 1; i <= pages; i++)
            {
                var r = await _fetchPage(1, c);//为了保证顺序，不使用并发
                returnlist.AddRange(r);
                if (i == pages)
                    if (r == null)
                        _hasMore = false;
                    else if (r.Count() < PageCapacity)
                        _hasMore = false;
                    else
                        _hasMore = true;
            }
            return returnlist;
        }
    }
}
