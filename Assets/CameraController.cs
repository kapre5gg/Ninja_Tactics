using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float panSpeed = 20f; // ī�޶� �̵� �ӵ�
    public float panBorderThickness = 10f; // ȭ�� ��迡�� �̵� ���� �β�
    public float scrollSpeed = 20f; // �� �ӵ�
    public float minY = 10f; // �� �� ����
    public float maxY = 80f; // �� �ƿ� ����

    public Vector2 mapSize; // �� ũ�� (x, z) ��������

    private Vector2 panLimit;

    void Start()
    {
        // �� ũ�� ������� panLimit ����
        panLimit = new Vector2(mapSize.x / 2, mapSize.y / 2);
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
        pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        pos.z = Mathf.Clamp(pos.z, -panLimit.y, panLimit.y);

        transform.position = pos;
    }
}
