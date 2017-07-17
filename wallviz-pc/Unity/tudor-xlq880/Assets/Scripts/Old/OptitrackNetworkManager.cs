using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class OptitrackNetworkManager : NetworkManager {

	// Use this for initialization
	void Start () {
       
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    // called when a client connects 
    public override void OnServerConnect(NetworkConnection conn) {
        base.OnServerConnect(conn);
        Debug.Log("OnPlayerConnected");
    }

    // called when a new player is added for a client
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        base.OnServerAddPlayer(conn, playerControllerId);
        Debug.Log("OnServerAddPlayer " + playerControllerId.ToString());

       
        // Set debug label for player on server
        var optitrackStreamingClient = GameObject.Find("OptitrackStreamingClientNetwork");
        var remoteSmartwatch = GameObject.FindWithTag("Smartwatch");
        Debug.Log("Found remote smartwatch", remoteSmartwatch);
        optitrackStreamingClient.GetComponent<OptitrackRigidBodyRPC>().remoteSmartwatch = remoteSmartwatch;
       
    }

    /*
    // called when a client disconnects
    public virtual void OnServerDisconnect(NetworkConnection conn)
    {
        NetworkServer.DestroyPlayersForConnection(conn);
    }

    // called when a client is ready
    public virtual void OnServerReady(NetworkConnection conn)
    {
        NetworkServer.SetClientReady(conn);
    }
    // called when a player is removed for a client
    public virtual void OnServerRemovePlayer(NetworkConnection conn, short playerControllerId)
    {
        
    }

    // called when a network error occurs
    public virtual void OnServerError(NetworkConnection conn, int errorCode) {
    }
         */




}
