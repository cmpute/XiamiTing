namespace JacobC.Xiami.Services
{
    /// <summary>
    /// 指示前台应用程序的状态
    /// </summary>
    public enum AppState
    {
        Unknown,
        Active,
        Suspended
    }
    /// <summary>
    /// 指示后台任务的状态
    /// </summary>
    public enum BackgroundTaskState
    {
        Unknown,
        Started,
        Running,
        Canceled
    }
}