using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace JacobC.Xiami.Services
{
    /// <summary>
    /// 获取虾米每日推荐的后台更新任务
    /// </summary>
    public sealed class RecommendUpdateTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            throw new NotImplementedException();
        }
    }
}
