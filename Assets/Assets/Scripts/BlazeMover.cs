// using UnityEngine;

// [RequireComponent(typeof(Rigidbody))]
// public class BlazeMover : MonoBehaviour
// {
//     [Header("Movement Settings")]
//     [SerializeField] private float moveSpeed = 5f;
//     [SerializeField] private float rotationSpeed = 720f;

//     [Header("Jump Settings")]
//     [SerializeField] private float jumpForce = 5f;
//     [SerializeField] private Transform groundCheck;
//     [SerializeField] private LayerMask ground;

//     [Header("Joystick Settings")]
//     [SerializeField] private FloatingJoystick joystick;

//     [Header("Animator")]
//     [SerializeField] private Animator anim;

//     private Rigidbody rb;
//     private bool jumpRequest;
//     private bool isGrounded;

//     void Start()
//     {
//         rb = GetComponent<Rigidbody>();
//         rb.freezeRotation = true;
//         rb.interpolation = RigidbodyInterpolation.Interpolate;
//         rb.drag = 0f;
//         rb.angularDrag = 0f;
//     }

//     void Update()
//     {
//         if (joystick != null && joystick.IsJumpPressed())
//             jumpRequest = true;

//         isGrounded = Physics.CheckSphere(groundCheck.position, 0.3f, ground);
//         anim.SetBool("InAir", !isGrounded);
//     }

//     void FixedUpdate()
//     {
//         UnarmedMovement();
//         HandleJump();
//     }

//     void UnarmedMovement()
//     {
//         // Read joystick input
//         Vector2 input = new Vector2(joystick.Horizontal, joystick.Vertical);

//         if (input.sqrMagnitude < 0.01f)
//         {
//             rb.velocity = Vector3.zero;
//             anim.SetFloat("Locomotion", 0f);
//             return;
//         }

//         // Camera-relative movement
//         Vector3 camForward = Camera.main.transform.forward;
//         camForward.y = 0;
//         camForward.Normalize();

//         Vector3 camRight = Camera.main.transform.right;
//         camRight.y = 0;
//         camRight.Normalize();

//         Vector3 moveDirection = (camRight * input.x + camForward * input.y).normalized;

//         // Move player
//         rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);

//         // Rotate player to face movement direction
//         Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
//         rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRotation,
//             rotationSpeed * Time.fixedDeltaTime));

//         // Update animator
//         anim.SetFloat("Locomotion", moveDirection.magnitude);
//     }

//     void HandleJump()
//     {
//         if (!jumpRequest || !isGrounded) return;

//         Vector3 velocity = rb.velocity;
//         velocity.y = 0;
//         rb.velocity = velocity;

//         rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
//         anim.SetTrigger("JumpUnarmed");
//         jumpRequest = false;
//     }
// }