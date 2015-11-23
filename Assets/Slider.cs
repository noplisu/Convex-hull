using UnityEngine;
using UnityEngine.UI;

public class Slider : MonoBehaviour {
    public GenerateRandomPoints manager;

    public void change(float value)
    {
        manager.randomPointsCount = Mathf.FloorToInt(value);    
    }
}
