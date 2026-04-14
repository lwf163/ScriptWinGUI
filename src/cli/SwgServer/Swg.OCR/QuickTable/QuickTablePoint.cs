namespace Swg.OCR.QuickTable;

/// <summary>表格线交点的整数坐标（避免与 <c>System.Drawing.Point</c> 混淆）。</summary>
public sealed class QuickTablePoint
{
    /// <summary>X。</summary>
    public int X { get; set; }

    /// <summary>Y。</summary>
    public int Y { get; set; }

    /// <summary>构造交点。</summary>
    public QuickTablePoint(int x, int y)
    {
        X = x;
        Y = y;
    }
}
