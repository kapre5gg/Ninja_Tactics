using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float panSpeed = 20f; // 카메라 이동 속도
    public float panBorderThickness = 10f; // 화면 경계에서 이동 감지 두께
    public float scrollSpeed = 20f; // 줌 속도
    public float minY = 10f; // 줌 인 제한
    public float maxY = 80f; // 줌 아웃 제한

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

    void Update()
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

        // 마우스 스크롤을 사용한 줌
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        pos.y -= scroll * scrollSpeed * 100f * Time.deltaTime;

        // 카메라 이동 제한
        pos = ClampPositionToCollider(pos);

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
}

