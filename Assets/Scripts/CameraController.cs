using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float panSpeed = 20f; // ī�޶� �̵� �ӵ�
    public float panBorderThickness = 10f; // ȭ�� ��迡�� �̵� ���� �β�
    public float height;
    public float scrollSpeed = 20f; // �� �ӵ�
    public float minY = 10f; // �� �� ����
    public float maxY = 80f; // �� �ƿ� ����
    private Vector3 direction = Vector3.zero;
    private float currentAngle = 0.0f;
    public Vector3 target;
    public float radius;

    private float lastClickTime;
    private float doubleClickTimeLimit = 0.7f;
    private Coroutine cor;

    public GameObject boundaryObject; // ī�޶� �̵� ������ �����ϴ� Collider�� �ִ� GameObject
    private Collider boundaryCollider;

    void Start()
    {
        // boundaryObject���� Collider ��������
        boundaryCollider = boundaryObject.GetComponent<Collider>();

        if (boundaryCollider == null)
        {
            Debug.LogError("boundaryObject�� Collider�� �����ϴ�.");
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

        // Ű���� �Է� �Ǵ� ���콺 ��ġ�� ����Ͽ� ī�޶� �̵�
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
        // ���콺 ��ũ���� ����� ��
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        pos.y -= scroll * scrollSpeed * 100f * Time.deltaTime;

        // ī�޶� �̵� ����
        pos = ClampPositionToCollider(pos);
        height = pos.y;
        transform.position = pos;
    }

    Vector3 ClampPositionToCollider(Vector3 position)
    {
        if (boundaryCollider == null)
            return position;

        // Collider�� Bounds�� ����Ͽ� ī�޶��� ��ġ ����
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

            // Ÿ���� �߽����� ī�޶� ��ġ�� ����
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
            if (Time.time - lastClickTime < doubleClickTimeLimit) // ���� Ŭ�� ����
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

