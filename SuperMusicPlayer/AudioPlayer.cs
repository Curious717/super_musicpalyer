using NAudio.Wave;

namespace SuperMusicPlayer;

/// <summary>播放状态枚举</summary>
public enum PlaybackState
{
    Stopped,
    Playing,
    Paused
}

/// <summary>
/// 音频播放器 —— 封装 NAudio 库实现 MP3 音频播放控制
///
/// 设计思路：
/// - 使用 WaveOutEvent 作为音频输出设备（兼容性最好，无需 Wasapi）
/// - 使用 AudioFileReader 统一读取多种音频格式（MP3/WAV等）
/// - 通过 WaveStream.Position 实现精准定位（seek）
/// - 播放结束通过 PlaybackStopped 事件回调自动触发
///
/// NAudio 核心组件说明：
/// - WaveOutEvent：将音频数据推送到 Windows 音频设备
/// - AudioFileReader：封装了不同格式的解码器（MP3→Mp3FileReader）
/// - WaveStream：所有音频数据流的抽象基类，提供 Position/TotalTime 等属性
/// </summary>
public class AudioPlayer : IDisposable
{
    private WaveOutEvent? _waveOut;
    private AudioFileReader? _audioFile;
    private float _volume = 0.8f;

    /// <summary>当前播放状态</summary>
    public PlaybackState State { get; private set; } = PlaybackState.Stopped;

    /// <summary>音量（0.0 ~ 1.0）</summary>
    public float Volume
    {
        get => _volume;
        set
        {
            _volume = Math.Clamp(value, 0f, 1f);
            if (_audioFile != null)
                _audioFile.Volume = _volume;
        }
    }

    /// <summary>当前播放位置</summary>
    public TimeSpan CurrentTime =>
        _audioFile?.CurrentTime ?? TimeSpan.Zero;

    /// <summary>音频总时长</summary>
    public TimeSpan TotalTime =>
        _audioFile?.TotalTime ?? TimeSpan.Zero;

    /// <summary>当前播放进度百分比（0.0 ~ 1.0）</summary>
    public double Progress =>
        TotalTime.TotalSeconds > 0
            ? CurrentTime.TotalSeconds / TotalTime.TotalSeconds
            : 0;

    /// <summary>当前加载的文件路径</summary>
    public string? CurrentFilePath { get; private set; }

    // ==================== 事件 ====================

    /// <summary>播放位置更新（由外部 Timer 驱动，约 200ms 一次）</summary>
    public event Action<TimeSpan, TimeSpan>? PositionChanged;

    /// <summary>播放状态变更</summary>
    public event Action<PlaybackState>? StateChanged;

    /// <summary>当前曲目自然播放结束</summary>
    public event Action? PlaybackFinished;

    // ==================== 播放控制 ====================

    /// <summary>
    /// 加载并开始播放指定音频文件
    /// 如果已在播放中，先停止当前播放再加载新文件
    /// </summary>
    public void LoadAndPlay(string filePath)
    {
        if (!File.Exists(filePath)) return;

        Stop(); // 先停止当前播放并释放资源

        try
        {
            // AudioFileReader 自动识别 MP3/WAV 等格式并选择合适的解码器
            _audioFile = new AudioFileReader(filePath)
            {
                Volume = _volume
            };

            // WaveOutEvent 使用回调方式与 Windows 音频系统交互
            _waveOut = new WaveOutEvent();
            _waveOut.PlaybackStopped += OnPlaybackStopped;
            _waveOut.Init(_audioFile);
            _waveOut.Play();

            CurrentFilePath = filePath;
            State = PlaybackState.Playing;
            StateChanged?.Invoke(State);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"播放失败: {ex.Message}");
            Stop();
            throw;
        }
    }

    /// <summary>播放（从暂停状态恢复或从头开始）</summary>
    public void Play()
    {
        if (_waveOut == null || _audioFile == null) return;

        if (State == PlaybackState.Paused)
        {
            _waveOut.Play();
            State = PlaybackState.Playing;
            StateChanged?.Invoke(State);
        }
    }

    /// <summary>暂停播放（保留播放位置）</summary>
    public void Pause()
    {
        if (_waveOut == null || State != PlaybackState.Playing) return;

        _waveOut.Pause();
        State = PlaybackState.Paused;
        StateChanged?.Invoke(State);
    }

    /// <summary>停止播放并重置到文件开头</summary>
    public void Stop()
    {
        if (_waveOut != null)
        {
            _waveOut.PlaybackStopped -= OnPlaybackStopped;
            _waveOut.Stop();
            _waveOut.Dispose();
            _waveOut = null;
        }

        _audioFile?.Dispose();
        _audioFile = null;

        CurrentFilePath = null;
        State = PlaybackState.Stopped;
        StateChanged?.Invoke(State);
    }

    /// <summary>
    /// 跳转到指定播放位置
    /// 实现原理：AudioFileReader.Position 直接设置 WaveStream 的字节偏移，
    /// NAudio 内部会根据格式参数将 TimeSpan 转换为正确的字节位置
    /// </summary>
    public void Seek(TimeSpan position)
    {
        if (_audioFile == null) return;

        // 确保位置在有效范围内
        if (position < TimeSpan.Zero) position = TimeSpan.Zero;
        if (position > _audioFile.TotalTime) position = _audioFile.TotalTime;

        _audioFile.CurrentTime = position;
        PositionChanged?.Invoke(position, _audioFile.TotalTime);
    }

    /// <summary>触发位置更新事件（由 UI 定时器调用）</summary>
    public void NotifyPosition()
    {
        if (State == PlaybackState.Playing && _audioFile != null)
        {
            PositionChanged?.Invoke(_audioFile.CurrentTime, _audioFile.TotalTime);
        }
    }

    /// <summary>
    /// 播放停止回调 —— NAudio 内部线程触发
    /// 注意：此回调在 NAudio 的内部线程上执行，而非 UI 线程
    /// 区分"自然播放结束"与"手动 Stop()"：手动 Stop 前会先取消事件订阅
    /// </summary>
    private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        // 只有自然播放结束（非手动 Stop）才触发此回调
        if (State == PlaybackState.Playing)
        {
            State = PlaybackState.Stopped;
            // 在 NAudio 线程中清理，避免跨线程问题
            _waveOut?.Dispose();
            _waveOut = null;
            _audioFile?.Dispose();
            _audioFile = null;
            CurrentFilePath = null;

            // 通过事件通知外部（外部负责将处理切换到 UI 线程）
            PlaybackFinished?.Invoke();
        }
    }

    /// <summary>释放音频资源</summary>
    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }
}
