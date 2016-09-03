using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JacobC.Xiami.Models
{
    public class MVModel : LovableModelBase<string>
    {
        public static MVModel GetNew(string key)
        {
            return new MVModel() { XiamiID = key };
        }
        private MVModel() { }
    }
}
