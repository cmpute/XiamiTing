namespace JacobC.Xiami.Services
{
    /*
     * 将播放方法改成仅仅播放当前轨和下一轨，在更改下一轨时通知后台
     * 这样的话消息类型仅有Resumed,Suspended,BackgroundAudioTaskStarted
     * SkipNext,SkipPrevious,StartPlayback
     * TrackChanged,NextTrackChanged
     * 
     */
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