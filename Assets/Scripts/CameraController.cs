using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float panSpeed = 20f; // 카메라 이동 속도
    public float panBorderThickness = 10f; // 화면 경계에서 이동 감지 두께
    public float height;
    public float scrollSpeed = 20f; // 줌 속도
    public float minY = 10f; // 줌 인 제한
    public float maxY = 80f; // 줌 아웃 제한
    private Vector3 direction = Vector3.zero;
    private float currentAngle = 0.0f;
    public Vector3 target;
    public float radius;

    private float lastClickTime;
    private float doubleClickTimeLimit = 0.7f;
    private Coroutine cor;

    public GameObject boundaryObject; // 카메라 이동 범위를 정의하는 Collider가 있는 GameObject
    private Collider boundaryCollider;

    void Start()
    {
        // boundaryObject에서 Collider 가져오기
        boundaryCollider = boundaryObject.GetComponent<Collider>();

        if (boundaryCollider == null)
        {
            Debug.LogError("boundaryObject에 Collider가 없습니다.");
        }
    }

    void FixedUpdate()
    {
        target = transform.position + transform.forward * Mathf.Sqrt(Mathf.Pow(height, 2) + Mathf.Pow(radius, 2));
        GetDirection();
        MouseOutScreen();
        MouseRotate();
        Zoom();
    }

    private void Update()
    {
        CallMyCharactor();
    }

    private void CameraMove()
    {
        Vector3 pos = transform.position;

        // 키보드 입력 또는 마우스 위치를 사용하여 카메라 이동
        if (Input.mousePosition.y >= Screen.height - panBorderThickness)
        {
            pos.z += panSpeed * Time.deltaTime;
        }
        if (Input.mousePosition.y <= panBorderThickness)
        {
            pos.z -= panSpeed * Time.deltaTime;
        }
        if (Input.mousePosition.x >= Screen.width - panBorderThickness)
        {
            pos.x += panSpeed * Time.deltaTime;
        }
        if (Input.mousePosition.x <= panBorderThickness)
        {
            pos.x -= panSpeed * Time.deltaTime;
        }
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
        transform.Translate(direction * panSpeed * Time.deltaTime, Space.Self);
        direction = Vector3.zero;
    }

    private void Zoom()
    {
        Vector3 pos = transform.position;
        // 마우스 스크롤을 사용한 줌
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        pos.y -= scroll * scrollSpeed * 100f * Time.deltaTime;

        // 카메라 이동 제한
        pos = ClampPositionToCollider(pos);
        height = pos.y;
        transform.position = pos;
    }

    Vector3 ClampPositionToCollider(Vector3 position)
    {
        if (boundaryCollider == null)
            return position;

        // Collider의 Bounds를 사용하여 카메라의 위치 제한
        Bounds bounds = boundaryCollider.bounds;
        position.x = Mathf.Clamp(position.x, bounds.min.x, bounds.max.x);
        position.y = Mathf.Clamp(position.y, minY, maxY);
        position.z = Mathf.Clamp(position.z, bounds.min.z, bounds.max.z);

        return position;
    }

    private void MouseRotate()
    {
        
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            float mouseX = Input.GetAxis("Mouse X");
            currentAngle += mouseX * panSpeed * Time.deltaTime;

            transform.Rotate(Vector3.up, currentAngle, Space.Self);

            // 타겟을 중심으로 카메라 위치를 갱신
            //UpdateCameraPosition(target, currentAngle);
        }
        else if (!Cursor.visible)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            currentAngle = 0;
        }
    }
    void UpdateCameraPosition(Vector3 _target, float _angle)
    {
        float radians = _angle * Mathf.Deg2Rad;
        float x = _target.x + radius * Mathf.Cos(radians);
        float z = _target.z + radius * Mathf.Sin(radians);
        transform.position = new Vector3(x, transform.position.y, z);
        transform.LookAt(_target);
    }


    
    private void CallMyCharactor()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (Time.time - lastClickTime < doubleClickTimeLimit) // 더블 클릭 감지
            {
                if (cor != null)
                {
                    StopCoroutine(cor);
                }
                cor = StartCoroutine(nameof(MoveCamera));
            }
            lastClickTime = Time.time;
        }
    }
    private IEnumerator MoveCamera()
    {
        Vector3 myNinjaPos = DBManager.instance.myCon.transform.position;
        Vector3 newPos = new Vector3(myNinjaPos.x, transform.position.y, myNinjaPos.z);
        float _t = 0f;
        while (_t < 0.2f)
        {
            _t += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, newPos, _t);
            yield return null;
        }
    }
}

