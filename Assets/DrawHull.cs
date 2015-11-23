using UnityEngine;
using System.Collections.Generic;

public class DrawHull : MonoBehaviour {
    LineRenderer line;
    public List<Vector3> scan;
    PointsManager manager;
    // Use this for initialization
    void Start () {
        manager = PointsManager.getInstance();
    }
	
	// Update is called once per frame
	void Update () {
        if(manager.changed)
        {
            if(manager.points.Count >= 3)
                Draw();
            manager.changed = false;
        }
	}

    private void Draw()
    {
        scan = new GrahamScan(manager.points).calculateConvexHull();

        line = GetComponent<LineRenderer>();
        line.SetVertexCount(scan.Count);

        for (int i = 0; i < scan.Count; i++)
        {
            line.SetPosition(i, scan[i]);
        }
    }
}
