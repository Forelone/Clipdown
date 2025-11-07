using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    [SerializeField] string BulletType = "Pistol";
    public string Type { get { return BulletType; } }
    [SerializeField] int Speed = 1605;
    public float Spd {get { return Speed; }}
    [SerializeField] double DamageMul = 1;
    [SerializeField] TrailRenderer Trail;

    void Start()
    {
        Rigidbody RG;
        if (!TryGetComponent(out RG))
        {
            RG = gameObject.AddComponent<Rigidbody>(); //I'M A GENIUS! HAHAHAHAHAHA!
            RG.collisionDetectionMode = CollisionDetectionMode.Continuous;
            RG.mass = (float)DamageMul;
        }

        Collider CL;
        if (!TryGetComponent(out CL))
        {
            var CLB = gameObject.AddComponent<BoxCollider>();
            CLB.size = Vector3.one * 0.05f;
            CL = CLB;
        }
        CL.enabled = true;

        if (Trail != null) Trail.enabled = true;
        RG.AddForce(transform.forward * Speed, ForceMode.VelocityChange);
        Destroy(this);
    }
}
