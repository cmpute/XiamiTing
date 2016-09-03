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
        public delegate Task<IEnumerable<T>> FetchPageDelegate(uint pageIndex, CancellationToken token);

        FetchPageDelegate _fetchPage;
        bool _hasMore = true;
        uint _current = 1;//当前页码

        public uint PageCapacity { get; set; }

        /// <summary>
        /// 根据已有的完整第一页内容获取一个<see cref="PageItemsCollection{T}"/>对象
        /// </summary>
        /// <param name="firstpage">第一页的内容</param>
        /// <param name="fetchPage">获取某一页的方法</param>
        public PageItemsCollection(IEnumerable<T> firstpage, FetchPageDelegate fetchPage) : base(firstpage)
        {
            AddRange(firstpage);
            PageCapacity = (uint)Count;
            _fetchPage = fetchPage;
        }
        /// <summary>
        /// 指定每页项目数量获取一个<see cref="PageItemsCollection{T}"/>对象，并且获取第一页 
        /// </summary>
        /// <param name="capacity">每一页项目数量</param>
        /// <param name="fetchPage">获取某一页的方法</param>
        public PageItemsCollection(uint capacity, FetchPageDelegate fetchPage) : base()
        {
            PageCapacity = capacity;
            _fetchPage = fetchPage;
            _GetFirst();
        }
        /// <summary>
        /// 指定每页项目数量获取一个<see cref="PageItemsCollection{T}"/>对象，并写入第一页的部分内容
        /// </summary>
        /// <param name="capacity">每一页项目数量</param>
        /// <param name="firstpart">第一页的部分内容</param>
        /// <param name="fetchpage">获取某一页的方法</param>
        public PageItemsCollection(uint capacity, IEnumerable<T> firstpart, FetchPageDelegate fetchpage)
        {
            PageCapacity = capacity;
            _fetchPage = fetchpage;
            AddRange(firstpart);
        }
        /// <summary>
        /// 获取固定大小的<see cref="PageItemsCollection{T}"/>
        /// </summary>
        /// <param name="items"></param>
        public PageItemsCollection(IEnumerable<T> items)
        {
            AddRange(items);
            PageCapacity = (uint)Count;
            _hasMore = false;
            _fetchPage = (a, b) => null;
        }
        internal async void _GetFirst()
        {
            var result = await Run((c) => _fetchPage?.Invoke(1, c));
            if (result == null)
                _hasMore = false;
            else if (result.Count() < PageCapacity)
                _hasMore = false;
            else
                _hasMore = true;
            AddRange(result);
        }

        protected sealed override bool HasMoreItemsOverride()
        {
            return _hasMore;
        }

        protected override async Task<IEnumerable<T>> LoadMoreItemsAsync(CancellationToken c, uint count)
        {
            var returnlist = new List<T>();
            if (Count < PageCapacity)
            {
                //针对第一页未满的情况
                var r = await _fetchPage(1, c);
                var tc = Count;
                var sc = Count;
                foreach (var item in r)
                {
                    if (tc > 0)
                    {
                        tc--;
                        continue;
                    }
                    returnlist.Add(item);
                    sc++;
                    if (count > 0) count--;
                }
                if (sc < PageCapacity)
                {
                    _hasMore = false;
                    return returnlist;
                }
            }
            uint pages = count / PageCapacity + _current;
            if (count % PageCapacity != 0) pages++;
            for (uint i = _current + 1; i <= pages; i++)
            {
                var r = await _fetchPage(i, c);//为了保证顺序，不使用并发.TODO: 考虑并发后排序？
                returnlist.AddRange(r);
                if (i == pages)
                    if (r == null)
                        _hasMore = false;
                    else if (r.Count() < PageCapacity)
                        _hasMore = false;
                    else
                        _hasMore = true;
            }
            _current = pages;
            return returnlist;
        }
    }
}
