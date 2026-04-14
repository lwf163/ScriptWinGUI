namespace Swg.OCR.QuickTable;

/// <summary>合并共线短线段为长表格线。</summary>
public sealed class QuickTableLineMerger
{
    /// <summary>按方向合并线段；<paramref name="orientation"/> 为 h 或 v。</summary>
    public List<QuickTableLine> MergeLines(List<QuickTableLine> lines, string orientation = "h", int distThresh = 10)
    {
        if (lines is null || lines.Count == 0)
            return [];

        var simplified = new List<(int pos, int start, int end)>();

        if (orientation == "h")
        {
            foreach (QuickTableLine line in lines)
            {
                int y = (int)Math.Round((line.Y1 + line.Y2) / 2.0);
                simplified.Add((y, Math.Min(line.X1, line.X2), Math.Max(line.X1, line.X2)));
            }

            simplified.Sort((a, b) => a.pos.CompareTo(b.pos));

            var merged = new List<QuickTableLine>();
            var (curY, curX1, curX2) = simplified[0];

            for (int i = 1; i < simplified.Count; i++)
            {
                var (y, x1, x2) = simplified[i];
                if (Math.Abs(y - curY) <= distThresh)
                {
                    curX1 = Math.Min(curX1, x1);
                    curX2 = Math.Max(curX2, x2);
                    curY = (int)Math.Round((curY + y) / 2.0);
                }
                else
                {
                    merged.Add(new QuickTableLine(curX1, curY, curX2, curY));
                    (curY, curX1, curX2) = (y, x1, x2);
                }
            }

            merged.Add(new QuickTableLine(curX1, curY, curX2, curY));
            return merged;
        }

        foreach (QuickTableLine line in lines)
        {
            int x = (int)Math.Round((line.X1 + line.X2) / 2.0);
            simplified.Add((x, Math.Min(line.Y1, line.Y2), Math.Max(line.Y1, line.Y2)));
        }

        simplified.Sort((a, b) => a.pos.CompareTo(b.pos));

        var mergedV = new List<QuickTableLine>();
        var (curX, curY1, curY2) = simplified[0];

        for (int i = 1; i < simplified.Count; i++)
        {
            var (x, y1, y2) = simplified[i];
            if (Math.Abs(x - curX) <= distThresh)
            {
                curY1 = Math.Min(curY1, y1);
                curY2 = Math.Max(curY2, y2);
                curX = (int)Math.Round((curX + x) / 2.0);
            }
            else
            {
                mergedV.Add(new QuickTableLine(curX, curY1, curX, curY2));
                (curX, curY1, curY2) = (x, y1, y2);
            }
        }

        mergedV.Add(new QuickTableLine(curX, curY1, curX, curY2));
        return mergedV;
    }
}
