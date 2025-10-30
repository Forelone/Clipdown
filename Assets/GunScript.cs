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
    [SerializeField] float Recoil;

    [SerializeField] AudioClip BarrelBack, BarrelFront, Clipdown, Clipup, Shoot; AudioSource ASS; //Because it makes a s~o~u~n~d~

    Transform BarrelEnd;
    public void TryShoot()
    {
        if (BulletOnBarrel == null) return;

        BulletOnBarrel.transform.SetPositionAndRotation(BarrelEnd.position, BarrelEnd.rotation);
        BulletOnBarrel.transform.SetParent(null);
        BulletOnBarrel.enabled = true;
        BulletOnBarrel = null;

        Barrel.GetComponentInChildren<ParticleSystem>().Emit(10);

        ASS.clip = Shoot;
        ASS.Play();
    
        StartCoroutine(ShootHandle());
    }

    IEnumerator ShootHandle()
    {
        for (int i = 0; i < 5; i++)
        {
            Barrel.localPosition = Vector3.Lerp(Barrel.localPosition, BarrelEndPos,0.2f);
            yield return new WaitForFixedUpdate();
        }

        Barrel.localPosition = BarrelEndPos;

        for (int i = 0; i < 10; i++)
        {
            Barrel.localPosition = Vector3.Lerp(Barrel.localPosition, BarrelStartPos, 0.1f);
            yield return new WaitForFixedUpdate();
        }

        Barrel.localPosition = BarrelStartPos;

        InsertNewBullet();
    }

    Vector3 BarrelStartPos, BarrelEndPos;

    void Awake()
    {
        BarrelStartPos = Barrel.localPosition;
        BarrelEndPos = BarrelStartPos + Barrel.forward * BarrelPushDist;
        BarrelEnd = Barrel.Find("Tip");
        ASS = GetComponent<AudioSource>();
    }
    public void StartTouchingBarrel() => StartCoroutine(FollowMouse());

    IEnumerator FollowMouse()
    {
        bool Chamber = true;
        float StartWidth = Input.mousePosition.x;
        while (Input.GetMouseButton(0))
        {
            float CurWidth = Input.mousePosition.x;
            float Away = StartWidth - CurWidth;
            Away /= 100;
            Away = Mathf.Clamp(Away, 0, 1f);
            if (Away == 1f && Chamber != true)
            {
                Chamber = true;
                InsertNewBullet();
                ASS.clip = BarrelFront;
                ASS.Play(); //Shake it.
            }
            
            if (Away == 0f && Chamber != false)
            {
                Chamber = false;
                EjectInsideBarrel();
                ASS.clip = BarrelBack;
                ASS.Play();
            }
            Barrel.localPosition = Vector3.Lerp(BarrelEndPos, BarrelStartPos, Away);
            yield return new WaitForFixedUpdate();
        }
    }

    void InsertNewBullet()
    {
        if (Clip == null) return;
        if (BulletOnBarrel != null) return;

        BulletOnBarrel = Clip.GiveNextBullet();
    }

    void EjectInsideBarrel()
    {
        if (BulletOnBarrel == null) return;

        BulletOnBarrel.transform.SetParent(null);
        BulletOnBarrel.transform.SetPositionAndRotation(ShellThrowArea.position, ShellThrowArea.rotation);
        BulletOnBarrel.AddComponent<Rigidbody>().AddForce(transform.right * 2 + transform.up, ForceMode.VelocityChange);
        BulletOnBarrel.AddComponent<BoxCollider>().size = Vector3.one * 0.005f;
        BulletOnBarrel = null;
    }

    public void DropClip()
    {
        if (Clip == null) return;
        Clip.transform.position -= ClipInsertArea.up * ClipAcceptDist * 1.01f;
        Clip.GetComponent<Rigidbody>().isKinematic = false;
        Clip.GetComponent<Collider>().enabled = true;
        Clip.transform.SetParent(null);

        ASS.clip = Clipdown;
        ASS.Play();

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

            ASS.clip = Clipup;
            ASS.Play();
        }
        Physics.IgnoreCollision(Clip.GetComponent<Collider>(), GetComponent<Collider>(),false);
    }
}
