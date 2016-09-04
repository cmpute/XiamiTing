﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace JacobC.Xiami.Models
{
    /// <summary>
    /// 虾米内容的带ID的基类
    /// </summary>
    /// <typeparam name="TID">虾米ID的类型</typeparam>
    public class XiamiModelBase<TID> : XiamiModelBase
    {
        /// <summary>
        /// 获取或设置在虾米中的编号
        /// </summary>
        [JsonProperty]
        public virtual TID XiamiID { get; set; }

        PageItemsCollection<CommentModel> _Comments = null;
        /// <summary>
        /// 获取或设置评论或者留言
        /// </summary>
        public PageItemsCollection<CommentModel> Comments { get { return _Comments; } set { Set(ref _Comments, value); } }

    }
    /// <summary>
    /// 虾米内容的基类
    /// </summary>
    public class XiamiModelBase : SafeBindableBase
    {
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

        //对string的设置默认decode一遍
        public bool Set(ref string storage, string value, [CallerMemberName] string propertyName = null)
        {
            return base.Set<string>(ref storage, WebUtility.HtmlDecode(value), propertyName);
        }
    }
}
