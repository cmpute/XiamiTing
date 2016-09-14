using JacobC.Xiami.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JacobC.Xiami.Models
{
    public class UserRadioModel : RadioModel, ICovered
    {
        public const string SmallDefaultUri = @"ms-appx:///Assets/Pictures/cd100.gif";
        public const string LargeDefaultUri = @"ms-appx:///Assets/Pictures/cd500.gif";

        internal UserRadioModel(uint xiamiID, uint oid) : base(xiamiID, RadioType.User, oid) { }

        #region ICovered Members
        Uri _RadioCoverUri = new Uri(SmallDefaultUri);
        /// <summary>
        /// 获取或设置用户电台封面的链接
        /// </summary>
        public Uri Art
        {
            get { return _RadioCoverUri; }
            set
            {
                if (_RadioCoverUri?.ToString() != value?.ToString())
                    Set(ref _RadioCoverUri, value);
            }
        }

        Uri _RadioCoverFullUri = new Uri(LargeDefaultUri);
        /// <summary>
        /// 获取或设置用户电台高清封面的链接
        /// </summary>
        public Uri ArtFull
        {
            get { return _RadioCoverFullUri; }
            set
            {
                if (_RadioCoverFullUri?.ToString() != value?.ToString())
                    Set(ref _RadioCoverFullUri, value);
            }
        }

        /// <summary>
        /// 获取指定大小的封面地址
        /// </summary>
        /// <param name="size">封面大小对应的数字，只能选0, 1</param>
        public Uri GetArtWithSize(int sizecode)
        {
            return (sizecode == 0 ? _RadioCoverFullUri : _RadioCoverUri);
        }
        #endregion

        //Description 为电台介绍

    }
}
