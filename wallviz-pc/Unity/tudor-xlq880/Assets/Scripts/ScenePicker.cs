using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

public class ScenePicker : MonoBehaviour {

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void onClick(int sceneNumber) {
        try
        {
            SceneManager.LoadScene(sceneNumber);
        }
        catch (Exception e) {
            Debug.LogError(e.ToString());
        }
        
    }
}
