namespace SuperMusicPlayer;

/// <summary>
/// 自定义圆角按钮 —— Tangerine Capital 风格的 Pill Button
/// 完全自绘，不依赖系统按钮渲染，彻底消除默认边框
/// </summary>
public class RoundedButton : Button
{
    private readonly Color _normalColor;
    private readonly Color _hoverColor;
    private readonly Color _textColor;
    private bool _isHovered;
    private bool _isPressed;

    public RoundedButton(Color normalColor, Color hoverColor, Color textColor)
    {
        _normalColor = normalColor;
        _hoverColor = hoverColor;
        _textColor = textColor;

        // 核心：完全接管绘制，禁止系统画任何东西（边框、背景等）
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.SupportsTransparentBackColor,
            true);
        UpdateStyles();

        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        FlatAppearance.BorderColor = normalColor; // 残余边框同色隐藏
        FlatAppearance.MouseDownBackColor = normalColor;
        FlatAppearance.MouseOverBackColor = normalColor;
        BackColor = Color.Transparent;
        ForeColor = textColor;
        Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
        Cursor = Cursors.Hand;
        Size = new Size(100, 40);

        MouseEnter += (s, e) => { _isHovered = true; Invalidate(); };
        MouseLeave += (s, e) => { _isHovered = false; _isPressed = false; Invalidate(); };
        MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { _isPressed = true; Invalidate(); } };
        MouseUp += (s, e) => { _isPressed = false; Invalidate(); };
        // 如果鼠标移出时仍按着，取消按下状态
        GotFocus += (s, e) => Invalidate();
        LostFocus += (s, e) => Invalidate();
    }

    /// <summary>阻止系统绘制默认背景/边框</summary>
    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        // 什么都不画 —— 背景由 OnPaint 的 FillPath 完全覆盖
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var rect = ClientRectangle;
        // 留 1px 边距，防止圆角被裁剪 + 消除锯齿外溢
        rect.Inflate(-1, -1);

        // 背景色：按下 > 悬停 > 正常
        var fillColor = _isPressed ? _hoverColor : (_isHovered ? _hoverColor : _normalColor);
        using var path = GetRoundedRect(rect, 10);
        using var brush = new SolidBrush(fillColor);
        e.Graphics.FillPath(brush, path);

        // 文字居中
        var textRect = ClientRectangle;
        using var textBrush = new SolidBrush(_textColor);
        var sf = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        e.Graphics.DrawString(Text, Font, textBrush, textRect, sf);

        // 焦点框（键盘导航时显示虚线框，鼠标点击时不显示）
        if (Focused && !_isPressed)
        {
            var focusRect = ClientRectangle;
            focusRect.Inflate(-4, -4);
            using var focusPath = GetRoundedRect(focusRect, 8);
            using var focusPen = new Pen(Color.FromArgb(100, _textColor), 1)
            {
                DashStyle = System.Drawing.Drawing2D.DashStyle.Dot
            };
            e.Graphics.DrawPath(focusPen, focusPath);
        }
    }

    private static System.Drawing.Drawing2D.GraphicsPath GetRoundedRect(Rectangle rect, int radius)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        int d = radius * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
