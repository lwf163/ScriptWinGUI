namespace Swg.OCR.QuickTable;

/// <summary>由网格线坐标生成单元格矩形。</summary>
public sealed class QuickTableCellGenerator
{
    /// <summary>根据行列线坐标生成单元格列表。</summary>
    public List<QuickTableCell> GenerateCells(List<int> rowCoords, List<int> colCoords)
    {
        var cells = new List<QuickTableCell>();

        if (rowCoords.Count < 2 || colCoords.Count < 2)
            return cells;

        for (int r = 0; r < rowCoords.Count - 1; r++)
        {
            int y1 = rowCoords[r];
            int y2 = rowCoords[r + 1];

            for (int c = 0; c < colCoords.Count - 1; c++)
            {
                int x1 = colCoords[c];
                int x2 = colCoords[c + 1];

                cells.Add(new QuickTableCell(r, c, x1, y1, x2, y2));
            }
        }

        return cells;
    }
}
