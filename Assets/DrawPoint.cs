using UnityEngine;
using System.Collections;

public class DrawPoint : MonoBehaviour {
    PointsManager manager;

    void Start()
    {
        manager = PointsManager.getInstance();
    }

	// Update is called once per frame
	void Update () {
	    if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if(Physics.Raycast(ray, out hitInfo))
            {
                manager.Add(hitInfo.point.x, hitInfo.point.z);
            }
        }
	}
}
