using TagLib;

namespace SuperMusicPlayer;

/// <summary>
/// MP3 文件 ID3 标签读取器，封装 TagLib# 库的调用
/// </summary>
public static class MetadataReader
{
    /// <summary>
    /// 从指定路径读取音频文件的元数据并构造 Track 对象
    /// </summary>
    /// <param name="filePath">MP3 文件的完整路径</param>
    /// <returns>包含元数据的 Track 对象，读取失败时使用文件名作为标题</returns>
    public static Track ReadTrack(string filePath)
    {
        var track = new Track { FilePath = filePath };

        try
        {
            // 使用 TagLib# 读取 ID3 标签
            using var tagFile = TagLib.File.Create(filePath);

            // 读取音频属性（时长）
            track.Duration = tagFile.Properties.Duration;

            // 读取 ID3 标签信息
            var tag = tagFile.Tag;
            track.Title = string.IsNullOrWhiteSpace(tag.Title)
                ? Path.GetFileNameWithoutExtension(filePath)
                : tag.Title;

            track.Artist = string.Join(", ", tag.Performers);
            track.Album = tag.Album ?? string.Empty;
        }
        catch
        {
            // 读取失败时使用文件名作为标题，时长通过 NAudio 获取
            track.Title = Path.GetFileNameWithoutExtension(filePath);
        }

        return track;
    }
}
