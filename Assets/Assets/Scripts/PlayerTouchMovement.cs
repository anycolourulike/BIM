using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerTouchMovement : MonoBehaviour
{
    [Header("Joystick")]
    [SerializeField] private FloatingJoystick joystick;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float stoppingDistance = 0.1f;
    [SerializeField] private NavMeshAgent agent;

    [Header("Jump")]
    public Button jumpButton;
    public float jumpForce = 5f;
    public float groundCheckDistance = 0.1f;
    public LayerMask groundMask;
    private bool isJumping = false;
    private PlayerControls controls;
    private Vector2 moveInput;              // Gamepad or keyboard input
    private Finger movementFinger;          // Finger controlling the joystick
    private Vector2 movementAmount; 
    private Canvas rootCanvas;  
    private Camera mainCam;      // Touch-based movement delta
    private  Animator anim;
    private Rigidbody rb;
    
    

    private void Awake()
    {
        // Ensure agent exists
        if (!agent) agent = GetComponent<NavMeshAgent>();
        //agent settings
        agent.updateRotation = false;
        agent.updatePosition = true;
       
         // Rigidbody setup for collision only
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;        

        // Initialize references
        controls = new PlayerControls();
        anim = GetComponentInChildren<Animator>();
        mainCam = Camera.main;

        if (joystick != null)
        {
            joystick.gameObject.SetActive(false);
            rootCanvas = joystick.GetComponentInParent<Canvas>();
        }
    }

    private void OnEnable()
    {
        controls.Player.Enable();
        controls.Player.Move.performed += OnMovePerformed;
        controls.Player.Move.canceled += OnMoveCanceled;
        controls.Player.Jump.performed += OnJumpPerformed;

        ETouch.EnhancedTouchSupport.Enable();
        ETouch.Touch.onFingerDown += OnFingerDown;
        ETouch.Touch.onFingerUp += OnFingerUp;
        ETouch.Touch.onFingerMove += OnFingerMove;
    }

    private void OnDisable()
    {
        controls.Player.Move.performed -= OnMovePerformed;
        controls.Player.Move.canceled -= OnMoveCanceled;
        controls.Player.Jump.performed -= OnJumpPerformed;
        controls.Player.Disable();

        ETouch.Touch.onFingerDown -= OnFingerDown;
        ETouch.Touch.onFingerUp -= OnFingerUp;
        ETouch.Touch.onFingerMove -= OnFingerMove;
        ETouch.EnhancedTouchSupport.Disable();
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }

     private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        TryJump();
    }

      private void TryJump()
    {
        if (IsGrounded() && !isJumping)
        {
            Jump();
        }
    }

      private bool IsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, groundCheckDistance + 0.1f, groundMask);
    }

    private void Jump()
    {
        isJumping = true;
        rb.isKinematic = false;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        agent.ResetPath(); // Stop NavMeshAgent while in air
        StartCoroutine(JumpRoutine());
    }

    private System.Collections.IEnumerator JumpRoutine()
    {
        while (!IsGrounded())
            yield return null;

        rb.isKinematic = true;
        isJumping = false;
    }

    // --- TOUCH INPUT HANDLING ---

    private void OnFingerDown(Finger finger)
    {
        // Activate joystick on left half of screen
        if (movementFinger == null && finger.screenPosition.x < Screen.width * 0.5f)
        {
            movementFinger = finger;
            joystick.gameObject.SetActive(true);

            float size = Mathf.Clamp(Screen.width * 0.15f, 200f, 400f);
            joystick.SetSize(size);

            // Convert to anchored position relative to canvas
            joystick.RectTransform.anchoredPosition = ScreenToCanvasPosition(finger.screenPosition);
            joystick.ResetKnob();
        }
        else PlayerStop();
    }

    private void OnFingerUp(Finger finger)
    {
        if (finger == movementFinger)
        {
            movementFinger = null;
            joystick.ResetKnob();
            joystick.gameObject.SetActive(false);
            movementAmount = Vector2.zero;
        }
    }

    private void OnFingerMove(Finger finger)
    {
        if (finger != movementFinger || joystick == null) return;

        float maxRadius = joystick.RectTransform.sizeDelta.x * 0.5f;
        Vector2 currentPos = ScreenToCanvasPosition(finger.screenPosition);
        Vector2 delta = currentPos - joystick.RectTransform.anchoredPosition;
        delta = Vector2.ClampMagnitude(delta, maxRadius);

        joystick.Knob.anchoredPosition = delta;
        movementAmount = delta / maxRadius;
    }

    private Vector2 ScreenToCanvasPosition(Vector2 screenPosition)
    {
        if (rootCanvas == null) return screenPosition;

        RectTransform canvasRect = rootCanvas.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            rootCanvas.renderMode == RenderMode.ScreenSpaceCamera ? rootCanvas.worldCamera : null,
            out Vector2 localPos
        );
        return localPos;
    }

    // --- MOVEMENT ---

    private void Update()
    {
        Vector2 input = movementFinger != null ? movementAmount : moveInput;
        Vector3 moveDir = GetWorldDirection(input);

        if (moveDir.sqrMagnitude > 0.01f)
        {
            MoveCharacter(moveDir);
            RotateCharacter(moveDir);
        }
        else
        {
            PlayerStop();
        }

        UpdateAnimator();
    }

    private void PlayerStop()
    {
        Debug.Log("PlayerStop");
        agent.ResetPath();
        agent.speed = 0f;
        anim.SetFloat("Locomotion", 0f);
    }

    private Vector3 GetWorldDirection(Vector2 input)
    {
        if (input.sqrMagnitude < 0.001f) return Vector3.zero;

        Vector3 camForward = mainCam.transform.forward;
        Vector3 camRight = mainCam.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        return (camForward * input.y + camRight * input.x).normalized;
    }

    private void MoveCharacter(Vector3 direction)
    {
        Vector3 targetPos = transform.position + direction * moveSpeed * Time.deltaTime;
        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 0.5f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    private void RotateCharacter(Vector3 direction)
    {
        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }

    private void UpdateAnimator()
    {        
        if (anim == null) 
        {
            Debug.Log("No animator on player object");
            return;
        }

        float velocity = agent.speed;
        Debug.Log("Player velocity is" + " " + velocity);
        anim.SetFloat("Locomotion", velocity, 1f, Time.deltaTime);
    }
}
