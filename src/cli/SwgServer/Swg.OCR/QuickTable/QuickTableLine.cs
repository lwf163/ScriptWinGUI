namespace Swg.OCR.QuickTable;

/// <summary>表格检测中的线段（图像坐标）。</summary>
public sealed class QuickTableLine
{
    /// <summary>端点 X1。</summary>
    public int X1 { get; set; }

    /// <summary>端点 Y1。</summary>
    public int Y1 { get; set; }

    /// <summary>端点 X2。</summary>
    public int X2 { get; set; }

    /// <summary>端点 Y2。</summary>
    public int Y2 { get; set; }

    /// <summary>构造线段。</summary>
    public QuickTableLine(int x1, int y1, int x2, int y2)
    {
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
    }
}
