namespace SuperMusicPlayer;

/// <summary>
/// 自定义圆角按钮 —— Tangerine Capital 风格的 Pill Button
/// 通过重写 OnPaint 绘制圆角矩形和自定义配色
/// </summary>
public class RoundedButton : Button
{
    private readonly Color _normalColor;
    private readonly Color _hoverColor;
    private readonly Color _textColor;
    private bool _isHovered;

    /// <summary>
    /// 创建圆角按钮
    /// </summary>
    /// <param name="normalColor">正常状态背景色</param>
    /// <param name="hoverColor">悬停状态背景色</param>
    /// <param name="textColor">文字颜色</param>
    public RoundedButton(Color normalColor, Color hoverColor, Color textColor)
    {
        _normalColor = normalColor;
        _hoverColor = hoverColor;
        _textColor = textColor;

        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        BackColor = normalColor;
        ForeColor = textColor;
        Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
        Cursor = Cursors.Hand;
        Size = new Size(100, 40);

        MouseEnter += (s, e) => { _isHovered = true; Invalidate(); };
        MouseLeave += (s, e) => { _isHovered = false; Invalidate(); };
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        // 绘制圆角矩形背景
        var color = _isHovered ? _hoverColor : _normalColor;
        using var brush = new SolidBrush(color);
        using var path = GetRoundedRect(ClientRectangle, 12);
        e.Graphics.FillPath(brush, path);

        // 绘制文字（居中）
        var textRect = ClientRectangle;
        textRect.Inflate(-4, 0);
        using var textBrush = new SolidBrush(_textColor);
        var sf = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        e.Graphics.DrawString(Text, Font, textBrush, textRect, sf);
    }

    /// <summary>生成圆角矩形 GraphicsPath</summary>
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
