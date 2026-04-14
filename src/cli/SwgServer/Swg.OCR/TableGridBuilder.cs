using OpenCvSharp;

namespace Swg.OCR;

/// <summary>
/// 将表格检测得到的单元框按几何位置归为规则网格行列（从 0 起）。
/// </summary>
internal static class TableGridBuilder
{
    /// <summary>
    /// 按中心点 Y 聚类为行，行内按 X 排序得到列。
    /// </summary>
    public static IReadOnlyList<(int Row, int Col, Rect R, string T)> AssignRowCol(
        IReadOnlyList<(Rect Bbox, string Text)> raw)
    {
        if (raw.Count == 0)
            return Array.Empty<(int, int, Rect, string)>();

        var items = raw.Select(x => (
            Bbox: x.Bbox,
            Text: x.Text,
            Cy: x.Bbox.Y + x.Bbox.Height * 0.5,
            Cx: x.Bbox.X + x.Bbox.Width * 0.5)).ToList();

        int medH = items.Select(x => x.Bbox.Height).OrderBy(h => h).ElementAt(items.Count / 2);
        double tol = Math.Max(12, medH * 0.4);

        var rows = new List<List<(Rect R, string T, double Cy, double Cx)>>();
        foreach (var it in items.OrderBy(x => x.Cy).ThenBy(x => x.Cx))
        {
            List<(Rect R, string T, double Cy, double Cx)>? row = null;
            foreach (var rlist in rows)
            {
                double avgCy = rlist.Average(x => x.Cy);
                if (Math.Abs(it.Cy - avgCy) <= tol)
                {
                    row = rlist;
                    break;
                }
            }

            if (row is null)
                rows.Add([(it.Bbox, it.Text, it.Cy, it.Cx)]);
            else
                row.Add((it.Bbox, it.Text, it.Cy, it.Cx));
        }

        rows.Sort((a, b) => a[0].Cy.CompareTo(b[0].Cy));
        var result = new List<(int Row, int Col, Rect R, string T)>();
        for (int ri = 0; ri < rows.Count; ri++)
        {
            List<(Rect R, string T, double Cy, double Cx)> ordered = rows[ri].OrderBy(x => x.Cx).ToList();
            for (int ci = 0; ci < ordered.Count; ci++)
            {
                (Rect R, string T, _, _) = ordered[ci];
                result.Add((ri, ci, R, T));
            }
        }

        return result;
    }
}
