using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class TeleportLocation : MonoBehaviour
{
    [SerializeField] private Transform target; // 이동할 대상
    [SerializeField] private Transform destination; // 목표 위치
    [SerializeField] private float moveSpeed = 6f; // 이동 속도

    // 대상(target)을 목표 위치(destination)로 부드럽게 이동시키는 메서드
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
