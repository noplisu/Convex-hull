using UnityEngine;

public class RenderPoints : MonoBehaviour {
    public GameObject PointPrefab;

    PointsManager pointsManager;

    void Start()
    {
        pointsManager = PointsManager.getInstance();
    }

    void Update()
    {
        if (!pointsManager.drawn)
        {
            foreach(Vector3 point in pointsManager.points)
            {
                Instantiate(PointPrefab, point, PointPrefab.transform.rotation);
            }
            pointsManager.drawn = true;
        }
    }
}
