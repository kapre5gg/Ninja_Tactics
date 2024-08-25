using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float panSpeed = 20f; // ī�޶� �̵� �ӵ�
    public float panBorderThickness = 10f; // ȭ�� ��迡�� �̵� ���� �β�
    public float scrollSpeed = 20f; // �� �ӵ�
    public float minY = 10f; // �� �� ����
    public float maxY = 80f; // �� �ƿ� ����

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

    void Update()
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

        // ���콺 ��ũ���� ����� ��
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        pos.y -= scroll * scrollSpeed * 100f * Time.deltaTime;

        // ī�޶� �̵� ����
        pos = ClampPositionToCollider(pos);

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
}

