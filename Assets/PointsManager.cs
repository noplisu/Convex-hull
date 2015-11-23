using UnityEngine;
using System.Collections.Generic;

public class PointsManager : MonoBehaviour {
    public List<Vector3> points = new List<Vector3>();
    static PointsManager instance;
    public float height;
    public bool changed = false;
    public bool drawn = true;

    public static PointsManager getInstance()
    {
        return instance;
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        instance = this;
    }

    public void Add(float x, float z)
    {
        points.Add(new Vector3(x, height, z));
        changed = true;
        drawn = false;
    }
}
