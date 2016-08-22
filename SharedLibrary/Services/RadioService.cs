using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JacobC.Xiami.Models;

namespace JacobC.Xiami.Services
{
    /// <summary>
    /// 提供播放电台的服务
    /// </summary>
    public class RadioService
    {
        private RadioService() { }

        /// <summary>
        /// 虾米猜电台
        /// </summary>
        public static RadioService XiamiCai
        {
            get { throw new NotImplementedException(); }
        }
        /// <summary>
        /// 获取艺人电台
        /// </summary>
        public static RadioService GetFromArtist(ArtistModel artist)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 获取用户电台
        /// </summary>
        public static RadioService GetFromUser(UserModel user)
        {
            throw new NotImplementedException();
        }
    }
}
