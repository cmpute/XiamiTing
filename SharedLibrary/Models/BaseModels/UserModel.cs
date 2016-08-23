using JacobC.Xiami.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JacobC.Xiami.Models
{
    public class UserModel : XiamiModelBase<uint>
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
        private UserModel() { }

        Uri _AvatarUri = new Uri(@"ms-appx:///Assets/Pictures/usr50.gif");
        /// <summary>
        /// 获取或设置用户头像的链接
        /// </summary>
        public Uri AvatarUri
        {
            get { return _AvatarUri; }
            set
            {
                if (_AvatarUri?.ToString() != value?.ToString())
                    Set(ref _AvatarUri, value);
            }
        }

        Uri _AvatarFullUri = new Uri(@"ms-appx:///Assets/Pictures/usr200.gif");
        /// <summary>
        /// 获取或设置用户高清头像的链接
        /// </summary>
        public Uri AvatarFullUri
        {
            get { return _AvatarFullUri; }
            set
            {
                if (_AvatarFullUri?.ToString() != value?.ToString())
                    Set(ref _AvatarFullUri, value);
            }
        }

        bool _IsVIP = default(bool);
        /// <summary>
        /// 获取或设置用户是否为VIP
        /// </summary>
        public bool IsVIP { get { return _IsVIP; } set { Set(ref _IsVIP, value); } }

    }
}
