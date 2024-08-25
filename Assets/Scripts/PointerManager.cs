using UnityEngine;
using System.Collections;

public class PointerManager : MonoBehaviour
{
    [Header("포인터 파티클 시스템")]
    public ParticleSystem pointerParticleSystem;

    public float pointerDuration = 0.5f; // 포인터가 화면에 표시되는 시간 (초)

    [Header("포인터 오프셋")]
    public float pointerYOffset = 0.5f; // 포인터가 지형보다 위에 떠 있게 하는 y 오프셋

    void Start()
    {
        if (pointerParticleSystem == null)
        {
            Debug.LogError("Pointer ParticleSystem을 할당하세요.");
            return;
        }

        pointerParticleSystem.Stop(); // 초기에는 비활성화
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // 마우스 왼쪽 버튼 클릭
        {
            Vector2 screenPosition = Input.mousePosition; // 마우스 클릭 위치 (스크린 좌표)
            ShowPointer(screenPosition);
        }
    }

    void ShowPointer(Vector2 screenPosition)
    {
        if (pointerParticleSystem == null)
            return;

        // Raycasting 사용하여 화면 좌표를 월드 좌표로 변환
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        // 레이캐스트가 충돌하는 지점을 찾기 위해 특정 레이어를 지정할 수 있습니다.
        if (Physics.Raycast(ray, out hit))
        {
            // 포인터를 충돌 위치에 배치하되, y 오프셋을 적용
            Vector3 worldPosition = hit.point;
            worldPosition.y += pointerYOffset; // y 오프셋 적용
            pointerParticleSystem.transform.position = worldPosition;

            // 파티클 시스템 재생
            pointerParticleSystem.Play();

            // 포인터가 일정 시간 후 사라지게 함
            Invoke("HidePointer", pointerDuration);
        }
    }

    void HidePointer()
    {
        if (pointerParticleSystem != null)
        {
            // 파티클 시스템 중지
            pointerParticleSystem.Stop();
        }
    }
}
