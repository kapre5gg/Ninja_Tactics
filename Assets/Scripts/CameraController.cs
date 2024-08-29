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
    public NinjaTacticsManager tacticsManager;
    public Vector3 target;
    public float radius;

    private float lastClickTime;
    private float doubleClickTimeLimit = 0.7f;
    private Coroutine cor;
    private Vector2 cameraCenter;
    public GameObject boundaryObject; // ī�޶� �̵� ������ �����ϴ� Collider�� �ִ� GameObject
    private Collider boundaryCollider;

    void Start()
    {
        cameraCenter = new Vector2(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2);
        // boundaryObject���� Collider ��������
        boundaryCollider = boundaryObject.GetComponent<Collider>();

        if (boundaryCollider == null)
        {
            Debug.LogError("boundaryObject�� Collider�� �����ϴ�.");
        }
    }

    void FixedUpdate()
    {
        if (!tacticsManager.ISGamePlay)
            return;
        //target = transform.position + transform.forward * Mathf.Sqrt(Mathf.Pow(height, 2) + Mathf.Pow(radius, 2));
        GetDirection();
        MouseOutScreen();
        MouseRotate();
        //Zoom();
        OffsetZoom();
        //UpdateHeight();
    }

    private void Update()
    {
        CallCharacters();
    }

    private void CameraMove() //Old version
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
    private void Zoom()
    {
        // ���콺 ��ũ���� ����� ��
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Vector3 pos = transform.position;
        pos.y -= scroll * scrollSpeed * 100f * Time.deltaTime;

        // ī�޶� �̵� ����
        pos = ClampPositionToCollider(pos);
        height = pos.y;
        transform.position = pos;
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
    public Vector3 offset;
    private void UpdateHeight()
    {
        Ray ray = Camera.main.ScreenPointToRay(cameraCenter);
        Physics.Raycast(ray, out RaycastHit hit);
        Vector3 tempPos = transform.position;
        // Collider�� Bounds�� ����Ͽ� ī�޶��� ��ġ ����
        Bounds bounds = boundaryCollider.bounds;
        tempPos.x = Mathf.Clamp(tempPos.x, bounds.min.x, bounds.max.x);
        tempPos.y = hit.point.y + 10f + offset.y;
        tempPos.z = Mathf.Clamp(tempPos.z, bounds.min.z, bounds.max.z);
        transform.position = tempPos;
    }
    private void OffsetZoom()
    {
        // ���콺 ��ũ���� ����� ��
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0f)
            return;
        offset.y = -scroll * scrollSpeed * 10f * Time.deltaTime;
    }
    

    Vector3 ClampPositionToCollider(Vector3 position)
    {
        if (boundaryCollider == null)
            return position;
        
        Ray ray = Camera.main.ScreenPointToRay(cameraCenter);
        Physics.Raycast(ray, out RaycastHit hit);

        // Collider�� Bounds�� ����Ͽ� ī�޶��� ��ġ ����
        Bounds bounds = boundaryCollider.bounds;
        position.x = Mathf.Clamp(position.x, bounds.min.x, bounds.max.x);
        position.y = Mathf.Clamp(hit.point.y + position.y, hit.point.y + minY, hit.point.y + maxY);
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

    private void CallCharacters()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            DoubleClick(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            DoubleClick(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            DoubleClick(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            DoubleClick(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            DoubleClick(4);
        }
    }
    private void DoubleClick(int _num)
    {
        if (Time.time - lastClickTime < doubleClickTimeLimit) // ���� Ŭ�� ����
        {
            if (cor != null)
            {
                StopCoroutine(cor);
            }
            cor = StartCoroutine(MoveCamera(_num));
        }
        lastClickTime = Time.time;
    }
    private IEnumerator MoveCamera(int _num)
    {
        target = tacticsManager.playerNinjaCons[_num].transform.position;
        if (target == Vector3.zero)
            yield break;
        Vector3 newPos = new Vector3(target.x, transform.position.y, target.z);
        float _t = 0f;
        while (_t < 0.2f)
        {
            _t += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, newPos, _t);
            yield return null;
        }
    }
}

