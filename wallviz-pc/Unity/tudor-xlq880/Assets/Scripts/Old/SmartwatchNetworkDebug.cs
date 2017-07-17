using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class SmartwatchNetworkDebug : NetworkBehaviour {

    private Text debugTextLabel;

    [SyncVar]
    public string debugString = "test";

	// Use this for initialization
	void Start () {
        debugTextLabel = GameObject.Find("DebugInfo").GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        if (isServer)
        {
            debugString = transform.position.ToString() + "\n" + transform.localRotation.ToString();
        }
        else {
            debugTextLabel.text = debugString;
        }
	}
}
