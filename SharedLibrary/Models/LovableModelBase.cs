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

        IList<UserModel> _RelatedLovers = default(IList<UserModel>);
        /// <summary>
        /// 获取或设置收藏了该内容的用户推荐
        /// </summary>
        public IList<UserModel> RelatedLovers { get { return _RelatedLovers; } set { Set(ref _RelatedLovers, value); } }

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

    /// <summary>
    /// 虾米内容的基类
    /// </summary>
    /// <typeparam name="TID">虾米ID的类型</typeparam>
    public class XiamiModelBase<TID> : SafeBindableBase
    {
        /// <summary>
        /// 获取或设置在虾米中的编号
        /// </summary>
        public TID XiamiID { get; set; }

        string _Description = default(string);
        /// <summary>
        /// 获取或设置副标题、别名或者描述
        /// </summary>
        public string Description { get { return _Description; } set { Set(ref _Description, value); } }

        string _Name = default(string);
        /// <summary>
        /// 获取或设置名称
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get { return _Name; } set { Set(ref _Name, WebUtility.HtmlDecode(value)); } }

        PageItemsCollection<CommentModel> _Comments = null;
        /// <summary>
        /// 获取或设置评论或者留言
        /// </summary>
        public PageItemsCollection<CommentModel> Comments { get { return _Comments; } set { Set(ref _Comments, value); } }

    }
}
