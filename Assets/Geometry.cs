using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Geometry : MonoBehaviour
{
    public static Vector3[] g_MinMaxCorners;
    public static Vector3[] g_NonCulledPoints;

    private static void GetMinMaxCorners(List<Vector3> points, ref Vector3 ul, ref Vector3 ur, ref Vector3 ll, ref Vector3 lr)
    {
        ul = points[0];
        ur = ul;
        ll = ul;
        lr = ul;

        foreach (Vector3 pt in points)
        {
            if (-pt.x - pt.z > -ul.x - ul.z) ul = pt;
            if (pt.x - pt.z > ur.x - ur.z) ur = pt;
            if (-pt.x + pt.z > -ll.x + ll.z) ll = pt;
            if (pt.x + pt.z > lr.x + lr.z) lr = pt;
        }

        g_MinMaxCorners = new Vector3[] { ul, ur, lr, ll }; // For debugging.
    }

    public static List<Vector3> MakeConvexHull(List<Vector3> points)
    {
        Vector3 best_pt = points[0];
        foreach (Vector3 pt in points)
        {
            if ((pt.z < best_pt.z) ||
                ((pt.z == best_pt.z) && (pt.x < best_pt.x)))
            {
                best_pt = pt;
            }
        }

        List<Vector3> hull = new List<Vector3>();
        hull.Add(best_pt);
        points.Remove(best_pt);

        float sweep_angle = 0;
        for (;;)
        {
            if (points.Count == 0) break;

            float X = hull[hull.Count - 1].x;
            float Y = hull[hull.Count - 1].z;
            best_pt = points[0];
            float best_angle = 3600;

            foreach (Vector3 pt in points)
            {
                float test_angle = AngleValue(X, Y, pt.x, pt.z);
                if ((test_angle >= sweep_angle) &&
                    (best_angle > test_angle))
                {
                    best_angle = test_angle;
                    best_pt = pt;
                }
            }

            float first_angle = AngleValue(X, Y, hull[0].x, hull[0].z);
            if ((first_angle >= sweep_angle) &&
                (best_angle >= first_angle))
            {
                break;
            }

            hull.Add(best_pt);
            points.Remove(best_pt);

            sweep_angle = best_angle;
        }

        return hull;
    }

    private static float AngleValue(float x1, float y1, float x2, float y2)
    {
        float dx, dy, ax, ay, t;

        dx = x2 - x1;
        ax = Math.Abs(dx);
        dy = y2 - y1;
        ay = Math.Abs(dy);
        if (ax + ay == 0)
        {
            t = 360f / 9f;
        }
        else
        {
            t = dy / (ax + ay);
        }
        if (dx < 0)
        {
            t = 2 - t;
        }
        else if (dy < 0)
        {
            t = 4 + t;
        }
        return t * 90;
    }

    public static void FindMinimalBoundingCircle(List<Vector3> points, out Vector3 center, out float radius)
    {
        List<Vector3> hull = MakeConvexHull(points);

        Vector3 best_center = points[0];
        float best_radius2 = float.MaxValue;

        for (int i = 0; i < hull.Count - 1; i++)
        {
            for (int j = i + 1; j < hull.Count; j++)
            {
                Vector3 test_center = new Vector3(
                    (hull[i].x + hull[j].x) / 2f,
                    0,
                    (hull[i].z + hull[j].z) / 2f);
                float dx = test_center.x - hull[i].x;
                float dy = test_center.z - hull[i].z;
                float test_radius2 = dx * dx + dy * dy;

                if (test_radius2 < best_radius2)
                {
                    if (CircleEnclosesPoints(test_center, test_radius2, hull, i, j, -1))
                    {
                        best_center = test_center;
                        best_radius2 = test_radius2;
                    }
                }
            }
        }

        for (int i = 0; i < hull.Count - 2; i++)
        {
            for (int j = i + 1; j < hull.Count - 1; j++)
            {
                for (int k = j + 1; k < hull.Count; k++)
                {
                    Vector3 test_center;
                    float test_radius2;
                    FindCircle(hull[i], hull[j], hull[k], out test_center, out test_radius2);

                    if (test_radius2 < best_radius2)
                    {
                        if (CircleEnclosesPoints(test_center, test_radius2, hull, i, j, k))
                        {
                            best_center = test_center;
                            best_radius2 = test_radius2;
                        }
                    }
                }
            }
        }

        center = best_center;
        if (best_radius2 == float.MaxValue)
            radius = 0;
        else
            radius = (float)Math.Sqrt(best_radius2);
    }

    private static bool CircleEnclosesPoints(Vector3 center,
        float radius2, List<Vector3> points, int skip1, int skip2, int skip3)
    {
        for (int i = 0; i < points.Count; i++)
        {
            if ((i != skip1) && (i != skip2) && (i != skip3))
            {
                Vector3 point = points[i];
                float dx = center.x - point.x;
                float dy = center.z - point.z;
                float test_radius2 = dx * dx + dy * dy;
                if (test_radius2 > radius2) return false;
            }
        }
        return true;
    }

    private static void FindCircle(Vector3 a, Vector3 b, Vector3 c, out Vector3 center, out float radius2)
    {
        float x1 = (b.x + a.x) / 2;
        float y1 = (b.z + a.z) / 2;
        float dy1 = b.x - a.x;
        float dx1 = -(b.z - a.z);

        float x2 = (c.x + b.x) / 2;
        float y2 = (c.z + b.z) / 2;
        float dy2 = c.x - b.x;
        float dx2 = -(c.z - b.z);

        bool lines_intersect, segments_intersect;
        Vector3 intersection, close_p1, close_p2;
        FindIntersection(
            new Vector3(x1, 0, y1),
            new Vector3(x1 + dx1, 0, y1 + dy1),
            new Vector3(x2, 0, y2),
            new Vector3(x2 + dx2, 0, y2 + dy2),
            out lines_intersect,
            out segments_intersect,
            out intersection,
            out close_p1,
            out close_p2);

        center = intersection;
        float dx = center.x - a.x;
        float dy = center.z - a.z;
        radius2 = dx * dx + dy * dy;
    }

    private static void FindIntersection(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4,
        out bool lines_intersect, out bool segments_intersect,
        out Vector3 intersection, out Vector3 close_p1, out Vector3 close_p2)
    {
        float dx12 = p2.x - p1.x;
        float dy12 = p2.z - p1.z;
        float dx34 = p4.x - p3.x;
        float dy34 = p4.z - p3.z;

        float denominator = (dy12 * dx34 - dx12 * dy34);

        float t1;
        try
        {
            t1 = ((p1.x - p3.x) * dy34 + (p3.z - p1.z) * dx34) / denominator;
        }
        catch
        {
            lines_intersect = false;
            segments_intersect = false;
            intersection = new Vector3(float.NaN, 0, float.NaN);
            close_p1 = new Vector3(float.NaN, 0, float.NaN);
            close_p2 = new Vector3(float.NaN, 0, float.NaN);
            return;
        }
        lines_intersect = true;

        float t2 = ((p3.x - p1.x) * dy12 + (p1.z - p3.z) * dx12) / -denominator;

        intersection = new Vector3(p1.x + dx12 * t1, 0, p1.z + dy12 * t1);

        segments_intersect = ((t1 >= 0) && (t1 <= 1) && (t2 >= 0) && (t2 <= 1));

        if (t1 < 0)
        {
            t1 = 0;
        }
        else if (t1 > 1)
        {
            t1 = 1;
        }

        if (t2 < 0)
        {
            t2 = 0;
        }
        else if (t2 > 1)
        {
            t2 = 1;
        }

        close_p1 = new Vector3(p1.x + dx12 * t1, 0, p1.z + dy12 * t1);
        close_p2 = new Vector3(p3.x + dx34 * t2, 0, p3.z + dy34 * t2);
    }
}
