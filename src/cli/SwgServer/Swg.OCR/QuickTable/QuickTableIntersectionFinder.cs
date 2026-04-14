namespace Swg.OCR.QuickTable;

/// <summary>水平线与垂直线求交并去重。</summary>
public sealed class QuickTableIntersectionFinder
{
    /// <summary>求网格交点（容差内合并）。</summary>
    public List<QuickTablePoint> FindIntersections(List<QuickTableLine> hLines, List<QuickTableLine> vLines, int tolerance = 2)
    {
        var points = new List<QuickTablePoint>();

        foreach (QuickTableLine hLine in hLines)
        {
            foreach (QuickTableLine vLine in vLines)
            {
                int vx = vLine.X1;
                int hy = hLine.Y1;

                if (vx >= Math.Min(hLine.X1, hLine.X2) - tolerance &&
                    vx <= Math.Max(hLine.X1, hLine.X2) + tolerance &&
                    hy >= Math.Min(vLine.Y1, vLine.Y2) - tolerance &&
                    hy <= Math.Max(vLine.Y1, vLine.Y2) + tolerance)
                {
                    points.Add(new QuickTablePoint(vx, hy));
                }
            }
        }

        if (points.Count == 0)
            return [];

        var uniquePoints = new List<QuickTablePoint>();
        var used = new bool[points.Count];

        for (int i = 0; i < points.Count; i++)
        {
            if (used[i])
                continue;

            int xSum = points[i].X;
            int ySum = points[i].Y;
            int count = 1;
            used[i] = true;

            for (int j = i + 1; j < points.Count; j++)
            {
                if (!used[j] && Math.Abs(points[i].X - points[j].X) <= tolerance &&
                    Math.Abs(points[i].Y - points[j].Y) <= tolerance)
                {
                    xSum += points[j].X;
                    ySum += points[j].Y;
                    count++;
                    used[j] = true;
                }
            }

            int avgX = (int)Math.Round((double)xSum / count);
            int avgY = (int)Math.Round((double)ySum / count);
            uniquePoints.Add(new QuickTablePoint(avgX, avgY));
        }

        return uniquePoints;
    }
}
