using UnityEngine;
using System.IO;

public class PointsCSV : MonoBehaviour {
    PointsManager manager;

    void Start()
    {
        manager = PointsManager.getInstance();
    }

    public void SaveCSV()
    {
        StreamWriter file = File.CreateText("coordinates.csv");
        foreach(Vector3 point in manager.points)
        {
            file.WriteLine(point.x + "," + point.z);
        }
        file.Close();
    }

    public void LoadCSV()
    {
        manager.points.Clear();
        GameObject[] points = GameObject.FindGameObjectsWithTag("Point");
        foreach(GameObject point in points)
        {
            Destroy(point);
        }
        StreamReader file = File.OpenText("coordinates.csv");
        while (file.Peek() > -1)
        {
            string line = file.ReadLine();
            string[] coordinates = line.Split(',');
            float x = float.Parse(coordinates[0]);
            float z = float.Parse(coordinates[1]);
            manager.Add(x, z);
        }
        file.Close();
    }
}
