using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace JacobC.Xiami
{
    public interface IIncrementalLoadingCollection<T> : ISupportIncrementalLoading, ICollection<T> { }
}
