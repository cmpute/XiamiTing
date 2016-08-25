using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JacobC.Xiami.Models
{
    /// <summary>
    /// 表示有封面的内容的接口
    /// </summary>
    public interface ICovered
    {
        /// <summary>
        /// 封面图
        /// </summary>
        Uri Art { get; set; }
        /// <summary>
        /// 完整封面图
        /// </summary>
        Uri ArtFull { get; set; }
        /// <summary>
        /// 获取指定大小的封面地址
        /// </summary>
        /// <param name="size">封面大小对应的数字
        /// 专辑：0:原图 1:100x100 2:185x184(按比例) 3:55x55 4:原图 5:185x185
        /// 精选集： 0:原图 1:100x100 2:55x55
        /// 用户： 0:原图 1:55x55 2:100x100 3:200x240(按比例)
        /// </param>
        Uri GetArtWithSize(int sizecode);
    }
}
