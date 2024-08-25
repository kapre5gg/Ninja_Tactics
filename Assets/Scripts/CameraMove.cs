using UnityEngine;

public class CameraMove : MonoBehaviour
{
    Camera mainCam;
    public Terrain terrain;
    public Vector3 offset;
    public float radius;
    public float height;
    public float sensitivity;
    public Vector3 target;
    private Vector3 direction = Vector3.zero;
    private float currentAngle = 0.0f;
    // Start is called before the first frame update
    private void Start()
    {
        mainCam = GetComponent<Camera>();
        mainCam.transform.rotation = Quaternion.Euler(offset.x, offset.y, offset.z);
    }
    private void FixedUpdate()
    {
        target = transform.position + transform.forward * Mathf.Sqrt(Mathf.Pow(height, 2) + Mathf.Pow(radius, 2));
        GetDirection();
        MouseOutScreen();
        MouseRotate();
    }

    private void GetDirection()
    {
        if (Screen.width - Input.mousePosition.x < 0.1f)
            direction += Vector3.right;
        else if (Input.mousePosition.x < 0.1f)
            direction += Vector3.left;
        if (Screen.height - Input.mousePosition.y < 0.1f)
            direction += Vector3.forward;
        else if (Input.mousePosition.y < 0.1f)
            direction += Vector3.back;
    }

    private void MouseOutScreen()
    {
        transform.Translate(direction * sensitivity * Time.deltaTime, Space.Self);
        direction = Vector3.zero;
    }

    private void MouseRotate()
    {
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            float mouseX = Input.GetAxis("Mouse X");
            currentAngle += mouseX * sensitivity * Time.deltaTime;

            // 타겟을 중심으로 카메라 위치를 갱신
            UpdateCameraPosition(target, currentAngle);
        }
        else if (!Cursor.visible)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        UpdateCameraHeight();
    }
    void UpdateCameraPosition(Vector3 _target, float _angle)
    {
        float radians = _angle * Mathf.Deg2Rad;
        float x = _target.x + radius * Mathf.Cos(radians);
        float z = _target.z + radius * Mathf.Sin(radians);
        transform.position = new Vector3(x, transform.position.y, z);
        transform.LookAt(_target);
    }

    void UpdateCameraHeight()
    {
        if (terrain != null)
        {
            float terrainHeight = terrain.SampleHeight(target);
            transform.position = new Vector3(transform.position.x, terrainHeight, transform.position.z);
        }
    }
}
