using JacobC.Xiami.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JacobC.Xiami.Models
{
    public class UserModel : XiamiModelBase<uint>, ICovered
    {
        static Dictionary<uint, UserModel> _dict = new Dictionary<uint, UserModel>();

        /// <summary>
        /// 获取一个新的<see cref="UserModel"/>实例，如果已经创建过则返回这个实例
        /// </summary>
        /// <param name="xiamiID">标志<see cref="UserModel"/>的虾米ID</param>
        /// <returns></returns>
        public static UserModel GetNew(uint xiamiID)
        {
            UserModel song = null;
            if (!(_dict?.TryGetValue(xiamiID, out song) ?? false))
            {
                song = new UserModel() { XiamiID = xiamiID };
                _dict?.Add(xiamiID, song);
            }
            return song;
        }
        
        /// <summary>
        /// 获取指定大小的封面地址
        /// </summary>
        /// <param name="size">封面大小对应的数字
        /// 0:原图 1:55x55 2:100x100 3:200x240(按比例)
        /// </param>
        public Uri GetArtWithSize(int sizecode)
        {
            return new Uri(ArtFull.ToString() + (sizecode == 0 ? "" : sizecode.ToString()));
        }

        private UserModel() { }

        Uri _AvatarArtUri = new Uri(@"ms-appx:///Assets/Pictures/usr50.gif");
        /// <summary>
        /// 获取或设置用户头像的链接
        /// </summary>
        public Uri Art
        {
            get { return _AvatarArtUri; }
            set
            {
                if (_AvatarArtUri?.ToString() != value?.ToString())
                    Set(ref _AvatarArtUri, value);
            }
        }

        Uri _AvatarArtFullUri = new Uri(@"ms-appx:///Assets/Pictures/usr200.gif");
        /// <summary>
        /// 获取或设置用户高清头像的链接
        /// </summary>
        public Uri ArtFull
        {
            get { return _AvatarArtFullUri; }
            set
            {
                if (_AvatarArtFullUri?.ToString() != value?.ToString())
                    Set(ref _AvatarArtFullUri, value);
            }
        }

        bool _IsVIP = default(bool);
        /// <summary>
        /// 获取或设置用户是否为VIP
        /// </summary>
        public bool IsVIP { get { return _IsVIP; } set { Set(ref _IsVIP, value); } }

    }
}
