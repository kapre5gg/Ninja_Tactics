using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float panSpeed = 20f; // 카메라 이동 속도
    public float panBorderThickness = 10f; // 화면 경계에서 이동 감지 두께
    public float scrollSpeed = 20f; // 줌 속도
    public float minY = 10f; // 줌 인 제한
    public float maxY = 80f; // 줌 아웃 제한

    public Vector2 mapSize; // 맵 크기 (x, z) 방향으로

    private Vector2 panLimit;

    void Start()
    {
        // 맵 크기 기반으로 panLimit 설정
        panLimit = new Vector2(mapSize.x / 2, mapSize.y / 2);
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
        pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        pos.z = Mathf.Clamp(pos.z, -panLimit.y, panLimit.y);

        transform.position = pos;
    }
}
