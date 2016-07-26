using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JacobC.Xiami.Models
{
    /// <summary>
    /// 可以收藏的内容基类
    /// </summary>
    public class LovableModelBase : XiamiModelBase
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
            throw new NotImplementedException();
        }

        IEnumerable<string> _Tags = default(IEnumerable<string>);
        /// <summary>
        /// 获取或设置收藏标签
        /// </summary>
        public IEnumerable<string> Tags { get { return _Tags; } set { Set(ref _Tags, value); } }

        IEnumerable<UserModel> _RelatedLovers = default(IEnumerable<UserModel>);
        /// <summary>
        /// 获取或设置收藏了该内容的用户推荐
        /// </summary>
        public IEnumerable<UserModel> RelatedLovers { get { return _RelatedLovers; } set { Set(ref _RelatedLovers, value); } }

    }

    public class XiamiModelBase : SafeBindableBase
    {
        /// <summary>
        /// 获取或设置在虾米中的编号
        /// </summary>
        public uint XiamiID { get; set; }

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
        public string Name { get { return _Name; } set { Set(ref _Name, value); } }

        PageItemsCollection<CommentModel> _Comments = null;
        /// <summary>
        /// 获取或设置评论或者留言
        /// </summary>
        public PageItemsCollection<CommentModel> Comments { get { return _Comments; } set { Set(ref _Comments, value); } }

    }
}
