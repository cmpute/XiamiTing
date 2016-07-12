namespace JacobC.Xiami.Services
{
    /// <summary>
    /// 确定消息类型的枚举
    /// </summary>
    public enum MediaMessageTypes
    {
        AppResumed,
        AppSuspended,
        BackgroundAudioTaskStarted,
        SkipNext,
        SkipPrevious,
        StartPlayback,
        TrackChanged,
        UpdatePlaylist
    }
}