namespace Swg.OCR.QuickTable;

/// <summary>交点坐标聚类为表格网格线位置。</summary>
public sealed class QuickTableCoordinateClusterer
{
    /// <summary>沿 y 或 x 轴聚类坐标。</summary>
    public List<int> ClusterCoordinates(List<QuickTablePoint> points, string axis = "y", int tolerance = 5)
    {
        if (points is null || points.Count == 0)
            return [];

        List<int> coordinates = axis == "y"
            ? points.Select(p => p.Y).OrderBy(c => c).ToList()
            : points.Select(p => p.X).OrderBy(c => c).ToList();

        var clusters = new List<int>();
        int currentCoord = coordinates[0];
        int count = 1;
        int sum = currentCoord;

        for (int i = 1; i < coordinates.Count; i++)
        {
            int coord = coordinates[i];

            if (Math.Abs(coord - currentCoord) <= tolerance)
            {
                sum += coord;
                count++;
                currentCoord = (int)Math.Round((double)sum / count);
            }
            else
            {
                clusters.Add(currentCoord);
                currentCoord = coord;
                sum = coord;
                count = 1;
            }
        }

        clusters.Add(currentCoord);
        return clusters;
    }
}
