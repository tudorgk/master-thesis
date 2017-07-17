using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using UnityEngine.UI;

public class UDPTransportClient : MonoBehaviour
{

    int dataStreamChannelId;
    int commandChannelId;

    int maxConnections = 1;

    int dataStreamSocketId;
    int commandSocketId;

    string serverIpAddress = "192.168.20.72";
    int dataStreamServerPort = 12345;
    int commandServerPort = 11111;

    int dataStreamConnectionId = -1;
    int commandConnectionId = -1;

    public Transform clientFinger;
    public Transform clientSmartwatch;
    public Transform clientPlane;
    public Transform clientTarget;


    public Button connectButton;
    public Text TrialStatusText;

    [Header("Touch Border")]
    public Image BorderImage;
    public Sprite OffBorderImage;
    public Sprite OnBorderImage;
    bool isPanning = false;

    void ChangeBorderColor(bool onValue)
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

    // Use this for initialization
    void Start()
    {
        // Init Transport using default values.
        NetworkTransport.Init();
        Debug.Log("Client: Networktransport initialized! ");

        /*
         * Data Stream config
         */

        ConnectionConfig dataStreamConfig = new ConnectionConfig();
        //The same as unrelaible but all unorder messages will be dropped. Example: VoIP.
        dataStreamChannelId = dataStreamConfig.AddChannel(QosType.UnreliableSequenced);

        //set max number of connections
        HostTopology dataStreamTopology = new HostTopology(dataStreamConfig, maxConnections);
        //finally we open the socket
        dataStreamSocketId = NetworkTransport.AddHost(dataStreamTopology);
        Debug.Log("Data Stream Socket Open. SocketId is: " + dataStreamSocketId);

        /*
         * Command config
         */
        ConnectionConfig commandConfig = new ConnectionConfig();
        commandChannelId = commandConfig.AddChannel(QosType.ReliableSequenced);
        HostTopology commandTopology = new HostTopology(commandConfig, maxConnections);
        commandSocketId = NetworkTransport.AddHost(commandTopology);

        Debug.Log("Command Socket Open. SocketId is: " + commandSocketId);

        StartCoroutine(ListenToDataStreamSocket());
        StartCoroutine(ListenToCommandSocket());
    }

    void OnApplicationQuit()
    {
        Debug.Log("Application ending after " + Time.time + " seconds");
        NetworkTransport.Shutdown();
    }

    public void Connect() {
        ConnectToDataStream();
        ConnectToCommand();
        if (dataStreamConnectionId != -1 && commandConnectionId != -1)
        {
            connectButton.gameObject.SetActive(false);
        }
    }

    public void ConnectToDataStream() 
    {
        byte error;
        if (dataStreamConnectionId == -1) {
            dataStreamConnectionId = NetworkTransport.Connect(dataStreamSocketId, serverIpAddress, dataStreamServerPort, 0, out error);
            Debug.Log("Connected to data stream. ConnectionId: " + dataStreamConnectionId);
        }
        
    }

    public void ConnectToCommand()
    {
        byte error;
        if (commandConnectionId == -1)
        {
            commandConnectionId = NetworkTransport.Connect(commandSocketId, serverIpAddress, commandServerPort, 0, out error);
            Debug.Log("Connected to command. ConnectionId: " + commandConnectionId);
        }

    }

    IEnumerator ListenToDataStreamSocket()
    {
        int outConnectionId;
        int outChannelId;

        int receivedSize;
        byte error;

        byte[] buffer = new byte[24*4];
        // create a second float array and copy the bytes into it...
        var floatArray = new float[256];

        for (;;)
        {
            if (dataStreamConnectionId != -1) {
                NetworkEventType evt = NetworkTransport.ReceiveFromHost(dataStreamSocketId, out outConnectionId, out outChannelId, buffer, buffer.Length, out receivedSize, out error);
                switch (evt)
                {
                    case NetworkEventType.ConnectEvent:
                        {
                            break;
                        }
                    case NetworkEventType.DisconnectEvent:
                        {
                            break;
                        }
                    case NetworkEventType.DataEvent:
                        {
                            Buffer.BlockCopy(buffer, 0, floatArray, 0, receivedSize);

                            clientFinger.position = new Vector3(floatArray[0], floatArray[1], floatArray[2]);
                            clientSmartwatch.position = new Vector3(floatArray[3], floatArray[4], floatArray[5]);
                            clientSmartwatch.rotation = new Quaternion(floatArray[6], floatArray[7], floatArray[8], floatArray[9]);
                            clientPlane.position = new Vector3(floatArray[10], floatArray[11], floatArray[12]);
                            clientPlane.rotation = new Quaternion(floatArray[13], floatArray[14], floatArray[15], floatArray[16]);
                            clientTarget.position = new Vector3(floatArray[17], floatArray[18], floatArray[19]);
                            clientTarget.rotation = new Quaternion(floatArray[20], floatArray[21], floatArray[22], floatArray[23]);
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
            }
            yield return null;
        }
    }

    private IEnumerator ListenToCommandSocket()
    {
        int outConnectionId;
        int outChannelId;

        int receivedSize;
        byte error;

        byte[] buffer = new byte[1024];

        for (;;)
        {
            if (commandSocketId != -1)
            {
                NetworkEventType evt = NetworkTransport.ReceiveFromHost(commandSocketId, out outConnectionId, out outChannelId, buffer, buffer.Length, out receivedSize, out error);
                switch (evt)
                {
                    case NetworkEventType.ConnectEvent:
                        {
                            break;
                        }
                    case NetworkEventType.DisconnectEvent:
                        {
                            break;
                        }
                    case NetworkEventType.DataEvent:
                        {
                            Stream stream = new MemoryStream(buffer);
                            BinaryFormatter formatter = new BinaryFormatter();
                            string message = formatter.Deserialize(stream) as string;

                            string[] words = message.Split('|');

                            if (words[0] == "isTouching")
                            {
                                if (words[1] == "true")
                                {
                                    ChangeBorderColor(true);
                                }
                                else
                                {
                                    ChangeBorderColor(false);
                                }
                                
                            } else if (words[0] == "trialStatus") {
                                if (words[1] == "0" || words[1] == "3" )
                                {
                                    TrialStatusText.color = Color.red;
                                }
                                else {
                                    TrialStatusText.color = Color.green;
                                }
                                TrialStatusText.text = words[2];
                            }
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
            }
            yield return null;
        }
    }
}