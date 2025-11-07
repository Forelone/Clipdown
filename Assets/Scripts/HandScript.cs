using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HandScript : MonoBehaviour
{
    Transform Hand, Inventory;
    Rigidbody RGHand, RGBody;
    Transform Eye;
    Camera Cam;
    [SerializeField] Rigidbody CurrentlyHoldingItem;

    [SerializeField] Rigidbody PassiveHoldingItem;

    [SerializeField] Rigidbody[] CarryingItems;

    [SerializeField][Range(0, 1f)] float SkillLevel = 0.2f;

    [SerializeField] float HandSway = 1f, DefaultFOV = 90;

    Vector3[] InventoryPos;

    void Awake()
    {
        Hand = transform.Find("Hand");
        Inventory = transform.Find("Inventory");
        Cam = GetComponentInChildren<Camera>();
        Eye = Cam.transform;
        RGHand = Hand.GetComponent<Rigidbody>();
        RGBody = GetComponent<Rigidbody>();

        InventoryPos = new Vector3[CarryingItems.Length];
        for (int i = 0; i < InventoryPos.Length; i++)
        {
            InventoryPos[i] += Vector3.right * (Screen.width / InventoryPos.Length * i);
        }
    }

    void Start()
    {
        if (CurrentlyHoldingItem != null && CurrentlyHoldingItem.transform.parent != Hand)
            StartCoroutine(EquipToHand(CurrentlyHoldingItem.transform));


        for (int i = 0; i < CarryingItems.Length; i++)
        {
            if (CarryingItems[i] == null) continue;

            var IT = CarryingItems[i].transform;
            if (IT.parent != transform)
                StartCoroutine(HaulToInventory(IT,i));
            
        }
    }

    void FixedUpdate()
    {
        bool HoldingInspectButton = Input.GetKey(KeyCode.R);
        bool PressedDropButton = Input.GetKeyDown(KeyCode.Q);
        bool PressedUseButton = Input.GetKeyDown(KeyCode.E) || (HoldingInspectButton && Input.GetMouseButtonDown(0));
        bool HoldingUseButton = Input.GetMouseButton(0) && HoldingInspectButton;
        bool HoldingAimButton = Input.GetMouseButton(1);
        bool PressedExecuteButton = Input.GetMouseButtonDown(0) && !HoldingInspectButton;
        bool PressedCancelButton = Input.GetMouseButtonDown(1);
        Quaternion TargetHoldAng = Quaternion.Euler(Eye.eulerAngles + new Vector3(0f, -45f, -35f));
        Vector3 TargetHoldPos = Eye.transform.position + Eye.transform.up * 0.35f - Eye.transform.forward * 0.25f;
        Vector3 TargetInvPos = transform.position + transform.up + transform.forward * 0.25f;
        Vector3 TargetHandPos = Eye.transform.position + Eye.transform.right * .350f + Eye.transform.up * -0.2f + Eye.transform.forward * .65f;
        Vector3 TargetAimPos = Eye.transform.localPosition + Vector3.forward * .3f + Vector3.up * -.2f;

        if (CurrentlyHoldingItem != null && CurrentlyHoldingItem.TryGetComponent(out Item I))
        {
            TargetHandPos = Eye.transform.position;
            TargetHandPos += transform.right * I.DefaultHoldPos.x;
            TargetHandPos += transform.up * I.DefaultHoldPos.y;
            TargetHandPos += Eye.transform.forward * I.DefaultHoldPos.z;            

            TargetHoldAng = Quaternion.Euler(Eye.eulerAngles + I.InspectHoldAng);

            TargetAimPos = Eye.transform.position;
            TargetAimPos += Eye.transform.right * I.AimHoldPos.x;
            TargetAimPos += Eye.transform.up * I.AimHoldPos.y;
            TargetAimPos += Eye.transform.forward * I.AimHoldPos.z;

            TargetHoldPos = Eye.transform.position;
            TargetHoldPos += Eye.transform.right * I.InspectHoldPos.x;
            TargetHoldPos += Eye.transform.up * I.InspectHoldPos.y;
            TargetHoldPos += Eye.transform.forward * I.InspectHoldPos.z;
        }

        Ray EyeRay = Cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        bool DidHit = Physics.Raycast(EyeRay, out hit, 2f);
        if (PressedUseButton && DidHit)
        {
            if (hit.collider.transform.TryGetComponent(out Button B))
            {
                B.onClick.Invoke();
            }

            if (hit.transform.TryGetComponent(out Item Pickup) && CurrentlyHoldingItem == null)
            {
                StartCoroutine(EquipToHand(hit.transform));
            }

            if (hit.collider != null && hit.collider.attachedRigidbody != null && hit.transform.GetComponent<Rigidbody>() != CurrentlyHoldingItem)
            {
                PassiveHoldingItem = hit.collider.attachedRigidbody;
                DetachFromInv(PassiveHoldingItem);
                PassiveHoldingItem.velocity = Vector3.zero;
            }
        }

        if (HoldingUseButton && PassiveHoldingItem != null)
        {
            var TargetPos = EyeRay.origin + EyeRay.direction * 0.5f;
            var DirPos = TargetPos - PassiveHoldingItem.position;
            DirPos /= 0.1f;

            if (!PassiveHoldingItem.isKinematic)
                PassiveHoldingItem.velocity = DirPos;
            
            if (HoldingAimButton)
            { StartCoroutine(HaulToInventory(PassiveHoldingItem.transform)); }
        }
        else PassiveHoldingItem = null;

        if (HoldingAimButton && CurrentlyHoldingItem != null) TargetHandPos = TargetAimPos;

        if (!HoldingInspectButton)
        {
            TargetHoldAng = Eye.rotation;
        }
        else if (CurrentlyHoldingItem == null)
        {
            //TargetInvPos = transform.localPosition + Vector3.forward * 0.25f + Vector3.up * -0.25f;
            TargetHandPos = hit.point;
            TargetHoldAng = Eye.rotation;
        }
        else
        {
            TargetHandPos = TargetHoldPos;
        }

        if (PressedDropButton) DropFromHand();

        if (PressedExecuteButton && CurrentlyHoldingItem != null) CurrentlyHoldingItem.GetComponent<Item>().Execution.Invoke();

        EnableCursor(HoldingInspectButton);

        float HeadSway = RGHand.velocity.magnitude + RGHand.angularVelocity.magnitude;
        Cam.fieldOfView = DefaultFOV + HeadSway;

        Hand.rotation = Quaternion.Lerp(Hand.rotation, TargetHoldAng, 0.1f);
        Hand.position = Vector3.Lerp(Hand.position, TargetHandPos, SkillLevel);
        Inventory.position = Vector3.Lerp(Inventory.position, TargetInvPos, 0.1f);
        RGHand.angularVelocity = Vector3.Lerp(RGHand.angularVelocity,Vector3.zero,SkillLevel);
        RGHand.velocity = Vector3.Lerp(RGHand.velocity, Vector3.zero, SkillLevel);
    }

    void DetachFromInv(Rigidbody RG)
    {
        for (int i = 0; i < CarryingItems.Length; i++)
        {
            if (CarryingItems[i] != null && CarryingItems[i] == RG)
            {
                RG.transform.SetParent(null);
                CarryingItems[i].isKinematic = false;
                CarryingItems[i] = null;
                break;
            }
        }
    }

    void DropFromHand()
    {
        if (CurrentlyHoldingItem == null) return;

        CurrentlyHoldingItem.transform.SetParent(null);
        CurrentlyHoldingItem.isKinematic = false;


        Vector3 RandomForce = Eye.forward * Random.Range(3, 5) + Eye.up * Random.Range(.5f, 2) + Eye.right * Random.Range(-.5f, .5f);
        CurrentlyHoldingItem.AddForce(RandomForce, ForceMode.VelocityChange);

        CurrentlyHoldingItem = null;
    }

    IEnumerator EquipToHand(Transform ItemToEquip)
    {
        Rigidbody ItemRG = ItemToEquip.GetComponent<Rigidbody>();
        Vector3 PickupStartPos = ItemToEquip.position;
        Quaternion PickupStartRot = ItemToEquip.rotation;
        float Dist = Vector3.Distance(Hand.position, PickupStartPos);
        float TotalDist = Dist;
        Vector3 Dir = (Hand.position - ItemToEquip.position).normalized;
        ItemRG.isKinematic = true;
//        print(ItemRG.isKinematic);
        while (Dist > 1)
        {
            Dist = Vector3.Distance(Hand.position, ItemToEquip.position);
            Dir = (Hand.position - ItemToEquip.position).normalized * Time.fixedDeltaTime * 10;

            //Rotation
            float Status = Dist / TotalDist;
            ItemToEquip.rotation = Quaternion.Lerp(PickupStartRot, Hand.rotation, Status);

            //Position
            ItemToEquip.Translate(Dir,Space.World);

            yield return new WaitForFixedUpdate();
        }
        ItemToEquip.position = Hand.position;
        ItemToEquip.rotation = Hand.rotation;
        ItemToEquip.SetParent(Hand);

        CurrentlyHoldingItem = ItemRG;
    }

    void EnableCursor(bool TrueToEnable)
    {
        Cursor.lockState = (TrueToEnable) ? CursorLockMode.Confined : CursorLockMode.Locked;
        Cursor.visible = TrueToEnable;
    }

    IEnumerator HaulToInventory(Transform ItemToHaul, int Index = 0)
    {
        bool JoinOnSuccess = false;
        for (int i = 0; i < CarryingItems.Length; i++)
        {
            if (CarryingItems[i] == null) { Index = i; JoinOnSuccess = true; break; }
        }

        var RG = ItemToHaul.GetComponent<Rigidbody>();
        RG.isKinematic = true;

        Vector3 TargetPos = Inventory.position;
        float Dist = Vector3.Distance(ItemToHaul.position, TargetPos);
        while (Dist > 1)
        {
            Vector3 Dir = (TargetPos - ItemToHaul.position).normalized * Time.fixedDeltaTime * 10;
            ItemToHaul.Translate(Dir, Space.World);
            Dist = Vector3.Distance(ItemToHaul.position, TargetPos);
            yield return new WaitForFixedUpdate();
        }

        float Squish = 0.15f;
        ItemToHaul.position = TargetPos + transform.right * Index * Squish - transform.right * (InventoryPos.Length - 1) * Squish / 2;
        ItemToHaul.rotation = transform.rotation;
        ItemToHaul.SetParent(Inventory);
        if (JoinOnSuccess) CarryingItems[Index] = RG;
    }
}
