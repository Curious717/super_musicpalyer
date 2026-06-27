namespace SuperMusicPlayer;

/// <summary>播放模式枚举</summary>
public enum PlayMode
{
    /// <summary>顺序播放：播完最后一首后停止</summary>
    Sequential,
    /// <summary>随机播放：随机选取下一首</summary>
    Shuffle,
    /// <summary>单曲循环：不断重复当前歌曲</summary>
    RepeatOne,
    /// <summary>列表循环：播完最后一首后从第一首继续</summary>
    RepeatAll
}

/// <summary>
/// 播放列表管理器 —— 管理歌曲列表、当前播放位置和播放模式
/// 设计思路：将播放列表逻辑与音频播放逻辑分离，遵循单一职责原则，
/// PlaylistManager 只关心"播哪一首"和"什么顺序播"，不关心"怎么播"
/// </summary>
public class PlaylistManager
{
    private readonly List<Track> _tracks = new();
    private readonly Random _random = new();
    private int _currentIndex = -1;

    /// <summary>只读歌曲列表</summary>
    public IReadOnlyList<Track> Tracks => _tracks;

    /// <summary>当前播放索引，-1 表示无当前曲目</summary>
    public int CurrentIndex => _currentIndex;

    /// <summary>当前播放的歌曲，无则返回 null</summary>
    public Track? CurrentTrack =>
        _currentIndex >= 0 && _currentIndex < _tracks.Count
            ? _tracks[_currentIndex] : null;

    /// <summary>列表中的歌曲数量</summary>
    public int Count => _tracks.Count;

    /// <summary>播放模式</summary>
    public PlayMode Mode { get; set; } = PlayMode.Sequential;

    /// <summary>当前曲目发生变化时触发</summary>
    public event Action<Track?>? CurrentTrackChanged;

    /// <summary>列表内容变化时触发</summary>
    public event Action? PlaylistChanged;

    // ==================== 列表管理 ====================

    /// <summary>添加单首歌曲（自动去重）</summary>
    public void AddTrack(Track track)
    {
        if (_tracks.Any(t => t.FilePath == track.FilePath)) return;
        _tracks.Add(track);
        PlaylistChanged?.Invoke();
    }

    /// <summary>批量添加歌曲（自动去重）</summary>
    public void AddTracks(IEnumerable<Track> tracks)
    {
        int added = 0;
        foreach (var track in tracks)
        {
            if (_tracks.Any(t => t.FilePath == track.FilePath)) continue;
            _tracks.Add(track);
            added++;
        }
        if (added > 0) PlaylistChanged?.Invoke();
    }

    /// <summary>移除指定索引的歌曲</summary>
    public void RemoveTrack(int index)
    {
        if (index < 0 || index >= _tracks.Count) return;
        _tracks.RemoveAt(index);

        // 调整当前索引
        if (index < _currentIndex) _currentIndex--;
        else if (index == _currentIndex)
        {
            _currentIndex = _tracks.Count > 0 ? Math.Min(index, _tracks.Count - 1) : -1;
            CurrentTrackChanged?.Invoke(CurrentTrack);
        }
        PlaylistChanged?.Invoke();
    }

    /// <summary>清空播放列表</summary>
    public void Clear()
    {
        _tracks.Clear();
        _currentIndex = -1;
        CurrentTrackChanged?.Invoke(null);
        PlaylistChanged?.Invoke();
    }

    // ==================== 播放导航 ====================

    /// <summary>切换到指定索引的歌曲</summary>
    /// <returns>是否成功切换</returns>
    public bool MoveTo(int index)
    {
        if (index < 0 || index >= _tracks.Count) return false;
        _currentIndex = index;
        CurrentTrackChanged?.Invoke(CurrentTrack);
        return true;
    }

    /// <summary>切换到下一首（根据当前播放模式决定）</summary>
    /// <returns>是否成功获取到下一首，返回 false 表示播放已结束</returns>
    public bool MoveToNext()
    {
        if (_tracks.Count == 0) return false;

        switch (Mode)
        {
            case PlayMode.RepeatOne:
                // 单曲循环：保持在当前索引，触发变化事件让播放器重新加载
                CurrentTrackChanged?.Invoke(CurrentTrack);
                return true;

            case PlayMode.Shuffle:
                // 随机播放：随机选取一个不同于当前的索引
                if (_tracks.Count == 1)
                {
                    _currentIndex = 0;
                }
                else
                {
                    int next;
                    do { next = _random.Next(_tracks.Count); }
                    while (next == _currentIndex && _tracks.Count > 1);
                    _currentIndex = next;
                }
                CurrentTrackChanged?.Invoke(CurrentTrack);
                return true;

            case PlayMode.RepeatAll:
                // 列表循环：到末尾后回到开头
                _currentIndex = (_currentIndex + 1) % _tracks.Count;
                CurrentTrackChanged?.Invoke(CurrentTrack);
                return true;

            case PlayMode.Sequential:
            default:
                // 顺序播放：下一首或停止
                if (_currentIndex + 1 < _tracks.Count)
                {
                    _currentIndex++;
                    CurrentTrackChanged?.Invoke(CurrentTrack);
                    return true;
                }
                return false; // 列表结束
        }
    }

    /// <summary>切换到上一首</summary>
    public bool MoveToPrevious()
    {
        if (_tracks.Count == 0) return false;

        if (Mode == PlayMode.Shuffle)
        {
            int prev;
            do { prev = _random.Next(_tracks.Count); }
            while (prev == _currentIndex && _tracks.Count > 1);
            _currentIndex = prev;
        }
        else if (Mode == PlayMode.RepeatOne)
        {
            // 单曲循环模式：保持当前索引不变，触发事件让播放器重新加载
        }
        else
        {
            _currentIndex--;
            if (_currentIndex < 0) _currentIndex = _tracks.Count - 1;
        }

        CurrentTrackChanged?.Invoke(CurrentTrack);
        return true;
    }
}
