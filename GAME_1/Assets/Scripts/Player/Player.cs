using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    private Rigidbody2D rb;
    private Vector2 inputVector;
    private float speed_player = 3f;
    private float minspeed = 0.1f;
    private bool isRunningUp = false;
    private bool isRunningDown = false;
    private bool isRunningLeftRight = false;
    private bool rev = false;
    //private bool isAttacking;
    public bool IsRunningUp()
    {
        return isRunningUp;
    }
    public bool IsRunningDown()
    {
        return isRunningDown;
    }
    public bool IsRunningLeftRight()
    {
        return isRunningLeftRight;
    }
    public bool Rev()
    {
        return rev;
    }
    /*
    public bool IsAttcking()
    {
        return isAttacking;
    }
    */
    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        inputVector = Vector2.zero;
        if (Input.GetKey(KeyCode.W))
        {
            inputVector.y = 1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            inputVector.y = -1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            inputVector.x = -1f;
            rev = true;
        }
        if (Input.GetKey(KeyCode.D))
        {
            inputVector.x = 1f;
            rev = false;
        }
    }
    private void FixedUpdate()
    {
        HandleMovement();
    }
    private void HandleMovement()
    {
        rb.MovePosition(rb.position + inputVector * (speed_player * Time.fixedDeltaTime));
        inputVector = inputVector.normalized;
        if (Mathf.Abs(inputVector.x) > minspeed || Mathf.Abs(inputVector.y) > minspeed)
        {
            if ((inputVector.y > 0f && (inputVector.x < 0f || inputVector.x > 0f)) || inputVector.y > 0f)
                isRunningUp = true;
            else if ((inputVector.y < 0f && (inputVector.x < 0f || inputVector.x > 0f)) || inputVector.y < 0f)
                isRunningDown = true;
            else if ((inputVector.x < 0f || inputVector.x > 0f) && inputVector.y == 0f)
                isRunningLeftRight = true;
        }
        else
        {
            isRunningUp = false;
            isRunningDown = false;
            isRunningLeftRight = false;
        }
    }
}
