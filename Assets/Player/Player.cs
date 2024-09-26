using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public bool Sneaking { get; private set; } = false;

    [SerializeField] float speed = 50;
    //[SerializeField] float sensitivity = 10;

    [SerializeField] Transform lookAt;
    [SerializeField] Animator characterAnim;
    [SerializeField] Transform character;

    Rigidbody rb;
    bool grounded;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        StaticPlayerInput.Input.Player.Enable();

        StaticPlayerInput.Input.Player.Sneaking.started += Sneak;
        StaticPlayerInput.Input.Player.Sneaking.canceled += Sneak;
    }
    private void FixedUpdate()
    {
        rb.velocity = UpdateDirection();
        //lookAt.rotation = UpdateRotation(StaticPlayerInput.Input.Player.Look.ReadValue<Vector2>(), lookAt.rotation);
    }

/*    private Quaternion UpdateRotation(Vector2 input, Quaternion rotator)
    {
        input.Normalize();

        return Quaternion.Lerp(rotator, Quaternion.Euler(0, rotator.eulerAngles.y + input.x*sensitivity, 0), .9f);

            
    }*/

    private Vector3 UpdateDirection()
    {
        Vector2 input = StaticPlayerInput.Input.Player.Move.ReadValue<Vector2>();

        Vector3 newDirection = CameraDirectionMovement(input, Camera.main);
        newDirection.Normalize();
        newDirection *= speed;
        characterAnim.SetFloat("Speed", newDirection.magnitude);
        characterAnim.SetFloat("AnimSpeed", speed * .17f);
        if (newDirection.magnitude > 0)
            character.rotation = Quaternion.LookRotation(newDirection);

        newDirection.y = rb.velocity.y;

        return newDirection;
    }

    private Vector3 CameraDirectionMovement(Vector2 input, Camera cam)
    {
        Vector3 camRight = cam.transform.right;
        camRight.y = 0;
        camRight.Normalize();

        Vector3 camForward = cam.transform.forward;
        camForward.y = 0;
        camForward.Normalize();

        return input.x * camRight + camForward * input.y;
    }

    private void Sneak(InputAction.CallbackContext ctx)
    {
        if (ctx.canceled)
        {
            speed *= 3;
            Sneaking = false;
        }
        else
        {
            speed /= 3f;
            Sneaking = true;
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 6)
        {
            grounded = true;
            characterAnim.SetBool("Grounded", grounded);
        }

    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == 6)
        {
            grounded = false;
            characterAnim.SetBool("Grounded", grounded);
        }
    }

}
