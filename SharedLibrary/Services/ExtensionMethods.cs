using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Services.SettingsService;

namespace JacobC.Xiami.Services
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// 读取设置后删除该设置
        /// </summary>
        public static T ReadAndReset<T>(this SettingsHelper setting,string key, T otherwise = default(T), SettingsStrategies strategy = SettingsStrategies.Local)
        {
            T val = setting.Read<T>(key,otherwise, strategy);
            setting.Remove(key, strategy);
            return val;
        }
    }
}
