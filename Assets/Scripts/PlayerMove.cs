using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed = 25f;
    private float jumpHeight = 100f;

    [Header("Running")]
    public bool Running_allowed = true;
    private float runSpeedMultiplier = 2.2f;
    private bool isRunning = false;

    private CharacterController controller;
    private Vector3 velocity;

    public bool IsGrounded => controller.isGrounded;

    public Vector3 ActualHorizontalVelocity { get; private set; }

    private Vector3 lastPosition;

    public static bool canMove = true;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        lastPosition = transform.position;
    }

    void Update()
    {
        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -5f;
        }

        Vector3 move = Vector3.zero;

        if (canMove)
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            bool isMovingInput = Mathf.Abs(x) > 0.01f || Mathf.Abs(z) > 0.01f;

            if (Running_allowed && isMovingInput)
            {
                if (Input.GetKeyDown(KeyCode.LeftShift))
                {
                    isRunning = true;
                }
            }
            else
            {
                isRunning = false;
            }

            float currentSpeed = moveSpeed * (isRunning ? runSpeedMultiplier : 1f);

            move = (transform.right * x + transform.forward * z) * currentSpeed;

            if (Running_allowed && Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            }
        }
        else
        {
            isRunning = false;
        }

        velocity.y += Physics.gravity.y * Time.deltaTime * 10f;

        Vector3 finalMovement = (move + velocity) * Time.deltaTime;
        controller.Move(finalMovement);

        Vector3 delta = transform.position - lastPosition;
        delta.y = 0f;
        ActualHorizontalVelocity = delta / Time.deltaTime;
        lastPosition = transform.position;
    }
}