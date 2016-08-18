namespace JacobC.Xiami.Services
{
    /*
     * 播放只用传递Xiamiid即可
     */
    /// <summary>
    /// 确定消息类型的枚举
    /// </summary>
    public enum MediaMessageTypes
    {
        //发向后台
        AppResumed,
        AppSuspended,
        StartTask,
        PlaySong,
        StartPlayback,
        //发向前台
        BackgroundAudioTaskStarted,
        SkipNext,
        SkipPrevious,
        TrackChanged
    }
}