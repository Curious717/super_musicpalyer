namespace SuperMusicPlayer;

/// <summary>
/// 无边框圆角面板 —— 完全自绘，不显示系统默认边框
/// </summary>
public class NoBorderPanel : Panel
{
    public NoBorderPanel()
    {
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.SupportsTransparentBackColor,
            true);
        UpdateStyles();
        BackColor = Color.Transparent;
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // 不画默认背景：完全由 Paint 事件控制
    }
}
