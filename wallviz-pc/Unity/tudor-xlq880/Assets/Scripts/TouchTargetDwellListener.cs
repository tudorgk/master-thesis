using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class TouchTargetDwellListener : MonoBehaviour {

    public TouchTrialManager trialManager;
    public TouchTransportOptitrackServer transportServer;
    public Text debugText;

    private float timeLeft = 3;
    private IEnumerator countdownCoroutine;

    void OnTriggerEnter(Collider other)
    {
        if (other.name == "Cube")
        {
            if (trialManager.trialStarted)
            {
                // Start timer
                countdownCoroutine = Countdown();
                StartCoroutine(countdownCoroutine);
            }

        }
    }

    private IEnumerator Countdown()
    {
        while (true)
        {
            timeLeft -= Time.deltaTime;
            trialManager.notificationText.text = timeLeft.ToString();
            transportServer.SendCommand("trialStatus|2|" + timeLeft.ToString());
            debugText.text = timeLeft.ToString();

            if (timeLeft < 0)
            {
                // tell trial manager to stop trial
                trialManager.StopTrial();

                // stop coroutine
                ResetCountDown();
            }
            yield return null;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.name == "Cube")
        {
            ResetCountDown();
        }
    }

    private void ResetCountDown()
    {
        // stop coroutine
        if (countdownCoroutine != null)
        {
            try
            {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }

        }

        // reset timer
        timeLeft = 3;
    }
}
