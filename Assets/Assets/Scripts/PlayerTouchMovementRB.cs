using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(Rigidbody))]
public class PlayerTouchMovement_RB : MonoBehaviour
{
    [Header("Joystick")]
    [SerializeField] private FloatingJoystick joystick;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Jump Settings")]
    public Button jumpButton;
    public float jumpForce = 7f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundMask;

    private Rigidbody rb;
    private PlayerControls controls;
    private Vector2 moveInput;
    private Finger movementFinger;
    private bool isMovementFingerActive = false;
    private Vector2 movementAmount;
    private Canvas rootCanvas;
    private Camera mainCam;
    private Animator anim;
    private bool isJumping = false;
    private bool isTurning = false;
    private bool weaponEquipped = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        controls = new PlayerControls();
        mainCam = Camera.main;
        anim = GetComponent<Animator>();

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

        if (jumpButton != null)
            jumpButton.onClick.AddListener(TryJump);
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

        if (jumpButton != null)
            jumpButton.onClick.RemoveListener(TryJump);
    }

    private void Update()
    {
        HandleMovement();
        HandleTurning();
    }

    #region Input Handlers
    private void OnMovePerformed(InputAction.CallbackContext ctx) => moveInput = ctx.ReadValue<Vector2>();
    private void OnMoveCanceled(InputAction.CallbackContext ctx) => moveInput = Vector2.zero;
    private void OnJumpPerformed(InputAction.CallbackContext ctx) => TryJump();

    private void OnFingerDown(Finger finger)
    {
        if (!isMovementFingerActive && finger.screenPosition.x < Screen.width * 0.5f)
        {
            movementFinger = finger;
            isMovementFingerActive = true;

            if (joystick != null)
            {
                joystick.gameObject.SetActive(true);
                float size = Mathf.Clamp(Screen.width * 0.15f, 200f, 400f);
                joystick.SetSize(size);
                joystick.RectTransform.anchoredPosition = ScreenToCanvasPosition(finger.screenPosition);
                joystick.ResetKnob();
            }
        }
    }

    private void OnFingerUp(Finger finger)
    {
        if (isMovementFingerActive && finger == movementFinger)
        {
            isMovementFingerActive = false;
            movementAmount = Vector2.zero;

            if (joystick != null)
            {
                joystick.ResetKnob();
                joystick.gameObject.SetActive(false);
            }
        }
    }

    private void OnFingerMove(Finger finger)
    {
        if (!isMovementFingerActive || finger != movementFinger || joystick == null) return;

        float maxRadius = joystick.RectTransform.sizeDelta.x * 0.5f;
        Vector2 currentPos = ScreenToCanvasPosition(finger.screenPosition);
        Vector2 delta = Vector2.ClampMagnitude(currentPos - joystick.RectTransform.anchoredPosition, maxRadius);
        joystick.Knob.anchoredPosition = delta;
        movementAmount = delta / maxRadius;
    }
    #endregion

    #region Movement
    private void HandleMovement()
    {
        Vector2 input = isMovementFingerActive ? movementAmount : moveInput;
        Vector3 moveDir = GetWorldDirection(input);

        HandleStrafing(input);

        if (moveDir.sqrMagnitude > 0.1f)
        {
            RotateCharacter(moveDir);
            Vector3 targetVelocity = moveDir * moveSpeed;
            rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);
            anim.SetFloat("Locomotion", 1f);
        }
        else
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            anim.SetFloat("Locomotion", 0f);
        }

        CheckAirState();
    }

    private void HandleStrafing(Vector2 input)
    {
        if (!weaponEquipped) return;

        Vector3 strafe = transform.forward * input.y + transform.right * input.x;
        if (strafe.sqrMagnitude > 0.001f)
        {
            Vector3 targetVelocity = strafe.normalized * moveSpeed;
            rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);
            anim.SetFloat("StrafeX", input.x);
            anim.SetFloat("StrafeY", input.y);
            anim.SetFloat("Locomotion", 1f);
        }
        else
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            anim.SetFloat("StrafeX", 0f);
            anim.SetFloat("StrafeY", 0f);
            anim.SetFloat("Locomotion", 0f);
        }
    }

    private Vector3 GetWorldDirection(Vector2 input)
    {
        if (input.sqrMagnitude < 0.001f) return Vector3.zero;

        Vector3 camForward = mainCam.transform.forward;
        Vector3 camRight = mainCam.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        return (camForward * input.y + camRight * input.x).normalized;
    }

    private void RotateCharacter(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.01f) return;

        float mag = (isMovementFingerActive ? movementAmount.magnitude : moveInput.magnitude);
        Quaternion targetRot = Quaternion.LookRotation(direction);

        float dynamicSpeed = rotationSpeed * (0.5f + mag * 1.5f); 
        // min ×0.5, max ×2.0 rotation multiplier
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            dynamicSpeed * Time.deltaTime
        );
    }
    #endregion

    #region Turning
    private void HandleTurning()
    { 
       if (isTurning) return;

       Vector2 input = isMovementFingerActive ? movementAmount : moveInput;

       // Require a strong directional input
       if (input.magnitude < 0.5f) return;

       // Backwards input = 180 turn
       if (input.y < -0.6f)
       {
           StartTurn("Turn180");
       } 
    }
        
    

    private void StartTurn(string triggerName)
    {
        anim.SetTrigger(triggerName);
        isTurning = true;
    }
    #endregion

    #region Jump & Air
    public void TryJump()
    {
        if (IsGrounded() && !isJumping)
        {
            isJumping = true;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            anim.SetTrigger("JumpUnarmed");
            StartCoroutine(ResetJump());
        }
    }

    private void CheckAirState()
    {
        bool grounded = IsGrounded();
        anim.SetBool("InAir", !grounded && !isJumping);
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.1f,
                               Vector3.down,
                               groundCheckDistance + 0.1f,
                               groundMask);
    }

    private System.Collections.IEnumerator ResetJump()
    {
        yield return new WaitForSeconds(2.5f);
        anim.ResetTrigger("JumpUnarmed");
        isJumping = false;
    }
    #endregion

    #region Utilities
    private Vector2 ScreenToCanvasPosition(Vector2 screenPosition)
    {
        if (rootCanvas == null) return screenPosition;

        RectTransform canvasRect = rootCanvas.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            rootCanvas.renderMode == RenderMode.ScreenSpaceCamera ? rootCanvas.worldCamera : null,
            out Vector2 localPos);
        return localPos;
    }

    private bool IsMoving()
    {
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        return horizontalVelocity.sqrMagnitude > 0.01f;
    }

    private void StopMovement()
    {
        rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
    }
    #endregion
}