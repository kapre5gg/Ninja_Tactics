using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GHTeleportLocation : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform destination; // 목표 위치
    [SerializeField] private float moveSpeed = 1f; // 이동 속도
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
    private void OnTriggerEnter(Collider other) //콜라이더가 부딪혔을때 아래 코드 실행
    {
        if (other.CompareTag("Player")) //플레이어 태그 감지할때
        {
            player = other.transform; //플레이어는 트랜스폼이라고 함.
            switchUpdate(gameObject.tag); //switchUpdate함수를 실행 
        }
    }
    void switchUpdate(string _tag) //1. 사다리 오르기, 2. 갈고리 오르기, 3. 동굴이동
    {
        switch (_tag)
        {
            case "Ladder":
                // 타겟 방향으로 회전
                Vector3 direction = (transform.position - player.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                player.rotation = Quaternion.Slerp(player.rotation, lookRotation, Time.deltaTime * 10f);
                DBManager.instance.myCon.ChangeAnimBool("Climbing", true); //태그를 검수해서 사다리이면 사다리 애니메이션 실행
                Teleport(player);
                break;

            case "Jump":
                DBManager.instance.myCon.ChangeAnim("Jump");
                break;

            case "Cave":
                player.position = destination.position;
                break;
        }

    }

    public void Teleport(Transform target)
    {
        if (target != null && destination != null)
        {
            Debug.Log("텔레포트");
            StartCoroutine(SmoothTeleport(target));
        }
        else
        {
            Debug.LogWarning("대상 또는 목표 위치가 설정되지 않았습니다.");
        }
    }
    private IEnumerator SmoothTeleport(Transform target)
    {
        Debug.Log("스무스텔레포팅");
        NavMeshAgent agent = target.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
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
        DBManager.instance.myCon.ChangeAnimBool("Climbing", false);
    }

}