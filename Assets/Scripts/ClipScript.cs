using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClipScript : MonoBehaviour
{
    [SerializeField] string AcceptedBulletType = "Pistol";
    [SerializeField] BulletScript[] BulletSlots;

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
                    BulletSlots[i] = Boolet;
                    return true;
                }
            }

        return Success;
    }
}
