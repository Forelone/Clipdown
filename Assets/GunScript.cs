using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class GunScript : MonoBehaviour
{
    [SerializeField] Transform Barrel;
    [SerializeField] BulletScript BulletOnBarrel;
    [SerializeField] ClipScript Clip;
    [SerializeField] Transform ClipInsertArea;
    [SerializeField] Transform ShellThrowArea;
    [SerializeField] float ClipAcceptDist, ClipRejectDist;
    [SerializeField] float BarrelPushDist;

    public void TryShoot()
    {
        if (BulletOnBarrel == null) return;
    }

    Vector3 BarrelStartPos, BarrelEndPos;

    void Awake()
    {
        BarrelStartPos = Barrel.localPosition;
        BarrelEndPos = BarrelStartPos + Barrel.forward * BarrelPushDist;
    }
    public void StartTouchingBarrel() => StartCoroutine(FollowMouse());

    IEnumerator FollowMouse()
    {
        float StartWidth = Input.mousePosition.x;
        while (Input.GetMouseButton(0))
        {
            float CurWidth = Input.mousePosition.x;
            float Away = StartWidth - CurWidth;
            Away /= 10;
            Away = Mathf.Clamp(Away, 0, 1f);

            if (Away == 1)
            {
                InsertBulletInsideChamber();
            }
            Barrel.localPosition = Vector3.Lerp(BarrelEndPos, BarrelStartPos, Away);
            yield return new WaitForFixedUpdate();
        }
    }
    
    void InsertBulletInsideChamber()
    {
        
    }

    public void DropClip()
    {
        if (Clip == null) return;
        Clip.transform.position -= ClipInsertArea.up * ClipAcceptDist * 1.01f;
        Clip.GetComponent<Rigidbody>().isKinematic = false;
        Clip.GetComponent<Collider>().enabled = true;
        Clip.transform.SetParent(null);

        Clip = null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (Clip != null) return;

        if (other.transform.TryGetComponent(out ClipScript CS))
        {
            Physics.IgnoreCollision(other, GetComponent<Collider>());
            StartCoroutine(ClipFollow(CS));
        }
    }

    IEnumerator ClipFollow(ClipScript Clip)
    {
        var Dist = Vector3.Distance(transform.position, Clip.transform.position);
        var RG = Clip.GetComponent<Rigidbody>();
        while (Dist > ClipAcceptDist)
        {
            RG.MovePosition(ClipInsertArea.position - ClipInsertArea.up * Dist);
            RG.MoveRotation(ClipInsertArea.rotation);

            Dist = Vector3.Distance(transform.position, Clip.transform.position);
            if (Dist > ClipRejectDist) break;
            yield return new WaitForFixedUpdate();
        }

        if (Dist < ClipAcceptDist)
        {
            RG.GetComponent<Collider>().enabled = false;
            RG.isKinematic = true;
            RG.transform.SetParent(transform);
            Clip.transform.SetPositionAndRotation(ClipInsertArea.position, ClipInsertArea.rotation);
            this.Clip = Clip;
        }
        Physics.IgnoreCollision(Clip.GetComponent<Collider>(), GetComponent<Collider>(),false);
    }
}
