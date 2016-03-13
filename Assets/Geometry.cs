using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Geometry : MonoBehaviour
{
    // For debugging.
    public static Vector3[] g_MinMaxCorners;
    //public static RectangleF g_MinMaxBox;
    public static Vector3[] g_NonCulledPoints;

    // Find the points nearest the upper left, upper right,
    // lower left, and lower right corners.
    private static void GetMinMaxCorners(List<Vector3> points, ref Vector3 ul, ref Vector3 ur, ref Vector3 ll, ref Vector3 lr)
    {
        // Start with the first point as the solution.
        ul = points[0];
        ur = ul;
        ll = ul;
        lr = ul;

        // Search the other points.
        foreach (Vector3 pt in points)
        {
            if (-pt.x - pt.z > -ul.x - ul.z) ul = pt;
            if (pt.x - pt.z > ur.x - ur.z) ur = pt;
            if (-pt.x + pt.z > -ll.x + ll.z) ll = pt;
            if (pt.x + pt.z > lr.x + lr.z) lr = pt;
        }

        g_MinMaxCorners = new Vector3[] { ul, ur, lr, ll }; // For debugging.
    }

    // Find a box that fits inside the MinMax quadrilateral.
    /*private static RectangleF GetMinMaxBox(List<Vector3> points)
    {
        // Find the MinMax quadrilateral.
        Vector3 ul = new Vector3(0, 0), ur = ul, ll = ul, lr = ul;
        GetMinMaxCorners(points, ref ul, ref ur, ref ll, ref lr);

        // Get the coordinates of a box that lies inside this quadrilateral.
        float xmin, xmax, ymin, ymax;
        xmin = ul.x;
        ymin = ul.z;

        xmax = ur.x;
        if (ymin < ur.z) ymin = ur.z;

        if (xmax > lr.x) xmax = lr.x;
        ymax = lr.z;

        if (xmin < ll.x) xmin = ll.x;
        if (ymax > ll.z) ymax = ll.z;

        RectangleF result = new RectangleF(xmin, ymin, xmax - xmin, ymax - ymin);
        g_MinMaxBox = result;    // For debugging.
        return result;
    }*/

    // Cull points out of the convex hull that lie inside the
    // trapezoid defined by the vertices with smallest and
    // largest X and Y coordinates.
    // Return the points that are not culled.
    /*private static List<Vector3> HullCull(List<Vector3> points)
    {
        // Find a culling box.
        //RectangleF culling_box = GetMinMaxBox(points);

        // Cull the points.
        List<Vector3> results = new List<Vector3>();
        foreach (Vector3 pt in points)
        {
            // See if (this point lies outside of the culling box.
            if (pt.x <= culling_box.Left ||
                pt.x >= culling_box.Right ||
                pt.z <= culling_box.Top ||
                pt.z >= culling_box.Bottom)
            {
                // This point cannot be culled.
                // Add it to the results.
                results.Add(pt);
            }
        }

        g_NonCulledPoints = new Vector3[results.Count];   // For debugging.
        results.CopyTo(g_NonCulledPoints);              // For debugging.
        return results;
    }*/

    // Return the points that make up a polygon's convex hull.
    // This method leaves the points list unchanged.
    public static List<Vector3> MakeConvexHull(List<Vector3> points)
    {
        // Cull.
        //points = HullCull(points);

        // Find the remaining point with the smallest Y value.
        // if (there's a tie, take the one with the smaller X value.
        Vector3 best_pt = points[0];
        foreach (Vector3 pt in points)
        {
            if ((pt.z < best_pt.z) ||
                ((pt.z == best_pt.z) && (pt.x < best_pt.x)))
            {
                best_pt = pt;
            }
        }

        // Move this point to the convex hull.
        List<Vector3> hull = new List<Vector3>();
        hull.Add(best_pt);
        points.Remove(best_pt);

        // Start wrapping up the other points.
        float sweep_angle = 0;
        for (;;)
        {
            // If all of the points are on the hull, we're done.
            if (points.Count == 0) break;

            // Find the point with smallest AngleValue
            // from the last point.
            float X = hull[hull.Count - 1].x;
            float Y = hull[hull.Count - 1].z;
            best_pt = points[0];
            float best_angle = 3600;

            // Search the rest of the points.
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

            // See if the first point is better.
            // If so, we are done.
            float first_angle = AngleValue(X, Y, hull[0].x, hull[0].z);
            if ((first_angle >= sweep_angle) &&
                (best_angle >= first_angle))
            {
                // The first point is better. We're done.
                break;
            }

            // Add the best point to the convex hull.
            hull.Add(best_pt);
            points.Remove(best_pt);

            sweep_angle = best_angle;
        }

        return hull;
    }

    // Return a number that gives the ordering of angles
    // WRST horizontal from the point (x1, y1) to (x2, y2).
    // In other words, AngleValue(x1, y1, x2, y2) is not
    // the angle, but if:
    //   Angle(x1, y1, x2, y2) > Angle(x1, y1, x2, y2)
    // then
    //   AngleValue(x1, y1, x2, y2) > AngleValue(x1, y1, x2, y2)
    // this angle is greater than the angle for another set
    // of points,) this number for
    //
    // This function is dy / (dy + dx).
    private static float AngleValue(float x1, float y1, float x2, float y2)
    {
        float dx, dy, ax, ay, t;

        dx = x2 - x1;
        ax = Math.Abs(dx);
        dy = y2 - y1;
        ay = Math.Abs(dy);
        if (ax + ay == 0)
        {
            // if (the two points are the same, return 360.
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

    // Find a minimal bounding circle.
    public static void FindMinimalBoundingCircle(List<Vector3> points, out Vector3 center, out float radius)
    {
        // Find the convex hull.
        List<Vector3> hull = MakeConvexHull(points);

        // The best solution so far.
        Vector3 best_center = points[0];
        float best_radius2 = float.MaxValue;

        // Look at pairs of hull points.
        for (int i = 0; i < hull.Count - 1; i++)
        {
            for (int j = i + 1; j < hull.Count; j++)
            {
                // Find the circle through these two points.
                Vector3 test_center = new Vector3(
                    (hull[i].x + hull[j].x) / 2f,
                    0,
                    (hull[i].z + hull[j].z) / 2f);
                float dx = test_center.x - hull[i].x;
                float dy = test_center.z - hull[i].z;
                float test_radius2 = dx * dx + dy * dy;

                // See if this circle would be an improvement.
                if (test_radius2 < best_radius2)
                {
                    // See if this circle encloses all of the points.
                    if (CircleEnclosesPoints(test_center, test_radius2, hull, i, j, -1))
                    {
                        // Save this solution.
                        best_center = test_center;
                        best_radius2 = test_radius2;
                    }
                }
            } // for i
        } // for j

        // Look at triples of hull points.
        for (int i = 0; i < hull.Count - 2; i++)
        {
            for (int j = i + 1; j < hull.Count - 1; j++)
            {
                for (int k = j + 1; k < hull.Count; k++)
                {
                    // Find the circle through these three points.
                    Vector3 test_center;
                    float test_radius2;
                    FindCircle(hull[i], hull[j], hull[k], out test_center, out test_radius2);

                    // See if this circle would be an improvement.
                    if (test_radius2 < best_radius2)
                    {
                        // See if this circle encloses all of the points.
                        if (CircleEnclosesPoints(test_center, test_radius2, hull, i, j, k))
                        {
                            // Save this solution.
                            best_center = test_center;
                            best_radius2 = test_radius2;
                        }
                    }
                } // for k
            } // for i
        } // for j

        center = best_center;
        if (best_radius2 == float.MaxValue)
            radius = 0;
        else
            radius = (float)Math.Sqrt(best_radius2);
    }

    // Return true if the indicated circle encloses all of the points.
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

    // Find a circle through the three points.
    private static void FindCircle(Vector3 a, Vector3 b, Vector3 c, out Vector3 center, out float radius2)
    {
        // Get the perpendicular bisector of (x1, y1) and (x2, y2).
        float x1 = (b.x + a.x) / 2;
        float y1 = (b.z + a.z) / 2;
        float dy1 = b.x - a.x;
        float dx1 = -(b.z - a.z);

        // Get the perpendicular bisector of (x2, y2) and (x3, y3).
        float x2 = (c.x + b.x) / 2;
        float y2 = (c.z + b.z) / 2;
        float dy2 = c.x - b.x;
        float dx2 = -(c.z - b.z);

        // See where the lines intersect.
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

    // Extension method to draw a RectangleF.
    /*public static void DrawRectangle(this Graphics graphics, Pen pen, RectangleF rect)
    {
        graphics.DrawRectangle(pen, Rectangle.Round(rect));
    }*/

    // Find the point of intersection between
    // the lines p1 --> p2 and p3 --> p4.
    private static void FindIntersection(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4,
        out bool lines_intersect, out bool segments_intersect,
        out Vector3 intersection, out Vector3 close_p1, out Vector3 close_p2)
    {
        // Get the segments' parameters.
        float dx12 = p2.x - p1.x;
        float dy12 = p2.z - p1.z;
        float dx34 = p4.x - p3.x;
        float dy34 = p4.z - p3.z;

        // Solve for t1 and t2
        float denominator = (dy12 * dx34 - dx12 * dy34);

        float t1;
        try
        {
            t1 = ((p1.x - p3.x) * dy34 + (p3.z - p1.z) * dx34) / denominator;
        }
        catch
        {
            // The lines are parallel (or close enough to it).
            lines_intersect = false;
            segments_intersect = false;
            intersection = new Vector3(float.NaN, 0, float.NaN);
            close_p1 = new Vector3(float.NaN, 0, float.NaN);
            close_p2 = new Vector3(float.NaN, 0, float.NaN);
            return;
        }
        lines_intersect = true;

        float t2 = ((p3.x - p1.x) * dy12 + (p1.z - p3.z) * dx12) / -denominator;

        // Find the point of intersection.
        intersection = new Vector3(p1.x + dx12 * t1, 0, p1.z + dy12 * t1);

        // The segments intersect if t1 and t2 are between 0 and 1.
        segments_intersect = ((t1 >= 0) && (t1 <= 1) && (t2 >= 0) && (t2 <= 1));

        // Find the closest points on the segments.
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
