using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JacobC.Xiami.Net;

namespace JacobC.Xiami.Models
{
    /// <summary>
    /// 电台的模型
    /// </summary>
    public class RadioModel : LovableModelBase<uint>
    {
        #region ctor
        static Dictionary<uint, RadioModel> _dict = new Dictionary<uint, RadioModel>()
        {
            [2] = new RadioModel(2, RadioType.Guess, 0),
        };
        //public static readonly RadioModel Null = new RadioModel() { };
        /// <summary>
        /// 根据虾米ID获取一个新的<see cref="RadioModel"/>实例，如果已经创建过则返回这个实例
        /// </summary>
        /// <param name="xiamiID">标志<see cref="RadioModel"/>的虾米ID</param>
        /// <returns></returns>
        public static RadioModel GetNew(uint xiamiID)
        {
            RadioModel radio = null;
            if (!(_dict?.TryGetValue(xiamiID, out radio) ?? false))
            {
                var type = RadioType.UnKnown;
                var oid = 0u;
                ExtensionMethods.InvokeAndWait(async () =>
                {
                    var res = await WebApi.Instance.GetRadioType(xiamiID);
                    type = res.Item1; oid = res.Item2;
                });
                radio = new RadioModel(xiamiID, type, oid);
                _dict?.Add(xiamiID, radio);
            }
            return radio;
        }
        /// <summary>
        /// 根据类型和类型ID获取一个新的<see cref="RadioModel"/>实例，如果已经创建过则返回这个实例
        /// </summary>
        /// <param name="type">电台的类型</param>
        /// <param name="oid">该类型电台需要的参数</param>
        public static RadioModel GetNew(RadioType type, uint oid)
        {
            uint xiamiID = 0;
            ExtensionMethods.InvokeAndWait(async () => xiamiID = await WebApi.Instance.GetRadioId(type, oid));
            RadioModel radio = null;
            if (!(_dict?.TryGetValue(xiamiID, out radio) ?? false))
            {
                if (type == RadioType.User)
                    radio = new UserRadioModel(xiamiID, oid);
                else
                    radio = new RadioModel(xiamiID, type, oid);
                _dict?.Add(xiamiID, radio);
            }
            return radio;
        }
        protected RadioModel(uint xiamiID, RadioType type, uint oid)
        {
            XiamiID = xiamiID;
            Type = type;
            OID = oid;
        }
        #endregion

        /// <summary>
        /// 获取或设置电台的类型
        /// </summary>
        public RadioType Type { get; set; } = RadioType.UnKnown;
        /// <summary>
        /// 获取或设置电台的类型参数
        /// </summary>
        public uint OID { get; set; }

        /// <summary>
        /// 虾米猜电台
        /// </summary>
        public static RadioModel XiamiCai => GetNew(2);
        /// <summary>
        /// （未登录用户）每日推荐
        /// </summary>
        public static RadioModel PublicRec => GetNew(RadioType.DailyRecommendaition, 1);
        /// <summary>
        /// 获取艺人电台
        /// </summary>
        public static RadioModel GetFromArtist(ArtistModel artist) => GetNew(RadioType.Artist, artist.XiamiID);
        /// <summary>
        /// 获取用户电台
        /// </summary>
        public static UserRadioModel GetFromUser(UserModel user) => GetNew(RadioType.User, user.XiamiID) as UserRadioModel;

    }

    /// <summary>
    /// 表示电台的类型
    /// </summary>
    public enum RadioType
    {
        UnKnown = 0,
        /// <summary>
        /// 星座
        /// </summary>
        Constellation = 1,
        /// <summary>
        /// 年代
        /// </summary>
        Decade = 2,
        /// <summary>
        /// 用户
        /// </summary>
        User = 4,
        /// <summary>
        /// 艺术家
        /// </summary>
        Artist = 5,
        /// <summary>
        /// 新歌
        /// </summary>
        NewSongs = 6,
        /// <summary>
        /// 虾米猜
        /// </summary>
        Guess = 8,
        /// <summary>
        /// 每日推荐歌单
        /// </summary>
        DailyRecommendaition = 9,
        /// <summary>
        /// 大类风格电台
        /// </summary>
        Style = 12,
        /// <summary>
        /// 风格电台
        /// </summary>
        Genre = 13,
        /// <summary>
        /// 心情电台
        /// </summary>
        Mood = 16
    }
}
 