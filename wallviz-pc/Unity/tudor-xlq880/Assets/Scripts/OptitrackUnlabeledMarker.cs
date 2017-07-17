using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class OptitrackUnlabeledMarker : MonoBehaviour
{
    public OptitrackStreamingClient StreamingClient;
    public Int32 RigidBodyId = 1;

    public bool enableEuroFilter = true;
    [HideInInspector]
    public OneEuroFilter<Vector3> vector3Filter;
    
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

    private Vector3 initialPosition = Vector3.zero;

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
    }


    void Update()
    {
        vector3Filter.UpdateParams(filterFrequency, minCutOff, beta);
        OptitrackOtherMarkersState omState = StreamingClient.GetLatestOtherMarkersState();
        OptitrackRigidBodyState rbState = StreamingClient.GetLatestRigidBodyState(RigidBodyId);
        if ( omState != null )
        {
            try
            {

                Vector3 pos = Vector3.zero;

                if (initialPosition == Vector3.zero)
                    {
                        // Must get any marker here
                        initialPosition = transform.position = omState.OtherMarkers.Last().Position;
                    }
                
                pos = GetClosestMarker(transform.position, omState.OtherMarkers,rbState.Markers);
                
                if (enableEuroFilter)
                {
                    this.transform.position = vector3Filter.Filter(pos);
                }
                else {
                    this.transform.position = pos;
                }
                    
            }
            catch (Exception e) {
                Debug.LogError(e.ToString());   
            }
           
        }
    }

    private Vector3 GetClosestMarker(Vector3 currentPosition, List<OptitrackOtherMarker> otherMarkers, List<OptitrackMarkerState> rbMarkers)
    {
        Vector3 bestTarget = currentPosition;
        float closestDistanceSqr = Mathf.Infinity;
        foreach (OptitrackOtherMarker potentialTarget in otherMarkers)
        {
            bool isRbMarker = false;
            // Dismiss the static unlabeled marker
            if (potentialTarget.Position == initialPosition) {
                continue;
            }

            foreach (OptitrackMarkerState rbMarkerState in rbMarkers) {
                if (V3Equal(potentialTarget.Position, rbMarkerState.Position))
                    isRbMarker = true;
            }

            if (isRbMarker) {
                continue;
            }

            Vector3 directionToTarget = potentialTarget.Position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget.Position;
            }
        }
        return bestTarget;
    }

    public bool V3Equal(Vector3 a, Vector3 b)
    {
        return Vector3.SqrMagnitude(a - b) < 0.0001;
    }
}
