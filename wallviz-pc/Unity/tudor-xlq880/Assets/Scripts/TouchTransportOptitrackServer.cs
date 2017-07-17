using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Networking;
using System.IO;
using System;
using UnityEngine.UI;

public class TouchTransportOptitrackServer : MonoBehaviour {

    int dataStreamChannelId;
    int commandChannelId;

    int maxConnections = 1;

    int dataStreamSocketId;
    int commandSocketId;

    int dataStreamSocketPort = 12345;
    int commandSocketPort = 11111;

    int dataStreamConnectedClientId = -1;
    int commandConnectedClient = -1;

    public Transform Plane;
    public Transform Target;

    BinaryFormatter formatter = new BinaryFormatter();

    // nr_of_clutches
    [HideInInspector]
    public int nr_of_clutches = 0;
    // isPanning
    [HideInInspector]
    public int is_panning = 0;

    public Text notificationText;
    TouchTrialManager trialManager;

    // Use this for initialization
    void Start () {
        NetworkTransport.Init();
        Debug.Log("Client: Networktransport initialized!");
        /*
         * Data Stream config
         */

        ConnectionConfig dataStreamConfig = new ConnectionConfig();
        //The same as unrelaible but all unorder messages will be dropped. Example: VoIP.
        dataStreamChannelId = dataStreamConfig.AddChannel(QosType.UnreliableSequenced);

        //set max number of connections
        HostTopology dataStreamTopology = new HostTopology(dataStreamConfig, maxConnections);
        //finally we open the socket
        dataStreamSocketId = NetworkTransport.AddHost(dataStreamTopology, dataStreamSocketPort);
        Debug.Log("Data Stream Socket Open. SocketId is: " + dataStreamSocketId);

        /*
         * Command config
         */
        ConnectionConfig commandConfig = new ConnectionConfig();
        commandChannelId = commandConfig.AddChannel(QosType.ReliableSequenced);
        HostTopology commandTopology = new HostTopology(commandConfig, maxConnections);
        commandSocketId = NetworkTransport.AddHost(commandTopology, commandSocketPort);

        Debug.Log("Command Socket Open. SocketId is: " + commandSocketId);

        StartCoroutine(ListenOnDataStreamSocket());
        StartCoroutine(ListenOnCommandSocket());
        
    }

    IEnumerator ListenOnDataStreamSocket()
    {
        for (;;)
        {

            int outConnectionId;
            int outChannelId;

            int receivedSize;
            byte error;
            byte[] buffer = new byte[256];
            // create a second float array and copy the bytes into it...
            var floatArray = new float[256];

            NetworkEventType evt = NetworkTransport.ReceiveFromHost(dataStreamSocketId, out outConnectionId, out outChannelId, buffer, buffer.Length, out receivedSize, out error);
            switch (evt)
            {
                case NetworkEventType.ConnectEvent:
                    {
                        this.dataStreamConnectedClientId = outConnectionId;
                        Debug.Log("data stream connected event:" + outConnectionId);
                        break;
                    }
                case NetworkEventType.DisconnectEvent:
                    {
                        this.dataStreamConnectedClientId = -1;
                        Debug.Log("data stream disconnected event:" + outConnectionId);
                        break;
                    }
                case NetworkEventType.DataEvent:
                    {
                        //TODO: handle number of clutches
                        Buffer.BlockCopy(buffer, 0, floatArray, 0, receivedSize);
                        Debug.Log(floatArray[5]);
                        Plane.localPosition = new Vector3(floatArray[0], floatArray[1], floatArray[2]);
                        Target.localPosition = new Vector3(floatArray[3], floatArray[4], floatArray[5]);
                        nr_of_clutches = Convert.ToInt32(floatArray[6]);
                        is_panning = Convert.ToInt32(floatArray[7]);
                        break;
                    }
                case NetworkEventType.BroadcastEvent:
                    {

                        break;
                    }
                case NetworkEventType.Nothing:
                    break;

                default:
                    Debug.LogError("Unknown network message type received: " + evt);
                    break;
            }
            yield return null;
        }
    }

    private IEnumerator ListenOnCommandSocket()
    {
        for (;;)
        {
            int outConnectionId;
            int outChannelId;

            int receivedSize;
            byte error;
            byte[] buffer = new byte[1024];

            NetworkEventType evt = NetworkTransport.ReceiveFromHost(commandSocketId, out outConnectionId, out outChannelId, buffer, buffer.Length, out receivedSize, out error);
            switch (evt)
            {
                case NetworkEventType.ConnectEvent:
                    {
                        this.commandConnectedClient = outConnectionId;
                        break;
                    }
                case NetworkEventType.DisconnectEvent:
                    {
                        this.commandConnectedClient = -1;
                        break;
                    }
                case NetworkEventType.DataEvent:
                    {
                       
                        break;
                    }
                case NetworkEventType.BroadcastEvent:
                    {

                        break;
                    }
                case NetworkEventType.Nothing:
                    break;

                default:
                    Debug.LogError("Unknown network message type received: " + evt);
                    break;
            }
            yield return null;
        }
    }

    public void SendCommand(string command)
    {
        if (commandConnectedClient == -1)
        {
            Debug.LogError("No connection established with command"); 
            return;
        }

        byte error;
        int bufferSize = 1024;
        byte[] buffer = new byte[bufferSize];
        Stream stream = new MemoryStream(buffer);

        formatter.Serialize(stream, command);

        Debug.Log("Sending command " + command);

        NetworkTransport.Send(commandSocketId, commandConnectedClient, commandChannelId, buffer, bufferSize, out error);
    }

    void OnApplicationQuit()
    {
        Debug.Log("Application ending after " + Time.time + " seconds");
        NetworkTransport.Shutdown();
    }
}
