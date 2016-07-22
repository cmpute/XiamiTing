using JacobC.Xiami.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;

namespace JacobC.Xiami
{
    public class SafeBindableBase : BindableBase
    {
        public override void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            try
            {
                base.RaisePropertyChanged(propertyName);
            }
            catch (Exception e)
            {
                LogService.ErrorWrite(e, "BindableBase");
            }
        }
    }
}
