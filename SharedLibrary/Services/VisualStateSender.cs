using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace JacobC.Xiami.Services
{
    /// <summary>
    /// 通过Tag标签传递的VisualStatue枚举
    /// </summary>
    public enum VisualStateSender
    {
        Normal,
        Selected,
        Hover,
        HoverSelected
    }

    public class VisualStateConverter : IMultiValueConverter
    {
        /// <param name="value">请传入VisualStateSender值</param>
        /// <param name="parameter">请传入当前音轨是否在播放或是否喜欢的属性，是Bool值</param>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            VisualStateSender svalue = (VisualStateSender)value;
            bool bpara = IgnoreParameter ? false : (bool)parameter ^ ParameterReverse;
            if (svalue == Mode || svalue == VisualStateSender.HoverSelected)
            {
                if (bpara)
                    return ValueAndParameter ?? Otherwise;
                else
                    return ValueTrue ?? Otherwise;
            }
            else
            {
                if (bpara)
                    return ParameterTrue ?? Otherwise;
                else
                    return Otherwise;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Value模式匹配时的值
        /// </summary>
        public object ValueTrue { get; set; }
        /// <summary>
        /// Parameter为True时的值
        /// </summary>
        public object ParameterTrue { get; set; }
        /// <summary>
        /// Value匹配且Parameter为True时的值
        /// </summary>
        public object ValueAndParameter { get; set; }
        /// <summary>
        /// 均不满足以及未设置时的值
        /// </summary>
        public object Otherwise { get; set; }
        /// <summary>
        /// 选择value识别的<see cref="VisualStateSender"/>模式，Hover/Selected
        /// </summary>
        public VisualStateSender Mode { get; set; } //选择识别的模式
        /// <summary>
        /// 设置Parameter的值是否取反
        /// </summary>
        public bool ParameterReverse { get; set; } = false;
        /// <summary>
        /// 忽略Paramter，只有ValueTrue属性生效。此时Parameter设置成false
        /// </summary>
        public bool IgnoreParameter { get; set; } = false;
    }
}
