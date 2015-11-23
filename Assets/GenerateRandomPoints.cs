using UnityEngine;
using System.Collections;

public class GenerateRandomPoints : MonoBehaviour {
    public int randomPointsCount = 10;

    PointsManager manager;

	// Use this for initialization
	void Start () {
        manager = PointsManager.getInstance();
	}

    public void clearPoints()
    {
        manager.points.Clear();
        GameObject[] points = GameObject.FindGameObjectsWithTag("Point");
        foreach (GameObject point in points)
        {
            Destroy(point);
        }
    }
	
    public void getPoints()
    {
        clearPoints();
        for (int i = 0; i < randomPointsCount; i++)
        {
            float x = Random.value;
            float z = Random.value;
            manager.Add(x, z);
        }
    }
}
