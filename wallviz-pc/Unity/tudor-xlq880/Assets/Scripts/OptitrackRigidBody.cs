//======================================================================================================
// Copyright 2016, NaturalPoint Inc.
//======================================================================================================

using System;
using UnityEngine;


public class OptitrackRigidBody : MonoBehaviour
{
    public OptitrackStreamingClient StreamingClient;
    public Int32 RigidBodyId;

    public bool enableEuroFilter = true;
    [HideInInspector]
    public OneEuroFilter<Vector3> vector3Filter;
    [HideInInspector]
    public OneEuroFilter<Quaternion> quaternionFilter;

    public float filterFrequency = 120.0f;
    [Header("Jitter Reduction (fcmin)")]
    [Tooltip("Lower value reduces jitter (fcmin or minimum cutoff frequency).Keep above 0. Start at 1. Adjust until jitter is reasonable.Recommended 1")]
    [Range(0.0f, 1.0f)]
    public float minCutOff = 1.0f;
    [Header("Lag Reduction (Beta)")]
    [Tooltip("Higher values reduce lag (beta or slope of velocity for cutoff frequency). Keep above 0. Start at 0. Increase until no lag. Recommended 1")]
    [Range(0.0f, 1.0f)]
    public float beta = 0.0f;
    [Range(0.1f, 1.0f)]
    private float derivateCutOff = 1.0f;

    void Start()
    {
        // If the user didn't explicitly associate a client, find a suitable default.
        if ( this.StreamingClient == null )
        {
            this.StreamingClient = OptitrackStreamingClient.FindDefaultClient();

            // If we still couldn't find one, disable this component.
            if ( this.StreamingClient == null )
            {
                Debug.LogError( GetType().FullName + ": Streaming client not set, and no " + typeof( OptitrackStreamingClient ).FullName + " components found in scene; disabling this component.", this );
                this.enabled = false;
                return;
            }
        }
        vector3Filter = new OneEuroFilter<Vector3>(filterFrequency);
        quaternionFilter = new OneEuroFilter<Quaternion>(filterFrequency);
    }


    void Update()
    {
        vector3Filter.UpdateParams(filterFrequency, minCutOff, beta);
        quaternionFilter.UpdateParams(filterFrequency, minCutOff, beta);
        OptitrackRigidBodyState rbState = StreamingClient.GetLatestRigidBodyState( RigidBodyId );
        if ( rbState != null )
        {
            if (enableEuroFilter)
            {
                this.transform.localPosition = vector3Filter.Filter(rbState.Pose.Position);
                this.transform.localRotation = quaternionFilter.Filter(rbState.Pose.Orientation);
            }
            else {
                this.transform.localPosition = rbState.Pose.Position;
                this.transform.localRotation = rbState.Pose.Orientation;
            }
            
        }
    }
}
