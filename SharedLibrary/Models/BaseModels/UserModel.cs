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
        public static readonly UserModel Null = new UserModel() { Name = "未登录" };
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

        #region ICovered Members
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
        #endregion

        bool _IsVIP = default(bool);
        /// <summary>
        /// 获取或设置用户是否为VIP
        /// </summary>
        public bool IsVIP { get { return _IsVIP; } set { Set(ref _IsVIP, value); } }

        UserRadioModel _UserRadio = default(UserRadioModel);
        /// <summary>
        /// 获取或设置用户电台属性
        /// </summary>
        public UserRadioModel UserRadio { get { return _UserRadio; } set { Set(ref _UserRadio, value); } }

        PageItemsCollection<ListenLogModel> _RecentTracks = default(PageItemsCollection<ListenLogModel>);
        /// <summary>
        /// 获取或设置最近播放的歌曲属性
        /// </summary>
        public PageItemsCollection<ListenLogModel> RecentTracks { get { return _RecentTracks; } set { Set(ref _RecentTracks, value); } }

        string _PersonalDescription = default(string);
        /// <summary>
        /// 获取或设置年龄、星座和性别
        /// </summary>
        public string PersonalDescription { get { return _PersonalDescription; } set { Set(ref _PersonalDescription, value); } }

        string _JoinDate = default(string);
        /// <summary>
        /// 获取或设置加入时间
        /// </summary>
        public string JoinDate { get { return _JoinDate; } set { Set(ref _JoinDate, value); } }

        string _Level = default(string);
        /// <summary>
        /// 获取或设置用户等级
        /// </summary>
        public string Level { get { return _Level; } set { Set(ref _Level, value); } }

        int _FollowerCount = default(int);
        /// <summary>
        /// 获取或设置粉丝数属性
        /// </summary>
        public int FollowerCount { get { return _FollowerCount; } set { Set(ref _FollowerCount, value); } }

        int _FollowingCount = default(int);
        /// <summary>
        /// 获取或设置关注数属性
        /// </summary>
        public int FollowingCount { get { return _FollowingCount; } set { Set(ref _FollowingCount, value); } }

        int _VisitedCount = default(int);
        /// <summary>
        /// 获取或设置被访问次数属性
        /// </summary>
        public int VisitedCount { get { return _VisitedCount; } set { Set(ref _VisitedCount, value); } }


        public override bool CheckWhetherNeedInfo()
        {
            return UserRadio == null;
        }
    }
}
