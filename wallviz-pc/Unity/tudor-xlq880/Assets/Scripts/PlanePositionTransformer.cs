using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;




public class PlanePositionTransformer : MonoBehaviour {


    public bool TouchscreenTrial = false;
    [Header("Touch Border")]
    public Image BorderImage;
    public Sprite OffBorderImage;
    public Sprite OnBorderImage;
    [HideInInspector]
    public bool isPanning = false;

    public GameObject Finger;
    public GameObject Smartwatch;
    public UDPTransportServer transportServer;
    public TrialManager trialManager;

    Vector3 localFingerPosition;
    Vector3 offset;

    [HideInInspector]
    public int nr_clutches = 0;

    private IEnumerator panCoroutine = null;
    private bool first_enter = false;
  
    private IEnumerator Pan()
    {
        for (;;) {
            try
            {
                if (first_enter && isPanning && trialManager.trialStarted && TouchscreenTrial == false)
                {
                    localFingerPosition = Smartwatch.transform.InverseTransformPoint(Finger.transform.position);
                    gameObject.transform.localPosition = new Vector3(localFingerPosition.x + offset.x, gameObject.transform.localPosition.y, localFingerPosition.z + offset.z);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error in CluthcingAreaObserver.Update: " + e.ToString());
            }
            yield return null;
        }
        
    }

    void ChangeBorderColor(bool onValue)
    {
        Debug.Log("Changing border color:" + onValue);
        if (onValue)
        {
            BorderImage.sprite = OnBorderImage;
            transportServer.SendCommand("isTouching|true");
        }
        else
        {
            BorderImage.sprite = OffBorderImage;
            transportServer.SendCommand("isTouching|false");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter " + other.name);

        if (other.name == "FingerPointServer")
        {
            first_enter = true;
            ChangeBorderColor(true);

            if (trialManager.trialStarted && TouchscreenTrial == false)
            {
                localFingerPosition = Smartwatch.transform.InverseTransformPoint(Finger.transform.position);
                offset = gameObject.transform.localPosition - localFingerPosition;

                isPanning = true;
                nr_clutches++;
                StartPan();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("OnTriggerStay " + other.name);

        if (other.name == "FingerPointServer")
        {
            ChangeBorderColor(false);
            isPanning = false;
        }

    }

    public void PreparePlaneTransformer() {
        first_enter = false;
    }

    public void FinishPlaneTransformer() {
        StopPan();
    }

    public void StartPan() {
        if (panCoroutine == null) {
            panCoroutine = Pan();
            StartCoroutine(panCoroutine);
        }
    }

    public void StopPan() {
        if (panCoroutine != null) {
            StopCoroutine(panCoroutine);
            panCoroutine = null;
            isPanning = false;
        }
    }

    private Vector3 GenerateProjectePoint()
    {
        // Project the finger onto the plane
        var normal = gameObject.transform.TransformDirection(gameObject.GetComponent<MeshFilter>().mesh.normals[0]);
        Vector3 v = Finger.transform.position - gameObject.transform.position;
        Vector3 d = Vector3.Project(v, normal);
        return Finger.transform.position - d;
    }

  
}

