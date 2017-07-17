using UnityEngine;
using System.Collections;

public class PositionConstrainer : MonoBehaviour {

    private float planeScaleX;
    private float planeScaleZ;

    // Use this for initialization
    void Start () {
        planeScaleX = transform.localScale.x * 10;
        planeScaleZ = transform.localScale.z * 10;
    }

    // Update is called once per frame
    void Update () {
        Vector3 pos = transform.position;
        
        if (transform.position.x > planeScaleX / 2) {
            pos.x = planeScaleX / 2;
        }
        if (transform.position.z > planeScaleZ / 2)
        {
            pos.z = planeScaleZ / 2 ;
        }

        if (transform.position.x < -planeScaleX / 2)
        {
            pos.x = -planeScaleX / 2;
        }
        if (transform.position.z < -planeScaleZ / 2)
        {
            pos.z = -planeScaleZ / 2;
        }

        transform.position = pos;
    }
}
