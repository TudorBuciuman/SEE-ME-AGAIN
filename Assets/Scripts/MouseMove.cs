using UnityEngine;

public class MouseLook : MonoBehaviour
{
    private float mouseSensitivity = 40f;
    public Transform playerBody;

    private float weaveFrequency = 10f;
    private float weaveRollAmount = 0.4f;
    private float leanAmount = 0.2f;
    private float smoothSpeed = 2f;

    private float xRotation = 0f;
    private float weaveTimer = 0f;
    private CharacterController playerController;

    public static bool CanLook = true;

    public PlayerMove movement;

    void Start()
    {
        Application.targetFrameRate = 60;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerController = GetComponentInParent<CharacterController>();
    }

    void Update()
    {
        if (!CanLook) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.smoothDeltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.smoothDeltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerBody.Rotate(Vector3.up * mouseX);

        float targetZRotation = 0f;

        if (movement != null && playerController != null && playerController.isGrounded)
        {
            Vector3 horizontalVelocity = movement.ActualHorizontalVelocity;
            float actualSpeed = horizontalVelocity.magnitude;

            if (actualSpeed > 0.1f)
            {
                weaveTimer += Time.deltaTime * weaveFrequency;

                float weaveTilt = Mathf.Sin(weaveTimer) * weaveRollAmount;

                float sideways = Vector3.Dot(horizontalVelocity.normalized, playerBody.right);
                float strafeLean = -sideways * leanAmount;

                targetZRotation = weaveTilt + strafeLean;
            }
            else
            {
                weaveTimer = Mathf.MoveTowards(weaveTimer, 0f, Time.deltaTime * weaveFrequency);
            }
        }
        else
        {
            weaveTimer = Mathf.MoveTowards(weaveTimer, 0f, Time.deltaTime * weaveFrequency);
        }

        float currentZ = transform.localRotation.eulerAngles.z;
        float smoothedZ = Mathf.LerpAngle(currentZ, targetZRotation, Time.deltaTime * smoothSpeed);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, smoothedZ);
    }
}