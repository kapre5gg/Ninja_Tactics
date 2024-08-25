using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class TeleportLocation : MonoBehaviour
{
    [SerializeField] private Transform target; // 이동할 대상
    [SerializeField] private Transform destination; // 목표 위치
    [SerializeField] private float moveSpeed = 6f; // 이동 속도
    [SerializeField] private GameObject hookIcon; // 마우스 오버 시 표시할 아이콘
    private Camera mainCamera; // 메인 카메라

    private void Start()
    {
        // 메인 카메라를 가져옵니다
        mainCamera = Camera.main;
        if (hookIcon != null)
        {
            hookIcon.SetActive(false);
        }
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // 3D 오브젝트와 충돌했을 때
                if (hit.transform == transform)
                {
                    Debug.Log("클릭");
                    // 오브젝트를 목표로 설정하여 텔레포트 처리
                    Teleport(target);
                }
            }
        }

        // 마우스 오버 시 처리
        HandleMouseOver();
    }

    private void HandleMouseOver()
    {
        if (hookIcon != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == transform)
                {
                    // 마우스가 3D 오브젝트 위에 있을 때
                    hookIcon.SetActive(true);

                    // hookIcon 위치를 3D 오브젝트의 위로 설정
                    Vector3 screenPosition = mainCamera.WorldToScreenPoint(transform.position);
                    hookIcon.transform.position = screenPosition;
                }
                else
                {
                    // 마우스가 3D 오브젝트를 벗어났을 때
                    hookIcon.SetActive(false);
                }
            }
            else
            {
                // 마우스가 화면 밖으로 나갔을 때
                hookIcon.SetActive(false);
            }
        }
    }

    public void Teleport(Transform target)
    {
        if (target != null && destination != null)
        {
            StartCoroutine(SmoothTeleport(target));
        }
        else
        {
            Debug.LogWarning("대상 또는 목표 위치가 설정되지 않았습니다.");
        }
    }

    private IEnumerator SmoothTeleport(Transform target)
    {
        NavMeshAgent agent = target.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
        }

        Animator animator = target.GetComponent<Animator>();
        if (animator != null)
        {
            // 이동 애니메이션 시작
            animator.SetTrigger("Jump");
        }

        Vector3 startPos = target.position; // 시작 위치
        Vector3 endPos = destination.position; // 목표 위치
        float distance = Vector3.Distance(startPos, endPos); // 이동할 거리
        float elapsed = 0f; // 경과 시간

        while (elapsed < distance / moveSpeed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed * moveSpeed / distance; // 보간 비율
            target.position = Vector3.Lerp(startPos, endPos, t); // 보간하여 새로운 위치 계산
            yield return null; // 다음 프레임까지 대기
        }

        target.position = endPos; // 정확한 목표 위치로 설정

        if (agent != null)
        {
            agent.enabled = true;
        }
    }
}
