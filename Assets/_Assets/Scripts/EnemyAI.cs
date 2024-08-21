using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform[] waypoints; // 웨이포인트 배열
    public float waitTime = 2f;   // 각 웨이포인트에서 대기하는 시간
    public float agentSpeed = 3.5f; // 에이전트 속도

    private NavMeshAgent agent;
    private int currentWaypointIndex;
    private bool waiting;
    [SerializeField] private Animator anim;
    [SerializeField] private FieldOfView fieldOfView;
    [SerializeField] private GameObject mark;
    [SerializeField] private Canvas textCanvas;
    [SerializeField] private Canvas enemyCanvas;
    [SerializeField] private TextMeshProUGUI enemyText;

    public float attackInterval = 1.5f; // 공격 간격
    private bool isAttacking = false; // 공격 상태를 추적하는 플래그

    void Start()
    {
        fieldOfView = GetComponentInChildren<FieldOfView>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = agentSpeed;

        if (waypoints.Length > 0)
        {
            currentWaypointIndex = 0;
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    void Update()
    {
        if (!isAttacking)
        {
            if (fieldOfView.isTarget())
            {
                mark.SetActive(true);
                textCanvas.gameObject.SetActive(true);
                enemyText.text = "침입자다!!";
                //visibleTargets의 첫 번째 타겟 위치로 이동
                if (fieldOfView.visibleTargets.Count > 0)
                {
                    Transform target = fieldOfView.visibleTargets[0];
                    agent.SetDestination(target.position);
                }
            }
            else
            {
                textCanvas.gameObject.SetActive(false);
                mark.SetActive(false);
                // 에이전트가 현재 웨이포인트에 도달했는지 확인
                if (!agent.pathPending && agent.remainingDistance < 0.5f && !waiting)
                {
                    StartCoroutine(WaitAtWaypoint());
                }

                // 에이전트의 속도에 따라 애니메이터 파라미터를 업데이트
                float speed = agent.velocity.magnitude;
                anim.SetFloat("MoveSpeed", speed);
            }
        }
    }

    // 웨이포인트에서 대기 후 다음 웨이포인트로 이동
    IEnumerator WaitAtWaypoint()
    {
        waiting = true;
        agent.isStopped = true; // 대기 중일 때 에이전트 정지
        anim.SetFloat("MoveSpeed", 0); // 대기 중일 때 애니메이션 정지
        yield return new WaitForSeconds(waitTime);

        agent.isStopped = false; // 이동 재개
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        agent.SetDestination(waypoints[currentWaypointIndex].position);
        waiting = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isAttacking)
        {
            StartCoroutine(AttackRoutine(other.transform));
        }
    }
    // 트리거를 벗어났을 때 호출되는 메서드
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<NinjaController>().OnDamage();
            print("공격");
            StopCoroutine(AttackRoutine(other.transform));
            isAttacking = false; // 공격 상태 초기화
        }
    }

    // 공격을 관리하는 코루틴
    private IEnumerator AttackRoutine(Transform player)
    {
        isAttacking = true;
        while (isAttacking)
        {
            // 공격 애니메이션 실행
            anim.SetTrigger("Attack");

            // 공격 애니메이션의 길이 또는 공격 딜레이를 고려한 대기 시간
            yield return new WaitForSeconds(attackInterval);
        }
    }
    private void OnMouseDown()
    {
        Debug.Log("클릭");
        fieldOfView.isViewMeshVisible = !fieldOfView.isViewMeshVisible;
    }
    private void OnMouseOver()
    {
        enemyCanvas.gameObject.SetActive(true);
    }
    private void OnMouseExit()
    {
        enemyCanvas.gameObject.SetActive(false);
    }
    public void Die()
    {
        print("적이 맞아 죽음");
        Destroy(gameObject);
    }
    public void Alarm()
    {
        print("경계태세(소리들음)");
    }
}