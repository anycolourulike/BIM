using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody))]
public class PlayerTouchMovement_RB : MonoBehaviour
{
    [Header("Joystick")]
    [SerializeField] private FloatingJoystick joystick;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] float extraGravity = 200f;

    [Header("Jump Settings")]
    private bool isJumping = false;
    private bool jumpPressed = false;
    public Button jumpButton;
    public float jumpForce = 7f;
    public float groundCheckDistance = 0.4f;
    public LayerMask groundMask;
    
    [Header("Assigned by fighter Script")]
    public Transform currentTarget;

    private Rigidbody rb;
    private Fighter fighter;
    private PlayerControls controls;
    private Vector2 moveInput;
    private Finger movementFinger;
    private bool isMovementFingerActive = false;
    private Vector2 movementAmount;
    private Canvas rootCanvas;
    private Camera mainCam;
    private Animator anim;
    
    private bool isTurning = false;
    private bool weaponEquipped = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        fighter = GetComponent<Fighter>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
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
        controls.Player.Disable();

        ETouch.Touch.onFingerDown -= OnFingerDown;
        ETouch.Touch.onFingerUp -= OnFingerUp;
        ETouch.Touch.onFingerMove -= OnFingerMove;
        ETouch.EnhancedTouchSupport.Disable();

        if (jumpButton != null)
            jumpButton.onClick.RemoveListener(TryJump);
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleTurning();
        if (!IsGrounded())
            rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
    }

    #region Input Handlers
    private void OnMovePerformed(InputAction.CallbackContext ctx) => moveInput = ctx.ReadValue<Vector2>();
    private void OnMoveCanceled(InputAction.CallbackContext ctx) => moveInput = Vector2.zero;
    private void OnJumpPerformed(InputAction.CallbackContext ctx) => TryJump();

    private void OnFingerDown(Finger finger)
    {
        if (EventSystem.current.IsPointerOverGameObject(finger.index))
            return;
        if (!isMovementFingerActive && finger.screenPosition.x < Screen.width * 0.5f) {
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
        if (EventSystem.current.IsPointerOverGameObject(finger.index))
            return;
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
        CheckAirState();

        Vector2 input = isMovementFingerActive ? movementAmount : moveInput;

        if (weaponEquipped)
            HandleArmedMovement(input);
        else
            HandleUnarmedMovement(input);

        if (!IsGrounded())
            rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
    }

    private void HandleUnarmedMovement(Vector2 input)
    {
        anim.SetLayerWeight(1, 0f);
        anim.SetFloat("StrafeX", 0f);

        Vector3 moveDir = GetWorldDirection(input);

        if (moveDir.sqrMagnitude < 0.01f)
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            anim.SetFloat("Locomotion", 0f);
            return;
        }

        RotateCharacter(moveDir);
        
        //Apply Movement
        rb.velocity = new Vector3(
            moveDir.x * moveSpeed,
            rb.velocity.y,
            moveDir.z * moveSpeed
        );
        
        float blend = Mathf.Clamp01(input.magnitude); //Joystick Magnitude maps to 
        anim.SetFloat("Locomotion", Mathf.Clamp01(input.magnitude));
    }
    
    private void HandleArmedMovement(Vector2 input)
    {
        anim.SetLayerWeight(1, 1f);

        float mag = Mathf.Clamp01(input.magnitude);
        if (mag < 0.1f)
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            anim.SetFloat("Locomotion", 0f, 0.1f, Time.deltaTime);
            anim.SetFloat("StrafeX", 0f, 0.1f, Time.deltaTime);
            return;
        }

        Vector3 forward;
        Vector3 right;

        if (currentTarget != null)
        {
            forward = (currentTarget.position - transform.position).normalized;
            forward.y = 0f;
            right = Vector3.Cross(Vector3.up, forward);
        }
        else
        {
            forward = transform.forward;
            right = transform.right;
        }

        Vector3 moveDir =
            forward * input.y +
            right * input.x;

        rb.velocity = new Vector3(
            moveDir.normalized.x * moveSpeed * mag,
            rb.velocity.y,
            moveDir.normalized.z * moveSpeed * mag
        );

        anim.SetFloat("Locomotion", input.y, 0.1f, Time.deltaTime);
        anim.SetFloat("StrafeX", input.x, 0.1f, Time.deltaTime);
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
    }
    #endregion

    #region Jump & Air

    private void OnJumpPressed()
    {
        //jumpPressed = true;   // button down
        TryJump(); 
    }

    private void OnJumpReleased()
    {
        //jumpPressed = false; // allow next jump
    }
    
    public void TryJump()
    {
        if (!IsGrounded() || isJumping) return;

        isJumping = true;
        //rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        anim.SetBool("JumpUnarmed", true);
        StartCoroutine(ResetJump());
    }
    
    private IEnumerator ResetJump()
    {
      // Wait until grounded
      while (!IsGrounded())
          yield return null;

      // Small buffer to avoid edge jitter
      yield return new WaitForSeconds(0.05f);

      // Fully reset jump state
      isJumping = false;
      anim.SetBool("JumpUnarmed", false);
    }

    private void CheckAirState()
    {
        bool grounded = IsGrounded();
        anim.SetBool("InAir", !grounded && !isJumping);
    }

    private bool IsGrounded()
    {
        CapsuleCollider col = GetComponent<CapsuleCollider>();
        if (col == null) return false;

        // World-space center of the capsule
        Vector3 worldCenter = transform.TransformPoint(col.center);

        // Account for lossy scale on radius/height
        float scaleY = transform.lossyScale.y;
        float scaleX = Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
        float radius = col.radius * scaleX;
        float height = Mathf.Max(col.height * scaleY, radius * 2f);

        // half-height of the cylindrical part (distance from center to sphere centers)
        float halfHeight = Mathf.Max(0f, (height / 2f) - radius);

        // Distance to check: from center down to bottom sphere + extra
        float checkDistance = halfHeight + groundCheckDistance;

        // 1) SphereCast downward from the capsule center (good for slopes, moving ground)
        if (Physics.SphereCast(worldCenter, radius * 0.9f, Vector3.down, out RaycastHit hit, checkDistance,
                groundMask, QueryTriggerInteraction.Ignore))
        {
            return true;
        }

        // 2) Fallback: a small sphere near the feet (useful for edge cases)
        Vector3 footPos = worldCenter - Vector3.up * halfHeight;
        if (Physics.CheckSphere(footPos + Vector3.down * groundCheckDistance, radius * 0.75f, groundMask,
                QueryTriggerInteraction.Ignore))
        {
            return true;
        }

        return false;
    }
    
    private void OnDrawGizmosSelected()
    {
        CapsuleCollider col = GetComponent<CapsuleCollider>();
        if (col == null) return;

        Vector3 worldCenter = transform.TransformPoint(col.center);
        float scaleY = transform.lossyScale.y;
        float scaleX = Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
        float radius = col.radius * scaleX;
        float height = Mathf.Max(col.height * scaleY, radius * 2f);
        float halfHeight = Mathf.Max(0f, (height / 2f) - radius);
        float checkDistance = halfHeight + groundCheckDistance;

        // SphereCast gizmo (line + small sphere at hit-dist)
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(worldCenter, worldCenter + Vector3.down * checkDistance);
        Gizmos.DrawWireSphere(worldCenter, radius * 0.9f);

        // Foot sphere gizmo
        Vector3 footPos = worldCenter - Vector3.up * halfHeight;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(footPos + Vector3.down * groundCheckDistance, radius * 0.75f);
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
    
    #region Weapon

    public bool WeaponEquipped
    {
        get => weaponEquipped;
        set => weaponEquipped = value;
    }
    #endregion
}