using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [SerializeField]
    public Camera cam;

    public float normalFOV = 60f;
    public float zoomFOV = 20f;
    public float zoomSpeed = 10f;

    void Update()
    {
        float targetFOV = Input.GetMouseButton(1)
            ? zoomFOV
            : normalFOV;

        cam.fieldOfView = Mathf.Lerp(
            cam.fieldOfView,
            targetFOV,
            zoomSpeed * Time.deltaTime
        );
    }
}