namespace SuperMusicPlayer;

/// <summary>
/// 音乐文件模型，封装单首歌曲的元数据与文件信息
/// </summary>
public class Track
{
    /// <summary>文件系统路径</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>歌曲标题（读取自 ID3 标签，若不存在则使用文件名）</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>艺术家/歌手</summary>
    public string Artist { get; set; } = string.Empty;

    /// <summary>专辑名称</summary>
    public string Album { get; set; } = string.Empty;

    /// <summary>音频总时长</summary>
    public TimeSpan Duration { get; set; }

    /// <summary>格式化时长字符串 (MM:SS)</summary>
    public string DurationString =>
        $"{(int)Duration.TotalMinutes:D2}:{Duration.Seconds:D2}";

    /// <summary>列表显示用的简短描述</summary>
    public string DisplayInfo =>
        string.IsNullOrEmpty(Artist) ? Title : $"{Title} — {Artist}";

    public override string ToString() => DisplayInfo;
}
