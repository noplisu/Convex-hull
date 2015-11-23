using UnityEngine;
using System.Collections.Generic;

public class GrahamScan {
    public List<Vector3> points;

    public GrahamScan(List<Vector3> points)
    {
        setPoints(points);
    }

    public void setPoints(List<Vector3> points)
    {
        this.points = points;
    }

    public List<Vector3> calculateConvexHull()
    {
        points.Sort(
            delegate (Vector3 point1, Vector3 point2)
            {
                if(point1.x > point2.x) return 1;
                if(point1.x < point2.x) return -1;
                return 0;
            }
        );
        List<Vector3> lUpper = new List<Vector3>();
        lUpper.Add(points[0]);
        lUpper.Add(points[1]);
        for (int i = 2; i < points.Count; i++)
        {
            lUpper.Add(points[i]);
            int count = lUpper.Count;
            while (count > 2 && ccw(lUpper[count - 3], lUpper[count - 2], lUpper[count - 1]) >= 0)
            {
                lUpper.RemoveAt(count - 2);
                count -= 1;
            }
        }

        List<Vector3> lLower = new List<Vector3>();
        int n = points.Count;
        lLower.Add(points[n-1]);
        lLower.Add(points[n-2]);
        for (int i = n-3; i >= 0; i--)
        {
            lLower.Add(points[i]);
            int count = lLower.Count;
            while (count > 2 && ccw(lLower[count - 3], lLower[count - 2], lLower[count - 1]) >= 0)
            {
                lLower.RemoveAt(count - 2);
                count -= 1;
            }
        }

        lLower.RemoveAt(lLower.Count - 1);
        lLower.RemoveAt(0);

        List<Vector3> hull = new List<Vector3>();
        hull.AddRange(lLower);
        hull.AddRange(lUpper);
        if (lLower.Count > 0)
            hull.Add(lLower[0]);

        return hull;
    }

    private float ccw(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return (p2.x - p1.x) * (p3.z - p1.z) - (p2.z - p1.z) * (p3.x - p1.x);
    }
}
