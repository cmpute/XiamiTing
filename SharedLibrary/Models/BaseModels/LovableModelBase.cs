using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JacobC.Xiami.Models
{
    /// <summary>
    /// 可以收藏的内容基类
    /// </summary>
    /// <typeparam name="TID">虾米ID的类型</typeparam>
    public class LovableModelBase<TID> : XiamiModelBase<TID>
    {
        bool _IsLoved = default(bool);
        /// <summary>
        /// 获取或设置是否被收藏
        /// </summary>
        public bool IsLoved
        {
            get { return _IsLoved; }
            set
            {
                if (Set(ref _IsLoved, value))
                    OnIsLovedChanged();
            }
        }
        protected virtual void OnIsLovedChanged()
        {
            //throw new NotImplementedException();
        }

        IList<string> _Tags = default(IList<string>);
        /// <summary>
        /// 获取或设置收藏标签
        /// </summary>
        public IList<string> Tags { get { return _Tags; } set { Set(ref _Tags, value); } }

        IIncrementalLoadingCollection<UserModel> _RelatedLovers = default(IIncrementalLoadingCollection<UserModel>);
        /// <summary>
        /// 获取或设置收藏了该内容的用户推荐
        /// </summary>
        public IIncrementalLoadingCollection<UserModel> RelatedLovers { get { return _RelatedLovers; } set { Set(ref _RelatedLovers, value); } }

        int _PlayCount = -1;
        /// <summary>
        /// 获取或设置播放次数属性
        /// </summary>
        public int PlayCount { get { return _PlayCount; } set { Set(ref _PlayCount, value); } }

        int _ShareCount = -1;
        /// <summary>
        /// 获取或设置分享次数属性
        /// </summary>
        public int ShareCount { get { return _ShareCount; } set { Set(ref _ShareCount, value); } }

        int _LovedCount = default(int);
        /// <summary>
        /// 获取或设置收藏次数
        /// </summary>
        public int LovedCount { get { return _LovedCount; } set { Set(ref _LovedCount, value); } }


    }
}
