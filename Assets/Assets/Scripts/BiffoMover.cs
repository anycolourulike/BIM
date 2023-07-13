using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class BiffoMover : MonoBehaviour
{
    [SerializeField] Animator anim; 
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask ground;
    
    Vector3 movementDir;  
    Rigidbody rb;
   // bool stopped;
    bool moveForward;
    bool moveBackward;
    bool moveRight;
    bool moveLeft;
    float horizontalMove;
    float verticalMove;
    public float speed = 5f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }  

    public void PointerDownForward()
    {    
        moveForward = true;        
    }

    public void PointerUpForward()
    {
        moveForward = false;
        StopMoving();
    }

    public void PointerDownBackward()
    {   
        moveBackward = true;        
    }

    public void PointerUpBackward()
    {
        moveBackward = false;
        StopMoving();
    }

    public void PointerDownRight()
    {
        moveRight = true;
    }

    public void PointerUpRight()
    {
        moveRight = false;
        StopMoving();
    }

    public void PointerDownLeft()
    {
        moveLeft = true;
    }

    public void PointerUpLeft()
    {
        moveLeft = false;
        StopMoving();
    }


    private void Movement()
    {
        if (moveLeft)
        {
            horizontalMove = -speed; 
            anim.SetFloat("x", horizontalMove);
        }
        else if (moveRight)
        {  
            horizontalMove = speed; 
            anim.SetFloat("x", horizontalMove);
        }
        else
        {
            horizontalMove = 0;
        }

        if (moveForward)
        { 
            verticalMove = speed;
            anim.SetFloat("x", verticalMove);
        }
        else if (moveBackward)
        { 
            verticalMove = -speed;
            anim.SetFloat("x", verticalMove);
        }
        else
        {
            verticalMove = 0;
        }
    }

    private void Update()
    {
        Movement();
        if(IsGrounded() == true)
        {
           anim.SetBool("InAir", false); 
        }
        else
        {
           anim.SetBool("InAir", true);
        }

        Debug.DrawLine(groundCheck.position, groundCheck.position + Vector3.down, Color.red);
    }

    void StopMoving()
    {
        anim.SetFloat("x", 0);
        anim.SetFloat("y", 0);
        //stopped = true;
        movementDir = new Vector3(0f,0f,0f);
        transform.Translate(movementDir, Space.World);
    }

    private void FixedUpdate()
    {   
        movementDir = new Vector3(horizontalMove, 0 , verticalMove);  
        movementDir.Normalize(); 
        transform.Translate(movementDir * speed * Time.deltaTime, Space.World);
         
        if(movementDir != Vector3.zero)
        {
            transform.forward = movementDir;
        }
    } 

    bool IsGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, .3f, ground);
    } 
}