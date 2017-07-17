using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using UnityEngine.UI;
using TouchScript.Behaviors;

public class TouchTransportSmartwatch : MonoBehaviour {

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

    public Transform Plane;
    private Transformer touchScriptTransformer;
    public Transform Target;
    public Text DebugText;
    public Text TrialStatusText;
    public Button connectButton;
  
    BinaryFormatter formatter = new BinaryFormatter();

    [Header("Touch Border")]
    public Image BorderImage;
    public Sprite OffBorderImage;
    public Sprite OnBorderImage;

    public Inertia inertiaScript;
    public TouchEventSubscriber touchEventSubscriberListener;

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


        touchScriptTransformer = Plane.GetComponent<Transformer>();

        Debug.Log("Command Socket Open. SocketId is: " + commandSocketId);

        StartCoroutine(SendPositionsToDataStreamSocket());
        StartCoroutine(ListenToCommandSocket());
        StartCoroutine(ListenToDataStreamSocket());
    }

    private IEnumerator ListenToDataStreamSocket()
    {
        int outConnectionId;
        int outChannelId;
        byte[] buffer = new byte[1024];
        int bufferSize = 1024;
        int receiveSize;
        byte error;
       
        for (;;)
        {
            NetworkEventType evnt = NetworkTransport.ReceiveFromHost(dataStreamSocketId, out outConnectionId, out outChannelId, buffer, bufferSize, out receiveSize, out error);
            switch (evnt)
            {
                case NetworkEventType.ConnectEvent:
                    if (outConnectionId == dataStreamConnectionId &&
                       (NetworkError)error == NetworkError.Ok)
                    {
                        Debug.Log("Connected");
                    }
                    break;
                case NetworkEventType.DisconnectEvent:
                    if (outConnectionId == dataStreamConnectionId)
                    {
                        Debug.Log("Connected, error:" + error.ToString());
                    }
                    break;
            }
            yield return null;
        }
    }

    private IEnumerator SendPositionsToDataStreamSocket()
    {

        byte error;

        int bufferSize = 256; //13 floats 
        byte[] buffer = new byte[bufferSize]; // 4 bytes per float

        float[] floatArray = new float[] {
            0, 0, 0,     //plane local position
            0, 0, 0, // target local position
            0, 0 // nr_of_cluthces, is_panning 
        };

        Int64 timestamp = 0;
        bool isInCluthcingArea = false;

        for (;;)
        {
            if (dataStreamConnectionId != -1)
            {

                // Send plane position (or target)
                Vector3 planeLocalPosition = Plane.localPosition;
                floatArray[0] = planeLocalPosition.x;
                floatArray[1] = planeLocalPosition.y;
                floatArray[2] = planeLocalPosition.z;

                Vector3 targetLocalPosition = Target.localPosition;
                floatArray[3] = targetLocalPosition.x;
                floatArray[4] = targetLocalPosition.y;
                floatArray[5] = targetLocalPosition.z;

                //TODO: nr_of_clutches + is_panning;
                floatArray[6] = touchEventSubscriberListener.nr_of_touches;
                floatArray[7] = touchEventSubscriberListener.is_panning;

                // create a byte array and copy the floats into it...
                Buffer.BlockCopy(floatArray, 0, buffer, 0, 8*4);

                NetworkTransport.Send(dataStreamSocketId, dataStreamConnectionId, dataStreamChannelId, buffer, bufferSize, out error);

            }
            yield return new WaitForSeconds(0.05f);
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

                            switch (words[0]) {
                                case "startTrial":
                                    {
                                        // "startTrial|-1|1"
                                        
                                        // Move plane and target in position
                                        Plane.localPosition = Vector3.zero;
                                        Target.localPosition = new Vector3(Convert.ToSingle(words[1]) * Globals.SCALE, 0.0005f, Convert.ToSingle(words[2]) * Globals.SCALE);

                                        touchScriptTransformer.enabled = true;
                                        touchEventSubscriberListener.ResetCounters();
                                    }
                                    break;
                                case "recordingTrial":
                                    {
                                    // Show debug text
                                    touchEventSubscriberListener.ResetCounters();
                                    }
                                    break;
                                case "trialFinished":
                                    {
                                        // Show trail finished debug text
                                        TrialStatusText.text = "Trial Finished";
              
                                    }
                                    break;
                                case "trialStatus":
                                    {
                                        if (words[1] == "0" || words[1] == "3")
                                        {
                                            TrialStatusText.color = Color.red;
                                        }
                                        else
                                        {
                                            TrialStatusText.color = Color.green;
                                        }
                                        TrialStatusText.text = words[2];

                                    }
                                    break;
                            case "toggleInertia":
                                {
                                    if (words[1] == "true")
                                    {
                                        ChangeBorderColor(true);
                                        inertiaScript.enabled = true;
                                    }
                                    else
                                    {
                                        ChangeBorderColor(false);
                                        inertiaScript.enabled = false;
                                    }
                                }
                                break;
                            default:
                                    break;                                
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
           
            yield return null;
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("Application ending after " + Time.time + " seconds");
        NetworkTransport.Shutdown();
    }

    public void Connect()
    {
        ConnectToDataStream();
        ConnectToCommand();
        if (dataStreamConnectionId != -1 && commandConnectionId != -1) {
            connectButton.gameObject.SetActive(false);
        }
    }

    public void ConnectToDataStream()
    {
        byte error;
        if (dataStreamConnectionId == -1)
        {
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

    public void SendCommand(string command)
    {
        if (commandConnectionId == -1)
        {
            Debug.LogError("No connection established with command");
            return;
        }

        byte error;
        int bufferSize = 1024;
        byte[] buffer = new byte[bufferSize];
        Stream stream = new MemoryStream(buffer);

        formatter.Serialize(stream, command);
        
        NetworkTransport.Send(commandSocketId, commandConnectionId, commandChannelId, buffer, bufferSize, out error);
    }
}
