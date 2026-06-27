using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace SuperMusicPlayer;

/// <summary>
/// 圆角面板 —— 通过 Region 裁剪消除矩形背景色块
/// </summary>
public class RoundedPanel : Panel
{
    private int _radius = 12;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int CornerRadius
    {
        get => _radius;
        set { _radius = value; UpdateRegion(); }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        UpdateRegion();
    }

    private void UpdateRegion()
    {
        if (Width > 0 && Height > 0)
            Region = new Region(GetRoundRect(ClientRectangle, _radius));
    }

    private static GraphicsPath GetRoundRect(Rectangle rect, int r)
    {
        var path = new GraphicsPath();
        int d = r * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d - 1, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d - 1, rect.Bottom - d - 1, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d - 1, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
