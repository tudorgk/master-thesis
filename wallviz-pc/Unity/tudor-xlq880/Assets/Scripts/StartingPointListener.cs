using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StartingPointListener : MonoBehaviour {

    public TrialManager trialManager;
    public UDPTransportServer transportServer;
    public Text debugText;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter " + other.name);

        if (other.name == "FingerPointServer")
        {
            transportServer.SendCommand("trialStatus|1|Trial Recording");
            debugText.text = "Trial Recording";
            trialManager.trialStarted = true;
            transportServer.SendCommand("recordingTrial");
            gameObject.SetActive(false);

        }
    }

}
