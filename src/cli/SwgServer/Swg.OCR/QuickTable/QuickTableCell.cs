namespace Swg.OCR.QuickTable;

/// <summary>快速表格（Tesseract）流水线中的单元格（行列与像素框）。</summary>
public sealed class QuickTableCell
{
    /// <summary>行索引（从 0 起）。</summary>
    public int Row { get; set; }

    /// <summary>列索引（从 0 起）。</summary>
    public int Col { get; set; }

    /// <summary>左边界 X。</summary>
    public int X1 { get; set; }

    /// <summary>上边界 Y。</summary>
    public int Y1 { get; set; }

    /// <summary>右边界 X。</summary>
    public int X2 { get; set; }

    /// <summary>下边界 Y。</summary>
    public int Y2 { get; set; }

    /// <summary>单元格高度。</summary>
    public int Height => Y2 - Y1;

    /// <summary>Tesseract 识别文本。</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>构造单元格。</summary>
    public QuickTableCell(int row, int col, int x1, int y1, int x2, int y2)
    {
        Row = row;
        Col = col;
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
    }
}

/// <summary>快速表格检测与 OCR 的完整结果。</summary>
public sealed class QuickTableDetectionResult
{
    /// <summary>行分隔线 Y 坐标序列。</summary>
    public List<int> Rows { get; set; } = [];

    /// <summary>列分隔线 X 坐标序列。</summary>
    public List<int> Cols { get; set; } = [];

    /// <summary>带文本的单元格列表。</summary>
    public List<QuickTableCell> Cells { get; set; } = [];
}
