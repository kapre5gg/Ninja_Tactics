using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Citizen : Enemy
{
    // 도움요청
    protected override void AttackState()
    {
        if (!isAttacking && atkTarget != null) // 공격 중이 아니고 타겟이 존재하면
        {
            StartCoroutine(AttackRoutine()); // 공격 코루틴 실행
        }
    }
    private IEnumerator AttackRoutine()
    {
        if (fieldOfView.isViewMeshVisible) // 시야 범위 메쉬가 활성화되어있다면
        {
            fieldOfView.isViewMeshVisible = false; // 시야 범위 메쉬 비활성화
        }

        StartCoroutine(SetDialogue("Attack")); // 공격 대사 출력
       
        if (!isAttacking && atkTarget != null)
        {
            isAttacking = true; // 공격 중 상태 설정

            // 가장 가까운 적을 찾아 이동 후 플레이어 위치 알림
            yield return StartCoroutine(MoveToClosestEnemyAndNotify());

            yield return new WaitForSeconds(attackInterval);
            enemyState = EnemyState.Patrol; // 순찰 상태로 전환
            isAttacking = false; // 공격 중 상태 해제
        }
        

        // 타겟이 공격 범위 안에 아직 있는지 체크
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange); // 본인 위치에서 attackRange 범위만큼의 원형 범위 안에 존재하는 모든 콜라이더 hitColliders에 저장
        bool targetStillInRange = false;

        foreach (var collider in hitColliders) // hitColliders의 모든 collider 순회
        {
            if (collider.CompareTag("Player"))  // collider의 태그가 "Player"이면
            {
                targetStillInRange = true; // 타겟이 아직 존재함
                break;
            }
        }

        if (!targetStillInRange) // 타겟이 존재하지 않으면
        {
            enemyState = EnemyState.Patrol; // 순찰 상태로 전환 
            agent.SetDestination(waypoints[currentWaypointIndex].position); // 에이전트의 이동 방향을 현재 인덱스의 웨이포인트로 설정
            if (fieldOfView.isViewMeshVisible) // 시야 범위 메쉬가 활성화되어있다면
            {
                fieldOfView.isViewMeshVisible = false; // 시야 범위 메쉬 비활성화
            }
        }
    }

    private IEnumerator MoveToClosestEnemyAndNotify()
    {
        Vector3 playerLastKnownPosition = atkTarget.transform.position; // 플레이어의 현재 위치를 초기값으로 설정
        Guard closestGuard = FindClosestGuard();
        if (closestGuard == null)
        {
            Debug.Log("가까운 적 없음");
            yield break;
        }
        anim.SetBool("Running", true); // 애니메이션 설정
        // 가까운 적에게 이동
        agent.SetDestination(closestGuard.transform.position);
        Debug.Log("적에게 이동 시작...");

        // stoppingDistance를 적의 위치에 적절하게 설정
        agent.stoppingDistance = 1.0f;  // 도착했다고 판단할 거리 (예: 1.0m)

        while (true)
        {
            // 도착을 판단하기 위해 stoppingDistance를 사용
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                Debug.Log("적에게 도착");
                anim.SetBool("Running", false); // 애니메이션 설정
                break;
            }

            // 적의 위치가 크게 변했을 때에만 목적지를 갱신
            if (Vector3.Distance(agent.destination, closestGuard.transform.position) > agent.stoppingDistance)
            {
                agent.SetDestination(closestGuard.transform.position);
                Debug.Log("적의 위치 갱신");
            }

            yield return null;  // 매 프레임마다 확인
        }
        
        // 도달한 후에 플레이어가 발견된 위치로 이동하도록 적에게 명령
        closestGuard.MoveToLastKnownPosition(playerLastKnownPosition);
        //agent.SetDestination(playerLastKnownPosition);
    }


    // 주변의 다른 적들 중 가장 가까운 적을 찾는 메서드
    private Guard FindClosestGuard()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 50f); // 50m 반경 내의 모든 콜라이더 감지
        Guard closestGuard = null;
        float closestDistance = Mathf.Infinity;

        foreach (var collider in hitColliders)
        {
            Guard nearbyGuard = collider.GetComponent<Guard>();
            if (nearbyGuard != null && nearbyGuard != this) // 자신을 제외한 다른 Enemy 객체
            {
                float distanceToGuard = Vector3.Distance(transform.position, nearbyGuard.transform.position);
                if (distanceToGuard < closestDistance)
                {
                    closestDistance = distanceToGuard;
                    closestGuard = nearbyGuard;
                }
            }
        }

        return closestGuard;
    }
}

