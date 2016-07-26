using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JacobC.Xiami.Models
{
    //TODO: 由于评论结构比较复杂，之后再处理，包括评论和留言
    public class CommentModel : SafeBindableBase
    {
        public CommentModel ReplyTo { get; set; }
    }
}
