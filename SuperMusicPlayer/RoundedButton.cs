namespace SuperMusicPlayer;

/// <summary>
/// 圆角按钮 —— 通过 Region 裁剪实现圆角，FlatStyle.Flat 消除系统边框
/// </summary>
public class RoundedButton : Button
{
    private readonly Color _normalColor;
    private readonly Color _hoverColor;
    private readonly Color _textColor;
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

        MouseEnter += (s, e) => BackColor = _hoverColor;
        MouseLeave += (s, e) => BackColor = _normalColor;
        Resize += (s, e) => UpdateRegion();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        UpdateRegion();
    }

    private void UpdateRegion()
    {
        if (Width > 0 && Height > 0)
            Region = new Region(GetRoundRect(ClientRectangle, 10));
    }

    private static System.Drawing.Drawing2D.GraphicsPath GetRoundRect(Rectangle rect, int r)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        int d = r * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d - 1, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d - 1, rect.Bottom - d - 1, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d - 1, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
