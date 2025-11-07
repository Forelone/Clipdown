using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClipScript : MonoBehaviour
{
    [SerializeField] string AcceptedBulletType = "Pistol";
    [SerializeField] BulletScript[] BulletSlots;
    [SerializeField] Vector3[] BulletPos;

    /*void Start()
    {
        BulletPos = new Vector3[BulletSlots.Length];
        for (int i = 0; i < BulletSlots.Length; i++)
        {
            BulletPos[i] = BulletSlots[i].transform.localPosition;
        }
    }*/

    public BulletScript GiveNextBullet()
    {
        BulletScript B = null;
        for (int i = BulletSlots.Length - 1; i > -1; i--)
        {
            if (BulletSlots[i] != null)
            {
                B = BulletSlots[i];
                BulletSlots[i] = null;
                break;
            }
        }
        return B;
    }

    public bool LoadNextBullet(Transform Bullet)
    {
        bool Success = false;

        if (Bullet.TryGetComponent(out BulletScript Boolet) && Boolet.Type == AcceptedBulletType)
            for (int i = 0; i < BulletSlots.Length; i++)
            {
                if (BulletSlots[i] == null)
                {
                    Bullet.position = transform.TransformPoint(BulletPos[i]);
                    Bullet.rotation = transform.rotation;
                    Bullet.SetParent(transform);
                    Destroy(Bullet.GetComponent<Rigidbody>());
                    Destroy(Bullet.GetComponent<Collider>());
                    BulletSlots[i] = Boolet;
                    return true;
                }
            }

        return Success;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.TryGetComponent(out BulletScript Bullet))
        {
            LoadNextBullet(collision.collider.transform);
        }
    }
}
