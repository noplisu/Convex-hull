using UnityEngine;
using System.Collections.Generic;

public class DrawRadar : MonoBehaviour
{
    public float ThetaScale = 0.01f;
    private int Size;
    private LineRenderer LineDrawer;
    private float Theta = 0f;
    private PointsManager manager;

    void Start()
    {
        LineDrawer = GetComponent<LineRenderer>();
        manager = PointsManager.getInstance();
    }

    void Update()
    {
        if (manager.changed)
        {
            if (manager.points.Count >= 6)
                Draw();
            manager.changed = false;
        }
    }

    void Draw()
    {
        Vector3 center;
        float radius;
        List<Vector3> pointList = new List<Vector3>(manager.points);
        Geometry.FindMinimalBoundingCircle(pointList, out center, out radius);
        transform.position = center;
        Theta = 0f;
        Size = (int)((1f / ThetaScale) + 1f);
        LineDrawer.SetVertexCount(Size);
        for (int i = 0; i < Size; i++)
        {
            Theta += (2.0f * Mathf.PI * ThetaScale);
            float x = radius * Mathf.Cos(Theta);
            float z = radius * Mathf.Sin(Theta);
            LineDrawer.SetPosition(i, new Vector3(x, 0, z));
        }
    }
}