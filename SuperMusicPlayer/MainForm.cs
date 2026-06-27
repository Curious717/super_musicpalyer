using System.ComponentModel;
using System.Windows.Forms;

namespace SuperMusicPlayer;

/// <summary>
/// 主窗体 —— Super Music Player 的完整 UI 实现
/// Tangerine Capital 设计风格：暖橘 + 奶油白 + 杂志排版感
/// 采用响应式布局：控件随窗体大小自适应居中
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

    // ==================== 核心组件 ====================
    private readonly AudioPlayer _player = new();
    private readonly PlaylistManager _playlist = new();
    private readonly System.Windows.Forms.Timer _posTimer = new();

    // ==================== 需要动态布局的容器 ====================
    private Panel _nowPlayingCard = null!;
    private Panel _controlBar = null!;
    private Panel _bottomBar = null!;

    // ==================== UI 控件 ====================
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
    private Label _lblVolume = null!;
    private Label _lblMode = null!;

    private bool _isDraggingProgress;
    private bool _isUserSeeking;
    private float _dragProgress;

    private const int DESIGN_CONTENT_WIDTH = 792;
    private const int SIDE_PADDING = 24;

    public MainForm()
    {
        InitializeUI();
        BindEvents();
        _posTimer.Interval = 250;
        _posTimer.Start();
        // 初始布局
        LayoutChildren();
    }

    // ==================== UI 初始化 ====================
    private void InitializeUI()
    {
        // 窗体基础设置
        Text = "Super Music Player";
        Size = new Size(860, 620);
        MinimumSize = new Size(620, 480);
        BackColor = Cream;
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Microsoft YaHei UI", 10F);

        // 响应式布局：Resize 时重新计算位置
        Resize += (s, e) => LayoutChildren();

        // === 顶部标题栏 ===
        var titleBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 48,
            BackColor = Tangerine
        };
        var lblAppTitle = new Label
        {
            Text = "🎵  Super Music Player",
            Font = new Font("Microsoft YaHei UI", 13F, FontStyle.Bold),
            ForeColor = White,
            AutoSize = true,
            Location = new Point(20, 12)
        };
        titleBar.Controls.Add(lblAppTitle);
        Controls.Add(titleBar);

        // === 当前播放信息卡片 ===
        _nowPlayingCard = new Panel { BackColor = White };
        _nowPlayingCard.Paint += DrawCardBg;

        _lblNowPlaying = new Label
        {
            Text = "♫  NOW PLAYING",
            Font = new Font("Microsoft YaHei UI", 8F, FontStyle.Bold),
            ForeColor = Tangerine,
            AutoSize = true,
            Location = new Point(20, 14)
        };
        _nowPlayingCard.Controls.Add(_lblNowPlaying);

        _lblTitle = new Label
        {
            Text = "未在播放",
            Font = new Font("Microsoft YaHei UI", 15F, FontStyle.Bold),
            ForeColor = TextDark,
            AutoSize = false,
            Location = new Point(20, 34),
            Size = new Size(0, 0), // 由 Layout 设置
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };
        _nowPlayingCard.Controls.Add(_lblTitle);

        _lblArtist = new Label
        {
            Text = "添加音乐文件开始播放",
            Font = new Font("Microsoft YaHei UI", 10F),
            ForeColor = TextGray,
            AutoSize = false,
            Location = new Point(20, 64),
            Size = new Size(0, 0), // 由 Layout 设置
            TextAlign = ContentAlignment.MiddleLeft
        };
        _nowPlayingCard.Controls.Add(_lblArtist);

        // 自定义进度条
        _progressBar = new Panel
        {
            Location = new Point(20, 98),
            Size = new Size(700, 6),
            Cursor = Cursors.Hand
        };
        _progressBar.Paint += DrawProgressBar;
        _progressBar.MouseDown += ProgressBar_MouseDown;
        _progressBar.MouseMove += ProgressBar_MouseMove;
        _progressBar.MouseUp += ProgressBar_MouseUp;
        _nowPlayingCard.Controls.Add(_progressBar);

        _lblCurrentTime = new Label
        {
            Text = "00:00",
            Font = new Font("Microsoft YaHei UI", 8F),
            ForeColor = TextGray,
            AutoSize = true,
            Location = new Point(20, 108)
        };
        _nowPlayingCard.Controls.Add(_lblCurrentTime);

        _lblTotalTime = new Label
        {
            Text = "00:00",
            Font = new Font("Microsoft YaHei UI", 8F),
            ForeColor = TextGray,
            AutoSize = true,
            TextAlign = ContentAlignment.TopRight
        };
        _nowPlayingCard.Controls.Add(_lblTotalTime);

        Controls.Add(_nowPlayingCard);

        // === 控制按钮行 ===
        _controlBar = new Panel { BackColor = Color.Transparent, Height = 48 };

        int btnY = 2;
        _btnPrev = new RoundedButton(White, TangerineLight, Tangerine)
        { Text = "⏮", Location = new Point(0, btnY), Size = new Size(40, 40) };

        _btnPlayPause = new RoundedButton(Tangerine, TangerineDark, White)
        { Text = "▶", Location = new Point(48, btnY), Size = new Size(48, 40) };

        _btnStop = new RoundedButton(White, TangerineLight, Tangerine)
        { Text = "⏹", Location = new Point(104, btnY), Size = new Size(40, 40) };

        _btnNext = new RoundedButton(White, TangerineLight, Tangerine)
        { Text = "⏭", Location = new Point(152, btnY), Size = new Size(40, 40) };

        _controlBar.Controls.AddRange([_btnPrev, _btnPlayPause, _btnStop, _btnNext]);

        // 音量
        _lblVolume = new Label
        {
            Text = "🔊",
            Font = new Font("Microsoft YaHei UI", 10F),
            Location = new Point(210, 12),
            AutoSize = true
        };
        _controlBar.Controls.Add(_lblVolume);

        _volumeSlider = new TrackBar
        {
            Location = new Point(240, 8),
            Size = new Size(90, 30),
            Minimum = 0,
            Maximum = 100,
            Value = 80,
            TickStyle = TickStyle.None,
            BackColor = Cream
        };
        _controlBar.Controls.Add(_volumeSlider);

        // 播放模式
        _lblMode = new Label
        {
            Text = "模式",
            Font = new Font("Microsoft YaHei UI", 9F),
            ForeColor = TextGray,
            Location = new Point(340, 14),
            AutoSize = true
        };
        _controlBar.Controls.Add(_lblMode);

        _cmbMode = new ComboBox
        {
            Location = new Point(372, 10),
            Size = new Size(90, 26),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Microsoft YaHei UI", 9F),
            FlatStyle = FlatStyle.Flat,
            BackColor = White,
            ForeColor = TextDark
        };
        _cmbMode.Items.AddRange(["顺序播放", "随机播放", "单曲循环", "列表循环"]);
        _cmbMode.SelectedIndex = 0;
        _controlBar.Controls.Add(_cmbMode);

        Controls.Add(_controlBar);

        // === 播放列表 ===
        _listView = new ListView
        {
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
        _listView.Columns.Add("", 38);
        _listView.Columns.Add("标题", 340);
        _listView.Columns.Add("歌手", 200);
        _listView.Columns.Add("时长", 70);

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
                    0 => StringAlignment.Center,
                    _ => StringAlignment.Near
                },
                LineAlignment = StringAlignment.Center
            };
            var textRect = e.Bounds;
            textRect.Inflate(-6, 0);
            e.Graphics.DrawString(e.SubItem?.Text ?? "", _listView.Font, textBrush, textRect, sf);
        };
        _listView.DrawItem += (s, e) => { e.DrawDefault = false; };

        _listView.AllowDrop = true;
        _listView.DragEnter += (s, e) =>
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                e.Effect = DragDropEffects.Copy;
        };
        _listView.DragDrop += ListView_DragDrop;

        var ctxMenu = new ContextMenuStrip();
        ctxMenu.Items.Add("播放", null, (s, e) => PlaySelected());
        ctxMenu.Items.Add("从列表移除", null, (s, e) => RemoveSelected());
        ctxMenu.Items.Add("-");
        ctxMenu.Items.Add("打开文件位置", null, (s, e) => OpenFileLocation());
        _listView.ContextMenuStrip = ctxMenu;

        Controls.Add(_listView);

        // === 底部操作栏 ===
        _bottomBar = new Panel { BackColor = Color.Transparent, Height = 44 };

        _btnAdd = new RoundedButton(Tangerine, TangerineDark, White)
        { Text = "＋ 添加文件", Location = new Point(0, 2), Size = new Size(105, 38) };

        _btnAddFolder = new RoundedButton(White, TangerineLight, Tangerine)
        { Text = "📁 添加文件夹", Location = new Point(112, 2), Size = new Size(115, 38) };

        _btnClear = new RoundedButton(White, TangerineLight, TextGray)
        { Text = "🗑 清空列表", Location = new Point(234, 2), Size = new Size(105, 38) };

        _lblCount = new Label
        {
            Text = "共 0 首",
            Font = new Font("Microsoft YaHei UI", 9F),
            ForeColor = TextGray,
            AutoSize = true
        };
        _bottomBar.Controls.AddRange([_btnAdd, _btnAddFolder, _btnClear, _lblCount]);
        Controls.Add(_bottomBar);

        // === 底部状态栏 ===
        _lblStatus = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 24,
            Text = "  就绪 — 添加 MP3 文件开始播放",
            Font = new Font("Microsoft YaHei UI", 8.5F),
            ForeColor = White,
            BackColor = TangerineDark,
            TextAlign = ContentAlignment.MiddleLeft
        };
        Controls.Add(_lblStatus);
    }

    /// <summary>响应式布局：计算居中偏移和各控件位置/尺寸</summary>
    private void LayoutChildren()
    {
        int titleBarH = 48;
        int statusH = 24;
        int availableW = ClientSize.Width - SIDE_PADDING * 2;
        if (availableW < 400) availableW = 400;
        int left = (ClientSize.Width - availableW) / 2;
        if (left < 0) left = 0;

        // 卡片高度: 固定130（标题区+进度条+时间）
        int cardH = 130;
        int cardTop = titleBarH + 12;
        _nowPlayingCard.SetBounds(left, cardTop, availableW, cardH);

        // 卡片内子控件
        _lblTitle.Size = new Size(availableW - 40, 26);
        _lblArtist.Size = new Size(availableW - 40, 22);
        _progressBar.Size = new Size(availableW - 40, 6);
        _lblTotalTime.Location = new Point(availableW - 40 - _lblTotalTime.Width, 108);

        // 控制栏
        int ctrlTop = cardTop + cardH + 8;
        _controlBar.SetBounds(left, ctrlTop, availableW, 48);

        // 音量滑块位置根据可用宽度动态调整
        _volumeSlider.Left = Math.Min(240, availableW - 180);
        _lblVolume.Left = _volumeSlider.Left - 30;
        _lblMode.Left = _volumeSlider.Right + 10;
        _cmbMode.Left = _lblMode.Right + 6;

        // 播放列表
        int listTop = ctrlTop + 56;
        int bottomH = 46;
        int statusTop = ClientSize.Height - statusH;
        int listBottom = statusTop - bottomH - 8;
        _listView.SetBounds(left, listTop, availableW, listBottom - listTop);

        // 底部栏
        _bottomBar.SetBounds(left, listBottom + 4, availableW, bottomH);
        _lblCount.Location = new Point(availableW - _lblCount.Width - 8, 12);
    }

    /// <summary>绘制卡片圆角背景（无黑边）</summary>
    private static void DrawCardBg(object? sender, PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        var panel = (Panel)sender!;
        var rect = panel.ClientRectangle;
        // 缩小 1px 避免边缘溢出
        rect.Inflate(-1, -1);
        using var path = GetRoundRect(rect, 12);
        using var brush = new SolidBrush(White);
        e.Graphics.FillPath(brush, path);
        // 极浅的暖色边框，替代黑色边框
        using var pen = new Pen(Color.FromArgb(60, 210, 180, 140), 1);
        e.Graphics.DrawPath(pen, path);
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
            _lblStatus.Text = state switch
            {
                PlaybackState.Playing => "  正在播放...",
                PlaybackState.Paused => "  已暂停",
                _ => "  就绪"
            };
        };

        _player.PlaybackFinished += () =>
        {
            BeginInvoke(() => PlayNext());
        };

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

        _listView.DoubleClick += (s, e) => PlaySelected();

        _volumeSlider.ValueChanged += (s, e) =>
            _player.Volume = _volumeSlider.Value / 100f;

        _cmbMode.SelectedIndexChanged += (s, e) =>
            _playlist.Mode = (PlayMode)_cmbMode.SelectedIndex;

        _posTimer.Tick += (s, e) => _player.NotifyPosition();

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

        using var trackPath = GetRoundRect(rect, 3);
        using var trackBrush = new SolidBrush(ProgressTrack);
        e.Graphics.FillPath(trackBrush, trackPath);

        double progress = _isUserSeeking ? _dragProgress : _player.Progress;
        int fillWidth = (int)(rect.Width * progress);
        if (fillWidth > 0)
        {
            var fillRect = new Rectangle(0, 0, Math.Min(fillWidth, rect.Width), rect.Height);
            using var fillPath = GetRoundRect(fillRect, 3);
            using var fillBrush = new SolidBrush(Tangerine);
            e.Graphics.FillPath(fillBrush, fillPath);
        }

        int dotX = (int)(rect.Width * progress) - 4;
        if (dotX < 0) dotX = 0;
        if (dotX > rect.Width - 8) dotX = rect.Width - 8;
        using var dotBrush = new SolidBrush(TangerineDark);
        e.Graphics.FillEllipse(dotBrush, dotX, -2, 10, 10);
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
