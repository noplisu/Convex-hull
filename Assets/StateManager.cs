using UnityEngine;
using System.Collections;

public class StateManager : MonoBehaviour {
    bool menu = true;
    public GameObject on;
    public GameObject off;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            menu = !menu;
            if (menu)
            {
                on.SetActive(true);
                off.SetActive(false);
            }
            else
            {
                off.SetActive(true);
                on.SetActive(false);
            }
        }
    }
}
