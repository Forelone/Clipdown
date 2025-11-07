using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Item : MonoBehaviour
{
    public Vector3 InspectHoldAng, InspectHoldPos;
    public Vector3 DefaultHoldPos = new Vector3(0.350f, 1.4f, 0.65f), AimHoldPos;

    public UnityEvent Execution;
}
