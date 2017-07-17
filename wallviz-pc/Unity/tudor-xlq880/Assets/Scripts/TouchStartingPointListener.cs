using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TouchStartingPointListener : MonoBehaviour {

    public TouchTrialManager trialManager;
    public TouchTransportOptitrackServer transportServer;
    public Text debugText;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter " + other.name);

        if (other.name == "FingerPointOptitrackTouch")
        {
            transportServer.SendCommand("trialStatus|1|Trial Recording");
            debugText.text = "Trial Recording";
            trialManager.trialStarted = true;
            transportServer.SendCommand("recordingTrial");
            gameObject.SetActive(false);
        }
    }
}
