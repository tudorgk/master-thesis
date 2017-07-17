using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

public class UDPTransportServer : MonoBehaviour
{

    int dataStreamChannelId;
    int commandChannelId;

    int maxConnections = 1;

    int dataStreamSocketId;
    int commandSocketId;

    int dataStreamSocketPort = 12345;
    int commandSocketPort = 11111;

    int dataStreamConnectedClientId = -1;
    int commandConnectedClient = -1;

    public Transform serverFinger;
    public Transform serverSmartwatch;
    public Transform serverPlane;
    public Transform serverTarget;

    BinaryFormatter formatter = new BinaryFormatter();

    // Use this for initialization
    void Start()
    {
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
        StartCoroutine(SendFingerPositionOverDataStreamSocket());
    }
    
    IEnumerator SendFingerPositionOverDataStreamSocket()
    {
       
        byte error;

        int bufferSize = 24*4; //13 floats 
        byte[] buffer = new byte[bufferSize]; // 4 bytes per float

        float[] floatArray = new float[] {
            0, 0, 0,     //finger position
            0, 0, 0,     //smartwatch position
            0, 0, 0, 0,  //smartwatch rotation
            0, 0, 0,     //plane position
            0, 0, 0, 0,   //plane rotation
            0, 0, 0,     //target position
            0, 0, 0, 0   //target rotation
        };

        Int64 timestamp = 0;
        bool isInCluthcingArea = false;

        for (;;)
        {
            if (dataStreamConnectedClientId != -1)
            {

                Vector3 fingerPosition = serverFinger.position;
                floatArray[0] = fingerPosition.x;
                floatArray[1] = fingerPosition.y;
                floatArray[2] = fingerPosition.z;

                Vector3 smartwatchPosition = serverSmartwatch.position;
                floatArray[3] = smartwatchPosition.x;
                floatArray[4] = smartwatchPosition.y;
                floatArray[5] = smartwatchPosition.z;

                Quaternion smartwatchRotation = serverSmartwatch.rotation;
                floatArray[6] = smartwatchRotation.x;
                floatArray[7] = smartwatchRotation.y;
                floatArray[8] = smartwatchRotation.z;
                floatArray[9] = smartwatchRotation.w;

                Vector3 planePosition = serverPlane.position;
                floatArray[10] = planePosition.x;
                floatArray[11] = planePosition.y;
                floatArray[12] = planePosition.z;

                Quaternion planeRotation = serverPlane.rotation;
                floatArray[13] = planeRotation.x;
                floatArray[14] = planeRotation.y;
                floatArray[15] = planeRotation.z;
                floatArray[16] = planeRotation.w;

                Vector3 targetPosition = serverTarget.position;
                floatArray[17] = targetPosition.x;
                floatArray[18] = targetPosition.y;
                floatArray[19] = targetPosition.z;

                Quaternion targetRotation = serverPlane.rotation;
                floatArray[20] = targetRotation.x;
                floatArray[21] = targetRotation.y;
                floatArray[22] = targetRotation.z;
                floatArray[23] = targetRotation.w;

                // create a byte array and copy the floats into it...
                Buffer.BlockCopy(floatArray, 0, buffer, 0, bufferSize); 

                NetworkTransport.Send(dataStreamSocketId, dataStreamConnectedClientId, dataStreamChannelId, buffer, bufferSize, out error);
                
            }
            yield return new WaitForSeconds(0.05f);
        }  
    }

    IEnumerator ListenOnDataStreamSocket()
    {
        for (;;)
        {
     
            int outConnectionId;
            int outChannelId;

            int receivedSize;
            byte error;
            byte[] buffer = new byte[128];
            
            NetworkEventType evt = NetworkTransport.ReceiveFromHost(dataStreamSocketId, out outConnectionId, out outChannelId, buffer, buffer.Length, out receivedSize, out error);
            switch (evt)
            {
                case NetworkEventType.ConnectEvent:
                    {
                        this.dataStreamConnectedClientId = outConnectionId;
                     
                        break;
                    }
                case NetworkEventType.DisconnectEvent:
                    {
                        this.dataStreamConnectedClientId = -1;
                       
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

    private IEnumerator ListenOnCommandSocket()
    {
        for (;;)
        {
            int outConnectionId;
            int outChannelId;

            int receivedSize;
            byte error;
            byte[] buffer = new byte[128];

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

    public void SendCommand(string command) {
        if (commandConnectedClient == -1) {
            return;
        }

        byte error;
        int bufferSize = 1024;
        byte[] buffer = new byte[bufferSize]; 
        Stream stream = new MemoryStream(buffer);

        formatter.Serialize(stream, command);

        NetworkTransport.Send(commandSocketId, commandConnectedClient, commandChannelId, buffer, bufferSize, out error);
    }

    void OnApplicationQuit()
    {
        Debug.Log("Application ending after " + Time.time + " seconds");
        NetworkTransport.Shutdown();
    }

    

}