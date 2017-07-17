using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DistanceWatcher : MonoBehaviour {

    public Transform point1;
    public Transform point2;
    private Text distanceLabel;

	// Use this for initialization
	void Start () {
        distanceLabel = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        float distance = Vector3.Distance(point1.position, point2.position) * 100.0f;
        distanceLabel.text = distance.ToString("n2") + " cm";
	}
}
