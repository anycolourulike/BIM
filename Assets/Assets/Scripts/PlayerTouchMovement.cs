using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(CharacterController))]
public class PlayerTouchMovement : MonoBehaviour
{
    [Header("Joystick")]
    [SerializeField] private FloatingJoystick joystick;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private CharacterController playerController;

    private Finger movementFinger;
    private Vector2 movementAmount;
    private float verticalVelocity;

    private PlayerControls controls;
    private Vector2 moveInput;
    private Animator anim;

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<CharacterController>();

        controls = new PlayerControls();
        anim = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        controls.Player.Enable();
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        ETouch.EnhancedTouchSupport.Enable();
        ETouch.Touch.onFingerDown += OnFingerDown;
        ETouch.Touch.onFingerUp += OnFingerUp;
    }

    private void OnDisable()
    {
        controls.Player.Move.performed -= ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled -= ctx => moveInput = Vector2.zero;
        controls.Player.Disable();

        ETouch.Touch.onFingerDown -= OnFingerDown;
        ETouch.Touch.onFingerUp -= OnFingerUp;
        ETouch.EnhancedTouchSupport.Disable();
    }

    private void OnFingerDown(Finger finger)
    {
        if (movementFinger != null) return;
        if (finger.screenPosition.x > Screen.width / 2f) return; // left side only

        movementFinger = finger;

        float size = Screen.width * 0.15f;
        joystick.SetSize(size);

        Vector2 localPos = ScreenToCanvasPosition(finger.screenPosition);
        joystick.RectTransform.anchoredPosition = localPos;
    }

    private void OnFingerUp(Finger finger)
    {
        if (finger != movementFinger) return;
        movementFinger = null;
        joystick.ResetKnob();
    }

    private Vector2 ScreenToCanvasPosition(Vector2 screenPos)
    {
        Canvas canvas = joystick.GetComponentInParent<Canvas>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null,
            out localPos
        );
        return localPos;
    }

    private void Update()
    {
        Vector2 finalInput = movementFinger != null ? joystick.Input : moveInput;
        movementAmount = finalInput; // Use for animator

        Vector3 move = new Vector3(finalInput.x, 0f, finalInput.y) * moveSpeed * Time.deltaTime;

        verticalVelocity = playerController.isGrounded ? -0.5f : verticalVelocity - gravity * Time.deltaTime;
        move.y = verticalVelocity * Time.deltaTime;

        playerController.Move(move);

        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(new Vector3(finalInput.x, 0f, finalInput.y));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            anim.SetFloat("Locomotion", movementAmount.magnitude);
        }        
    }
}