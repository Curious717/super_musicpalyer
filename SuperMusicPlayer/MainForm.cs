using System.ComponentModel;
using System.Windows.Forms;

namespace SuperMusicPlayer;

/// <summary>
/// 主窗体 —— Super Music Player 的完整 UI 实现
/// Tangerine Capital 设计风格：暖橘 + 奶油白 + 杂志排版感
/// </summary>
public class MainForm : Form
{
    // ==================== Tangerine Capital 配色常量 ====================
    private static readonly Color Cream = Color.FromArgb(255, 248, 240);
    private static readonly Color Tangerine = Color.FromArgb(242, 140, 40);
    private static readonly Color TangerineDark = Color.FromArgb(224, 123, 32);
    private static readonly Color TangerineLight = Color.FromArgb(255, 240, 224);
    private static readonly Color White = Color.White;
    private static readonly Color TextDark = Color.FromArgb(44, 24, 16);
    private static readonly Color TextGray = Color.FromArgb(139, 115, 85);
    private static readonly Color ProgressTrack = Color.FromArgb(232, 213, 196);
    private static readonly Color Gold = Color.FromArgb(255, 215, 0);

    // ==================== 核心组件 ====================
    private readonly AudioPlayer _player = new();
    private readonly PlaylistManager _playlist = new();
    private readonly System.Windows.Forms.Timer _posTimer = new();

    // ==================== UI 控件 ====================
    private Panel _titleBar = null!;
    private Label _lblNowPlaying = null!;
    private Label _lblTitle = null!;
    private Label _lblArtist = null!;
    private Panel _progressBar = null!;
    private Label _lblCurrentTime = null!;
    private Label _lblTotalTime = null!;
    private ListView _listView = null!;
    private TrackBar _volumeSlider = null!;
    private ComboBox _cmbMode = null!;
    private Label _lblStatus = null!;
    private Label _lblCount = null!;
    private RoundedButton _btnPlayPause = null!;
    private RoundedButton _btnStop = null!;
    private RoundedButton _btnPrev = null!;
    private RoundedButton _btnNext = null!;
    private RoundedButton _btnAdd = null!;
    private RoundedButton _btnAddFolder = null!;
    private RoundedButton _btnClear = null!;

    private bool _isDraggingProgress;
    private bool _isUserSeeking;
    private float _dragProgress;

    public MainForm()
    {
        InitializeUI();
        BindEvents();
        _posTimer.Interval = 250;
        _posTimer.Start();
    }

    // ==================== UI 初始化 ====================
    private void InitializeUI()
    {
        // 窗体基础设置
        Text = "Super Music Player";
        Size = new Size(860, 620);
        MinimumSize = new Size(600, 400);
        BackColor = Cream;
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Microsoft YaHei UI", 10F);
        Icon = SystemIcons.WinLogo;

        // === 顶部标题栏 ===
        _titleBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 52,
            BackColor = Tangerine,
            Padding = new Padding(20, 0, 20, 0)
        };
        var lblAppTitle = new Label
        {
            Text = "Super Music Player",
            Font = new Font("Microsoft YaHei UI", 14F, FontStyle.Bold),
            ForeColor = White,
            AutoSize = true,
            Location = new Point(20, 12)
        };
        _titleBar.Controls.Add(lblAppTitle);
        Controls.Add(_titleBar);

        // === 主体内容区 ===
        var mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Cream,
            Padding = new Padding(24, 16, 24, 16)
        };

        // --- 当前播放信息卡片 ---
        var nowPlayingCard = CreateCard(24, 16, 792, 140);

        _lblNowPlaying = new Label
        {
            Text = "♫  NOW PLAYING",
            Font = new Font("Microsoft YaHei UI", 8F, FontStyle.Bold),
            ForeColor = Tangerine,
            Location = new Point(24, 14),
            AutoSize = true
        };
        nowPlayingCard.Controls.Add(_lblNowPlaying);

        _lblTitle = new Label
        {
            Text = "未在播放",
            Font = new Font("Microsoft YaHei UI", 16F, FontStyle.Bold),
            ForeColor = TextDark,
            Location = new Point(24, 36),
            AutoSize = true,
            MaximumSize = new Size(740, 28)
        };
        nowPlayingCard.Controls.Add(_lblTitle);

        _lblArtist = new Label
        {
            Text = "添加音乐文件开始播放",
            Font = new Font("Microsoft YaHei UI", 11F),
            ForeColor = TextGray,
            Location = new Point(24, 68),
            AutoSize = true
        };
        nowPlayingCard.Controls.Add(_lblArtist);

        // 自定义进度条
        _progressBar = new Panel
        {
            Location = new Point(24, 100),
            Size = new Size(620, 8),
            BackColor = ProgressTrack,
            Cursor = Cursors.Hand
        };
        _progressBar.Paint += DrawProgressBar;
        _progressBar.MouseDown += ProgressBar_MouseDown;
        _progressBar.MouseMove += ProgressBar_MouseMove;
        _progressBar.MouseUp += ProgressBar_MouseUp;
        nowPlayingCard.Controls.Add(_progressBar);

        _lblCurrentTime = new Label
        {
            Text = "00:00",
            Font = new Font("Microsoft YaHei UI", 9F),
            ForeColor = TextGray,
            Location = new Point(24, 112),
            AutoSize = true
        };
        nowPlayingCard.Controls.Add(_lblCurrentTime);

        _lblTotalTime = new Label
        {
            Text = "00:00",
            Font = new Font("Microsoft YaHei UI", 9F),
            ForeColor = TextGray,
            Location = new Point(590, 112),
            AutoSize = true,
            TextAlign = ContentAlignment.TopRight
        };
        nowPlayingCard.Controls.Add(_lblTotalTime);

        mainPanel.Controls.Add(nowPlayingCard);

        // --- 控制按钮行 ---
        var controlBar = new Panel
        {
            Location = new Point(24, 176),
            Size = new Size(792, 52),
            BackColor = Color.Transparent
        };

        // 播放控制按钮（pill buttons）
        int btnY = 4;
        _btnPrev = new RoundedButton(White, TangerineLight, Tangerine)
        { Text = "⏮", Location = new Point(0, btnY), Size = new Size(44, 44) };

        _btnPlayPause = new RoundedButton(Tangerine, TangerineDark, White)
        { Text = "▶", Location = new Point(54, btnY), Size = new Size(52, 44) };

        _btnStop = new RoundedButton(White, TangerineLight, Tangerine)
        { Text = "⏹", Location = new Point(116, btnY), Size = new Size(44, 44) };

        _btnNext = new RoundedButton(White, TangerineLight, Tangerine)
        { Text = "⏭", Location = new Point(170, btnY), Size = new Size(44, 44) };

        controlBar.Controls.AddRange([_btnPrev, _btnPlayPause, _btnStop, _btnNext]);

        // 分隔线
        var sep = new Label
        {
            Text = "│",
            Font = new Font("Microsoft YaHei UI", 14F),
            ForeColor = ProgressTrack,
            Location = new Point(228, 10),
            AutoSize = true
        };
        controlBar.Controls.Add(sep);

        // 音量标签
        var lblVolume = new Label
        {
            Text = "🔊",
            Font = new Font("Microsoft YaHei UI", 12F),
            Location = new Point(250, 14),
            AutoSize = true
        };
        controlBar.Controls.Add(lblVolume);

        // 音量滑块
        _volumeSlider = new TrackBar
        {
            Location = new Point(280, 12),
            Size = new Size(100, 30),
            Minimum = 0,
            Maximum = 100,
            Value = 80,
            TickStyle = TickStyle.None,
            BackColor = Cream
        };
        controlBar.Controls.Add(_volumeSlider);

        // 播放模式选择器
        var lblMode = new Label
        {
            Text = "模式",
            Font = new Font("Microsoft YaHei UI", 9F),
            ForeColor = TextGray,
            Location = new Point(395, 16),
            AutoSize = true
        };
        controlBar.Controls.Add(lblMode);

        _cmbMode = new ComboBox
        {
            Location = new Point(430, 12),
            Size = new Size(90, 28),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Microsoft YaHei UI", 9F),
            FlatStyle = FlatStyle.Flat,
            BackColor = White,
            ForeColor = TextDark
        };
        _cmbMode.Items.AddRange(["顺序播放", "随机播放", "单曲循环", "列表循环"]);
        _cmbMode.SelectedIndex = 0;
        controlBar.Controls.Add(_cmbMode);

        mainPanel.Controls.Add(controlBar);

        // --- 播放列表 (ListView) ---
        _listView = new ListView
        {
            Location = new Point(24, 236),
            Size = new Size(792, 220),
            View = View.Details,
            FullRowSelect = true,
            MultiSelect = true,
            BackColor = White,
            ForeColor = TextDark,
            Font = new Font("Microsoft YaHei UI", 9.5F),
            BorderStyle = BorderStyle.None,
            HeaderStyle = ColumnHeaderStyle.Nonclickable,
            OwnerDraw = true,
            GridLines = false
        };
        _listView.Columns.Add("", 40);
        _listView.Columns.Add("标题", 350);
        _listView.Columns.Add("歌手", 220);
        _listView.Columns.Add("时长", 80);

        // 手动绘制 ListView 项（实现卡片风格的交替行颜色）
        _listView.DrawColumnHeader += (s, e) =>
        {
            e.Graphics.FillRectangle(new SolidBrush(White), e.Bounds);
            using var headerFont = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            using var brush = new SolidBrush(TextGray);
            e.Graphics.DrawString(
                _listView.Columns[e.ColumnIndex].Text,
                headerFont, brush, e.Bounds.X + 8, e.Bounds.Y + 6);
        };

        _listView.DrawSubItem += (s, e) =>
        {
            if (e.Item == null) return;
            var isEven = e.Item.Index % 2 == 0;
            var bgColor = isEven ? White : Color.FromArgb(252, 244, 236);
            var isSelected = e.Item.Selected;

            using var bgBrush = new SolidBrush(isSelected ? TangerineLight : bgColor);
            e.Graphics.FillRectangle(bgBrush, e.Bounds);

            using var textBrush = new SolidBrush(TextDark);
            var sf = new StringFormat
            {
                Alignment = e.ColumnIndex switch
                {
                    0 => StringAlignment.Center,  // 序号居中
                    3 => StringAlignment.Center,  // 时长居中
                    _ => StringAlignment.Near
                },
                LineAlignment = StringAlignment.Center
            };
            var textRect = e.Bounds;
            textRect.Inflate(-6, 0);
            e.Graphics.DrawString(e.SubItem?.Text ?? "", _listView.Font, textBrush, textRect, sf);
        };
        _listView.DrawItem += (s, e) => { e.DrawDefault = false; };

        // 允许拖放文件
        _listView.AllowDrop = true;
        _listView.DragEnter += (s, e) =>
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                e.Effect = DragDropEffects.Copy;
        };
        _listView.DragDrop += ListView_DragDrop;

        // 右键菜单
        var ctxMenu = new ContextMenuStrip();
        ctxMenu.Items.Add("播放", null, (s, e) => PlaySelected());
        ctxMenu.Items.Add("从列表移除", null, (s, e) => RemoveSelected());
        ctxMenu.Items.Add("-");
        ctxMenu.Items.Add("打开文件位置", null, (s, e) => OpenFileLocation());
        _listView.ContextMenuStrip = ctxMenu;

        mainPanel.Controls.Add(_listView);

        // --- 底部操作栏 ---
        var bottomBar = new Panel
        {
            Location = new Point(24, 462),
            Size = new Size(792, 44),
            BackColor = Color.Transparent
        };

        _btnAdd = new RoundedButton(Tangerine, TangerineDark, White)
        { Text = "＋ 添加文件", Location = new Point(0, 2), Size = new Size(110, 40) };

        _btnAddFolder = new RoundedButton(White, TangerineLight, Tangerine)
        { Text = "📁 添加文件夹", Location = new Point(120, 2), Size = new Size(120, 40) };

        _btnClear = new RoundedButton(White, TangerineLight, TextGray)
        { Text = "🗑 清空列表", Location = new Point(250, 2), Size = new Size(110, 40) };

        _lblCount = new Label
        {
            Text = "共 0 首",
            Font = new Font("Microsoft YaHei UI", 9F),
            ForeColor = TextGray,
            Location = new Point(700, 14),
            AutoSize = true
        };
        bottomBar.Controls.AddRange([_btnAdd, _btnAddFolder, _btnClear, _lblCount]);
        mainPanel.Controls.Add(bottomBar);

        Controls.Add(mainPanel);

        // === 底部状态栏 ===
        _lblStatus = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 26,
            Text = "  就绪 — 添加 MP3 文件开始播放",
            Font = new Font("Microsoft YaHei UI", 8.5F),
            ForeColor = White,
            BackColor = TangerineDark,
            TextAlign = ContentAlignment.MiddleLeft
        };
        Controls.Add(_lblStatus);

        // 设置 tab 顺序
        _btnAdd.TabIndex = 0;
        _btnAddFolder.TabIndex = 1;
        _listView.TabIndex = 2;
        _btnPlayPause.TabIndex = 3;
    }

    /// <summary>创建白色圆角卡片面板</summary>
    private static Panel CreateCard(int x, int y, int w, int h)
    {
        var card = new Panel
        {
            Location = new Point(x, y),
            Size = new Size(w, h),
            BackColor = White,
            Padding = new Padding(8)
        };
        card.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var path = GetRoundRect(card.ClientRectangle, 14);
            using var brush = new SolidBrush(White);
            e.Graphics.FillPath(brush, path);
            // 轻微阴影效果
            using var pen = new Pen(Color.FromArgb(30, 0, 0, 0), 1);
            e.Graphics.DrawPath(pen, path);
        };
        return card;
    }

    private static System.Drawing.Drawing2D.GraphicsPath GetRoundRect(Rectangle rect, int r)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        int d = r * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    // ==================== 事件绑定 ====================
    private void BindEvents()
    {
        // 播放器事件
        _player.PositionChanged += (cur, total) =>
        {
            if (!_isUserSeeking)
            {
                _lblCurrentTime.Text = FormatTime(cur);
                _lblTotalTime.Text = FormatTime(total);
                _progressBar.Invalidate();
            }
        };

        _player.StateChanged += state =>
        {
            _btnPlayPause.Text = state == PlaybackState.Playing ? "⏸" : "▶";
            _btnPlayPause.BackColor = state == PlaybackState.Playing ? Tangerine : Tangerine;
            _lblStatus.Text = state switch
            {
                PlaybackState.Playing => "  正在播放...",
                PlaybackState.Paused => "  已暂停",
                _ => "  就绪"
            };
        };

        _player.PlaybackFinished += () =>
        {
            // NAudio 回调在非 UI 线程，需切换到 UI 线程
            BeginInvoke(() => PlayNext());
        };

        // 播放列表事件
        _playlist.CurrentTrackChanged += track =>
        {
            if (track != null)
            {
                _lblTitle.Text = track.Title;
                _lblArtist.Text = string.IsNullOrEmpty(track.Artist) ? "未知歌手" : track.Artist;
                _lblNowPlaying.Text = "♫  NOW PLAYING";
                HighlightCurrentTrack();
            }
            else
            {
                _lblTitle.Text = "未在播放";
                _lblArtist.Text = "添加音乐文件开始播放";
                _lblNowPlaying.Text = "♫  NOW PLAYING";
            }
        };

        _playlist.PlaylistChanged += RefreshListView;

        // 按钮事件
        _btnPlayPause.Click += (s, e) =>
        {
            if (_player.State == PlaybackState.Playing)
                _player.Pause();
            else if (_player.State == PlaybackState.Paused)
                _player.Play();
            else if (_playlist.CurrentTrack != null)
                PlayCurrent();
        };

        _btnStop.Click += (s, e) => _player.Stop();
        _btnPrev.Click += (s, e) => PlayPrevious();
        _btnNext.Click += (s, e) => PlayNext();
        _btnAdd.Click += (s, e) => AddFiles();
        _btnAddFolder.Click += (s, e) => AddFolder();
        _btnClear.Click += (s, e) => { _playlist.Clear(); _player.Stop(); };

        // 双击列表播放
        _listView.DoubleClick += (s, e) => PlaySelected();

        // 音量
        _volumeSlider.ValueChanged += (s, e) =>
            _player.Volume = _volumeSlider.Value / 100f;

        // 播放模式
        _cmbMode.SelectedIndexChanged += (s, e) =>
            _playlist.Mode = (PlayMode)_cmbMode.SelectedIndex;

        // 定时器更新位置
        _posTimer.Tick += (s, e) => _player.NotifyPosition();

        // 键盘快捷键
        KeyPreview = true;
        KeyDown += (s, e) =>
        {
            switch (e.KeyCode)
            {
                case Keys.Space: _btnPlayPause.PerformClick(); e.Handled = true; break;
                case Keys.Left: _btnPrev.PerformClick(); e.Handled = true; break;
                case Keys.Right: _btnNext.PerformClick(); e.Handled = true; break;
            }
        };

        // 关闭时释放资源
        FormClosing += (s, e) =>
        {
            _posTimer.Stop();
            _player.Dispose();
        };
    }

    // ==================== 自定义进度条绘制 ====================
    private void DrawProgressBar(object? sender, PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        var rect = _progressBar.ClientRectangle;

        // 背景轨道
        using var trackPath = GetRoundRect(rect, 4);
        using var trackBrush = new SolidBrush(ProgressTrack);
        e.Graphics.FillPath(trackBrush, trackPath);

        // 进度填充
        double progress = _isUserSeeking ? _dragProgress : _player.Progress;
        int fillWidth = (int)(rect.Width * progress);
        if (fillWidth > 0)
        {
            var fillRect = new Rectangle(0, 0, Math.Min(fillWidth, rect.Width), rect.Height);
            using var fillPath = GetRoundRect(fillRect, 4);
            using var fillBrush = new SolidBrush(Tangerine);
            e.Graphics.FillPath(fillBrush, fillPath);
        }

        // 拖动圆点
        int dotX = (int)(rect.Width * progress) - 5;
        if (dotX < 0) dotX = 0;
        if (dotX > rect.Width - 10) dotX = rect.Width - 10;
        using var dotBrush = new SolidBrush(TangerineDark);
        e.Graphics.FillEllipse(dotBrush, dotX, -2, 12, 12);
    }

    private void ProgressBar_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && _player.TotalTime > TimeSpan.Zero)
        {
            _isUserSeeking = true;
            _isDraggingProgress = true;
            _dragProgress = (float)e.X / _progressBar.Width;
            _dragProgress = Math.Clamp(_dragProgress, 0f, 1f);
            _progressBar.Invalidate();

            // 实时更新时间显示
            var seekTime = TimeSpan.FromSeconds(_player.TotalTime.TotalSeconds * _dragProgress);
            _lblCurrentTime.Text = FormatTime(seekTime);
        }
    }

    private void ProgressBar_MouseMove(object? sender, MouseEventArgs e)
    {
        if (_isDraggingProgress)
        {
            _dragProgress = (float)e.X / _progressBar.Width;
            _dragProgress = Math.Clamp(_dragProgress, 0f, 1f);
            _progressBar.Invalidate();
            var seekTime = TimeSpan.FromSeconds(_player.TotalTime.TotalSeconds * _dragProgress);
            _lblCurrentTime.Text = FormatTime(seekTime);
        }
    }

    private void ProgressBar_MouseUp(object? sender, MouseEventArgs e)
    {
        if (_isDraggingProgress)
        {
            _isDraggingProgress = false;
            _isUserSeeking = false;
            var seekTime = TimeSpan.FromSeconds(_player.TotalTime.TotalSeconds * _dragProgress);
            _player.Seek(seekTime);
        }
    }

    // ==================== 播放控制 ====================
    private void PlayCurrent()
    {
        var track = _playlist.CurrentTrack;
        if (track == null) return;
        try
        {
            _player.LoadAndPlay(track.FilePath);
            _lblStatus.Text = $"  正在播放: {track.DisplayInfo}";
            HighlightCurrentTrack();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"播放失败：{ex.Message}", "错误",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void PlaySelected()
    {
        if (_listView.SelectedIndices.Count > 0)
        {
            _playlist.MoveTo(_listView.SelectedIndices[0]);
            PlayCurrent();
        }
    }

    private void PlayNext()
    {
        if (_playlist.MoveToNext())
            PlayCurrent();
        else
        {
            _lblStatus.Text = "  播放列表已结束";
            RefreshNowPlaying();
        }
    }

    private void PlayPrevious()
    {
        if (_playlist.MoveToPrevious())
            PlayCurrent();
    }

    private void RemoveSelected()
    {
        // 从大到小移除，避免索引偏移
        var indices = _listView.SelectedIndices.Cast<int>().OrderByDescending(i => i).ToList();
        foreach (var i in indices)
            _playlist.RemoveTrack(i);
    }

    private void OpenFileLocation()
    {
        var track = _playlist.CurrentTrack;
        if (track != null && File.Exists(track.FilePath))
        {
            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{track.FilePath}\"");
        }
    }

    // ==================== 文件操作 ====================
    private void AddFiles()
    {
        using var dlg = new OpenFileDialog
        {
            Title = "选择 MP3 音乐文件",
            Filter = "音频文件|*.mp3;*.wav;*.flac;*.m4a|MP3文件|*.mp3|所有文件|*.*",
            Multiselect = true
        };
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            var tracks = dlg.FileNames
                .Where(f => f.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
                .Select(MetadataReader.ReadTrack);
            _playlist.AddTracks(tracks);
            _lblStatus.Text = $"  已添加 {dlg.FileNames.Length} 个文件";
        }
    }

    private void AddFolder()
    {
        using var dlg = new FolderBrowserDialog
        {
            Description = "选择包含 MP3 文件的文件夹"
        };
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            var mp3Files = Directory.GetFiles(dlg.SelectedPath, "*.mp3",
                SearchOption.AllDirectories);
            if (mp3Files.Length == 0)
            {
                MessageBox.Show("所选文件夹中没有找到 MP3 文件。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var tracks = mp3Files.Select(MetadataReader.ReadTrack);
            _playlist.AddTracks(tracks);
            _lblStatus.Text = $"  已添加 {mp3Files.Length} 首歌曲";
        }
    }

    private void ListView_DragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            var tracks = files
                .Where(f => f.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
                .Select(MetadataReader.ReadTrack);
            _playlist.AddTracks(tracks);
        }
    }

    // ==================== UI 刷新 ====================
    private void RefreshListView()
    {
        _listView.BeginUpdate();
        _listView.Items.Clear();

        for (int i = 0; i < _playlist.Count; i++)
        {
            var t = _playlist.Tracks[i];
            var item = new ListViewItem((i + 1).ToString());
            item.SubItems.Add(t.Title);
            item.SubItems.Add(t.Artist);
            item.SubItems.Add(t.DurationString);
            _listView.Items.Add(item);
        }

        _listView.EndUpdate();
        _lblCount.Text = $"共 {_playlist.Count} 首";
        HighlightCurrentTrack();
    }

    private void HighlightCurrentTrack()
    {
        for (int i = 0; i < _listView.Items.Count; i++)
        {
            _listView.Items[i].BackColor = i == _playlist.CurrentIndex
                ? TangerineLight
                : (i % 2 == 0 ? White : Color.FromArgb(252, 244, 236));
            _listView.Items[i].ForeColor = i == _playlist.CurrentIndex
                ? TangerineDark
                : TextDark;
        }
    }

    private void RefreshNowPlaying()
    {
        _lblTitle.Text = "未在播放";
        _lblArtist.Text = "播放列表已结束";
        _lblCurrentTime.Text = "00:00";
        _lblTotalTime.Text = "00:00";
        _progressBar.Invalidate();
    }

    // ==================== 工具方法 ====================
    private static string FormatTime(TimeSpan t)
    {
        if (t.TotalHours >= 1)
            return $"{(int)t.TotalHours}:{t.Minutes:D2}:{t.Seconds:D2}";
        return $"{t.Minutes:D2}:{t.Seconds:D2}";
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _posTimer?.Dispose();
            _player?.Dispose();
        }
        base.Dispose(disposing);
    }
}
