using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;

public class ClutchingAreaObserver : NetworkBehaviour {

    [Header("Touch Border")]
    public Image BorderImage;
    public Sprite OffBorderImage;
    public Sprite OnBorderImage;
    bool isPanning = false;

    private GameObject Finger;
    private GameObject Plane;
    private GameObject Parent;
    private Transform smartwatchParent;

    private Vector3 localProjectedPoint;
    private Vector3 projectedPoint;
    private Vector3 localPlanePositionOnEnter;
    private Vector3 offset = Vector3.zero;
    

    [ClientRpc]
    void RpcChangeBorderColor(bool onValue)
    {
        Debug.Log("Changing border color:" + onValue);
        if (onValue)
        {
            BorderImage.sprite = OnBorderImage;
        }
        else
        {
            BorderImage.sprite = OffBorderImage;
        }
    }

    [ClientRpc]
    void RpcChangePlaneParent(int parent) {
        if (parent == 0)
        {
            Plane.transform.SetParent(Parent.transform);
            
        }
        else
        {
            Parent.transform.DetachChildren();
            Plane.transform.SetParent(smartwatchParent);
        }
            
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter " + other.name);
        if (!isServer)
            return;

        if (other.name == "FingerPointNetwork") {
            RpcChangeBorderColor(true);
            isPanning = true;

            localPlanePositionOnEnter = Plane.transform.localPosition;
            // generate projected point
            projectedPoint = GenerateProjectePoint();
            localProjectedPoint = Plane.transform.InverseTransformPoint(projectedPoint);
            // calculate offset
            offset = localProjectedPoint - localPlanePositionOnEnter;

            // create empty parent object
            Parent.transform.position = projectedPoint;

            // attach plane to parent
            Plane.transform.SetParent(Parent.transform);
            RpcChangePlaneParent(0);
        }
    }

    void OnTriggerStay(Collider other)
    {
        Debug.Log("OnTriggerStay " + other.name);
        if (!isServer)
            return;

        if (other.name == "FingerPointNetwork")
        {
            RpcChangeBorderColor(true);
            isPanning = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("OnTriggerStay " + other.name);
        if (!isServer)
            return;

        if (other.name == "FingerPointNetwork")
        {
            RpcChangeBorderColor(false);
            isPanning = false;
            // dettach plane to parent
            Parent.transform.DetachChildren();
            Plane.transform.SetParent(smartwatchParent);
            RpcChangePlaneParent(1);
        }
    }
    public override void OnStartServer()
    {
        // disable client stuff
        Debug.Log("CluthcingAreaObserver: OnStartServer");
        Finger = GameObject.Find("FingerPointNetwork");
        Parent = GameObject.Find("Parent");
        Plane = GameObject.Find("Plane");
        smartwatchParent = Plane.transform.parent;
    }

    public override void OnStartClient()
    {
        // register client events, enable effects
        Debug.Log("CluthcingAreaObserver: OnStartClient");
        Finger = GameObject.Find("FingerPointNetwork");
        Parent = GameObject.Find("Parent");
        Plane = GameObject.Find("Plane");
        smartwatchParent = Plane.transform.parent;
    }

    [ServerCallback]
    void Update() {
        if (!isServer)
            return;

        try
        {
            if (isPanning)
            {
                Vector3 projectedPoint = GenerateProjectePoint();
                Parent.transform.position = projectedPoint;
            }
        }
        catch (Exception e) {
            Debug.LogError("Error in CluthcingAreaObserver.Update: " + e.ToString());  
        }
    }

  
    private Vector3 GenerateProjectePoint()
    {
        // Project the finger onto the plane
        var normal = Plane.transform.TransformDirection(Plane.GetComponent<MeshFilter>().mesh.normals[0]);
        Vector3 v = Finger.transform.position - Plane.transform.position;
        Vector3 d = Vector3.Project(v, normal);
        return Finger.transform.position - d;
    }

    void OnDrawGizmos()
    {
        try
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(Plane.transform.TransformPoint(localProjectedPoint), 0.01f);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
        
    }

}
