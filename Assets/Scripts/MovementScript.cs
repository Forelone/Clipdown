using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class MovementScript : MonoBehaviour
{
    [SerializeField] Vector2 RawMovement,MouseMovement;

    Rigidbody RG;
    Transform Eyes;
    [SerializeField] float WalkSpeed = 2f, SprintMul = 3f;

    void Awake()
    {
        RG = GetComponent<Rigidbody>();
        Eyes = Camera.main.transform;
    }

    void FixedUpdate()
    {
        RawMovement = Vector2.up * Input.GetAxisRaw("Vertical") + Vector2.right * Input.GetAxisRaw("Horizontal");
        MouseMovement += Vector2.right * Input.GetAxis("Mouse X") + Vector2.up * Input.GetAxis("Mouse Y");
        MouseMovement.y = Mathf.Clamp(MouseMovement.y, -90, 90);

        Vector3 DesiredMovement = transform.forward * RawMovement.y * WalkSpeed + transform.right * RawMovement.x * WalkSpeed;

        if (Input.GetKey(KeyCode.LeftShift)) DesiredMovement *= SprintMul;

        float Grav = RG.velocity.y;
        RG.velocity = Vector3.Lerp(RG.velocity, DesiredMovement + Vector3.up * Grav, 0.1f);
        transform.rotation = Quaternion.Euler(transform.up * MouseMovement.x);
        Eyes.localRotation = Quaternion.Euler(Vector3.left * MouseMovement.y);
    }
}
