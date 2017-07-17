using UnityEngine;
using System.Collections;
using TouchScript;
using System;

public class TouchEventSubscriber : MonoBehaviour {

    [HideInInspector]
    public float nr_of_touches = 0;
    [HideInInspector]
    public float is_panning = 0;


    public void ResetCounters() {
        nr_of_touches = 0;
        is_panning = 0;
    }

    private void OnEnable()
    {
        if (TouchManager.Instance != null)
        {
            TouchManager.Instance.TouchesBegan += touchesBeganHandler;
            TouchManager.Instance.TouchesEnded += touchesEndedHandler;
            TouchManager.Instance.TouchesMoved += touchesMovedHandler;
        }
    }

    private void touchesBeganHandler(object sender, TouchEventArgs e)
    {
        nr_of_touches += 1;
        is_panning = 1;
    }


    private void touchesMovedHandler(object sender, TouchEventArgs e)
    {
        is_panning = 1;
    }

    private void touchesEndedHandler(object sender, TouchEventArgs e)
    {
        is_panning = 0;
    }

    private void OnDisable()
    {
        if (TouchManager.Instance != null)
        {
            TouchManager.Instance.TouchesBegan -= touchesBeganHandler;
            TouchManager.Instance.TouchesEnded -= touchesEndedHandler;
            TouchManager.Instance.TouchesMoved -= touchesMovedHandler;
        }
    }
}
