using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

public class TouchTrialManager : MonoBehaviour {
    public Transform smartwatchTransform;
    public Transform fingerTransform;
    public Transform targetTransform;
    public Transform planeTransform;
    public TouchStartingPointListener startingPointListener;
    public Slider TrialSlider;
    public Text TrialLabel;
    public InputField username;
    public Text notificationText;
    public Button startTrialButton;
    public Button nextTrialButton;
    public Button stopTrialButton;
    public TouchTransportOptitrackServer transportServer;
    public Toggle inertiaToggle;

    private IEnumerator trialCoroutine = null;
    OptitrackHiResTimer.Timestamp start_timestamp;
    [HideInInspector]
    public bool trialStarted = false;
    StreamWriter outputFile;

    string trial_type = "";
    string user_id = "";
    string target_start_pos = "";
    string target_distance_string = "";
    int task_number = 0;

    private List<TargetPosition> targetPositions = new List<TargetPosition>();

    void Start()
    {

        targetPositions.Clear();

        string filename = "random_targets.txt";
        string path = "";
        // read trial positions from file
        try
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                // on Android
                path = "jar:file://" + Application.dataPath + "!/assets/" + filename;
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                // on iPhone
                path = Application.dataPath + "/Raw/" + filename;
            }
            else
            {
                // on PC
                path = Application.dataPath + "/StreamingAssets/" + filename;
            }

            bool loaded = Load(path);
            Debug.Log("Data loaded =" + loaded);
            Debug.Log("target positions =" + targetPositions[2].ToString());

            TrialSlider.maxValue = targetPositions.Count - 1;
            TrialSlider.value = 0;

            // shuffle array
            var rnd = new System.Random();
            targetPositions = targetPositions.OrderBy(item => rnd.Next()).ToList<TargetPosition>();

            UpdateTrialLabel();
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }

    }

    private bool Load(string fileName)
    {
        // Handle any problems that might arise when reading the text
        try
        {
            string line;
            // Create a new StreamReader, tell it which file to read and what encoding the file
            // was saved as
            StreamReader theReader = new StreamReader(fileName, Encoding.Default);
            // Immediately clean up the reader after this block of code is done.
            // You generally use the "using" statement for potentially memory-intensive objects
            // instead of relying on garbage collection.
            // (Do not confuse this with the using directive for namespace at the 
            // beginning of a class!)
            using (theReader)
            {
                // While there's lines left in the text file, do this:
                do
                {
                    line = theReader.ReadLine();

                    if (line != null)
                    {
                        // Do whatever you need to do with the text line, it's a string now
                        // In this example, I split it into arguments based on comma
                        // deliniators, then send that array to DoStuff()
                        string[] entries = line.Split(',');
                        if (entries.Length > 0)
                            targetPositions.Add(new TargetPosition
                            {
                                x_pos = Convert.ToInt32(entries[0]),
                                y_pos = Convert.ToInt32(entries[1]),
                            });
                    }
                }
                while (line != null);
                // Done reading, close the reader and return true to broadcast success    
                theReader.Close();
                return true;
            }
        }
        // If anything broke in the try block, we throw an exception with information
        // on what didn't work
        catch (Exception e)
        {
            Console.WriteLine("{0}\n", e.Message);
            return false;
        }
    }

    public void StartTrial()
    {
        transportServer.SendCommand("trialStatus|0|Place finger in the middle of the screen");
        notificationText.text = "Waiting for confirmation";
        // create file

        if (inertiaToggle.isOn) {
           trial_type  = "touch+inertia";
        } else {
           trial_type = "touch";
        }      

        // get user_id
        user_id = username.text;
        // trial name
        target_start_pos = targetPositions[Convert.ToInt32(TrialSlider.value)].ToString();
        // target_distance
        if (target_start_pos.Contains("10"))
        {
            target_distance_string = "long";
        }
        else if (target_start_pos.Contains("5"))
        {
            target_distance_string = "medium";
        }
        else
        {
            target_distance_string = "short";
        }

        //timestamp
        start_timestamp = OptitrackHiResTimer.Now();

        //task_number
        task_number = Convert.ToInt32(TrialSlider.value);

        outputFile = new StreamWriter(user_id + "_" + trial_type + "_" + target_distance_string + "_" + target_start_pos + ".csv");

        // write header
        string line = "user_id, trial_type, target_distance, target_start_pos, sw_pos_x, sw_pos_y, sw_pos_z, sw_rot_x, sw_rot_y, sw_rot_z, sw_rot_w, finger_relative_pos_x, finger_relative_pos_y, finger_relative_pos_z, target_local_x, target_local_y, target_local_z, nr_of_clutches, is_panning, timestamp, task_number";
        outputFile.WriteLine(line);

        // notif text
        notificationText.text = "Waiting for center";

        // reset,prepare and wait for finger to be in the starting position
        transportServer.SendCommand("startTrial|" + targetPositions[Convert.ToInt32(TrialSlider.value)].x_pos + "|" + targetPositions[Convert.ToInt32(TrialSlider.value)].y_pos);
        startingPointListener.gameObject.SetActive(true);

        // start coroutine for trial
        trialCoroutine = RecordTrial();
        StartCoroutine(trialCoroutine);
    
    }

    private IEnumerator RecordTrial()
    {
        while (true)
        {
            if (trialStarted == true)
            {
                // if the user is at the starting point, begin recording
                // sw_pos
                float sw_pos_x = smartwatchTransform.position.x;
                float sw_pos_y = smartwatchTransform.position.y;
                float sw_pos_z = smartwatchTransform.position.z;

                //sw_rot
                float sw_rot_x = smartwatchTransform.rotation.x;
                float sw_rot_y = smartwatchTransform.rotation.y;
                float sw_rot_z = smartwatchTransform.rotation.z;
                float sw_rot_w = smartwatchTransform.rotation.w;

                //finger_relative_pos
                Vector3 finger_relative_pos = smartwatchTransform.InverseTransformPoint(fingerTransform.position);

                //target_local
                Vector3 target_local_pos = smartwatchTransform.InverseTransformPoint(targetTransform.position);

                //TODO:
                //nr_of_clutches
                int nr_of_clutches = transportServer.nr_of_clutches;

                //is_panning
                int is_panning = transportServer.is_panning;

                //timestamp
                OptitrackHiResTimer.Timestamp timestamp = OptitrackHiResTimer.Now();

                string line = String.Format("{0},{1},{2},{3},{4:F4},{5:F4},{6:F4},{7:F4},{8:F4},{9:F4},{10:F4},{11:F4},{12:F4},{13:F4},{14:F4},{15:F4},{16:F4},{17:D},{18:D},{19:F4},{20:D}",
                    user_id,
                    trial_type,
                    target_distance_string,
                    target_start_pos,
                    sw_pos_x, sw_pos_y, sw_pos_z,
                    sw_rot_x, sw_rot_y, sw_rot_z, sw_rot_w,
                    finger_relative_pos.x, finger_relative_pos.y, finger_relative_pos.z,
                    target_local_pos.x, target_local_pos.y, target_local_pos.z,
                    nr_of_clutches,
                    is_panning,
                    timestamp.SecondsSince(start_timestamp),
                    task_number);

                outputFile.WriteLine(line);

            }
            yield return new WaitForSeconds(0f);
        }
    }

    public void UpdateTrialLabel()
    {
        TrialLabel.text = "Trial: " + targetPositions[Convert.ToInt32(TrialSlider.value)].ToString();
    }

    public void PrepareNextTrial()
    {
        StopTrial();

        if (TrialSlider.value == TrialSlider.maxValue)
        {
            TrialSlider.value = 0;
        }
        else
        {
            TrialSlider.value += 1;
        }

        UpdateTrialLabel();
    }

    public void StopTrial()
    {
        transportServer.SendCommand("trialFinished");
        notificationText.text = "Trial Ended!";
        if (trialCoroutine != null)
        {
            StopCoroutine(trialCoroutine);
        }

        trialCoroutine = null;
        trialStarted = false;
        startingPointListener.gameObject.SetActive(false);
        try
        {
            if (outputFile != null)
            { 
                outputFile.Flush();
                outputFile.Close();
                outputFile.Dispose();
                outputFile = null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    public void ToggleInertia(Toggle inertiaToggle) {
        Debug.Log(inertiaToggle.isOn);
        if (inertiaToggle.isOn)
        {
            transportServer.SendCommand("toggleInertia|true");
        } else {
            transportServer.SendCommand("toggleInertia|false");
        }
    }
}
