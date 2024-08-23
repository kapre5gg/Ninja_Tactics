using System.Collections;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public enum EnemyType { Melee, Range }
public enum EnemyState { Patrol, Search, Attack, Stun }
public enum StunType { None, Move, Stun, FixedView, BlockingView, RotateView };

public class Enemy : MonoBehaviour
{
    #region 변수 선언
    public EnemyType enemyType; // 적 타입
    public EnemyState enemyState; // 적 상태
    public StunType stunType; // 교란 종류

    [Header("Waypoint")]
    [SerializeField] protected Transform[] waypoints; // 웨이포인트 배열
    [SerializeField] protected float waitTime = 2f;   // 각 웨이포인트에서 대기하는 시간
    [SerializeField] protected float agentSpeed = 3.5f; // 에이전트 속도
    protected int currentWaypointIndex; // 현재 웨이포인트 인덱스

    [Header("Components")]
    protected NavMeshAgent agent; // 내비메쉬
    [SerializeField] protected Animator anim; // 애니메이션
    [SerializeField] protected FieldOfView fieldOfView; // 시야
    [SerializeField] protected GameObject exclamationMark; // 적 발견시 머리 위에 뜨는 느낌표 오브젝트

    [Header("Canvas")]
    [SerializeField] protected Canvas infoCanvas; // 정보창 캔버스
    [SerializeField] protected Canvas dialogueCanvas; // 대사 캔버스
    [SerializeField] protected TextMeshProUGUI dialogueText; // 대사 텍스트

    [Header("Dialogue")]
    [SerializeField] protected string[] patrolDialogues; // 순찰 대사 배열
    [SerializeField] protected string[] searchDialogues; // 탐색 대사 배열
    [SerializeField] protected string[] attackDialogues; // 공격 대사 배열

    //상태 변수
    protected bool isWaiting; // 대기 중인지 여부
    protected bool hasPlayedDialogue = false; // 대사 출력 진행 중인지 여부
    protected bool isAttacking = false; // 공격 중인지 여부
    protected bool isRange = false; // 공격범위에 적이 있는지 여부
    private bool isSearchingLastPosition = false; // 마지막 위치를 수색 중인지 여부
    public bool isDead; // 죽었는지 여부
    private bool isTargeting = false;

    public float stunTime;
    public float initialViewRadius; // (스턴 로직용)초기 시야 범위
    private Vector3 targetPosition;

    //공격 관련 변수
    public Transform atkTarget; // 현재 타겟 위치 저장 변수
    public Transform currTarget;
    public float attackInterval = 1.5f; // 공격 간격
    public float attackRange = 5f; // 공격 범위

    
    //RangeType을 위한 변수들
    [SerializeField] protected Transform firePos; // 총알이 나갈 위치
    [SerializeField] protected GameObject bulletPrefab; // 총알 프리팹

    private Vector3 lastKnownPosition; // 마지막으로 플레이어가 감지된 위치

    [Header("Corpse Detection")]
    public float corpseDetectionRadius = 10f; // 시체 탐지 반경
    #endregion

    #region Start
    private void Start()
    {
        fieldOfView = GetComponentInChildren<FieldOfView>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = agentSpeed;

        if (waypoints.Length > 0)
        {
            currentWaypointIndex = 0; //웨이포인트 인덱스 초기화
        }

        enemyState = EnemyState.Patrol; //EnemyState 초기화(초기 설정 : Patrol)

        if(fieldOfView != null)
        {
            initialViewRadius = fieldOfView.viewRadius; // 시야 설정 초기화
        }
    }
    #endregion

    #region Update
    private void Update()
    {
        if (!isDead)
        {
            switch (enemyState)
            {
                case EnemyState.Patrol:
                    PatrolState();
                    break;
                case EnemyState.Attack:
                    AttackState();
                    break;
                case EnemyState.Search:
                    SearchState();
                    break;
                case EnemyState.Stun:
                    StunState();
                    break;
            }
        }

        UpdateUIPosition(); // UI 위치 업데이트
    }
    #endregion
    private void ChangeSearchState()
    {
        if (fieldOfView.visibleTargets.Count > 0) // 시야에 감지된 타겟이 하나 이상 있을 경우
        {
            Debug.Log("탐색 전환");
            enemyState = EnemyState.Search; // 탐색 상태로 전환

            if (!exclamationMark)
                exclamationMark.SetActive(true); // 적 발견 마크 활성화

            if (!fieldOfView.isViewMeshVisible) // 시야 범위 메쉬가 비활성화되어있다면
            {
                fieldOfView.isViewMeshVisible = true; // 시야 범위 메쉬 활성화
            }
        }
    }
    private void ChangeAttackState()
    {
        // 공격 범위 내의 플레이어 감지
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange); // 본인 위치에서 attackRange 범위만큼의 원형 범위 안에 존재하는 모든 콜라이더 hitColliders에 저장
        isRange = false;
        atkTarget = null;

        foreach (var collider in hitColliders) // hitColliders의 모든 collider 순회
        {
            if (collider.CompareTag("Player")) // collider의 태그가 "Player"이면
            {
                isRange = true; // 공격 범위 안에 플레이어가 존재함
                atkTarget = collider.transform; // 타겟으로 설정
                break;
            }
        }

        if (isRange) // 공격 범위 안에 플레이어가 존재하면
        {
            Debug.Log("공격 전환");

            enemyState = EnemyState.Attack; // 공격 상태로 전환

            if (fieldOfView.isViewMeshVisible) // 시야 범위 메쉬가 활성화되어있다면
            {
                fieldOfView.isViewMeshVisible = false; // 시야 범위 메쉬 비활성화
            }
        }
    }

    private void ChangePatrolState() // 순찰 상태 전환
    {
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
            Debug.Log("순찰 전환");
            isTargeting = false;
            enemyState = EnemyState.Search;
        }
    }

    #region 순찰 상태
    protected virtual void PatrolState()
    {
        agent.SetDestination(waypoints[currentWaypointIndex].position); // 웨이포인트로 이동 시작

        float speed = agent.velocity.magnitude; // 에이전트 스피드 설정
        anim.SetFloat("MoveSpeed", speed); // 애니메이션 설정

        if (!agent.pathPending && agent.remainingDistance < 0.5f && !isWaiting) //  에이전트가 목표 지점으로 가는 경로를 계산 중이 아니고 / 에이전트가 현재 목표 지점에 거의 도달하고 / 대기 중이 아니면
        {
            StartCoroutine(WaitAtWaypoint()); //WaitAtWaypoint() 실행 (웨이포인트 지점에서 일정 시간 대기)
        }

        if (!hasPlayedDialogue) // 대사 출력 중이 아니면
        {
            StartCoroutine(SetDialogue("Patrol")); // 순찰 대사 출력
        }

        ChangeSearchState(); //탐색 상태 전환 함수

        ChangeAttackState(); //공격 상태 전환 함수
    }

    protected virtual IEnumerator WaitAtWaypoint() // 웨이포인트 지점에서 일정 시간 대기
    {
        isWaiting = true; // 대기 중
        agent.isStopped = true; // 에이전트 이동 중지
        anim.SetFloat("MoveSpeed", 0); // 애니메이션 설정 - Idle 실행
        yield return new WaitForSeconds(waitTime); // waitTime만큼 대기
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh) // NavMeshAgent가 활성화되어 있고 NavMesh 위에 있는지 확인
        {
            agent.isStopped = false; // 에이전트 이동 재개
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length; // 웨이포인트 인덱스를 +1하고 나머지 연산자로 waypoints.Length를 넘어가지 않도록 설정
            agent.SetDestination(waypoints[currentWaypointIndex].position); // 에이전트의 이동 방향을 현재 인덱스의 웨이포인트로 설정
            isWaiting = false; // 대기 끝
        }   
    }
    #endregion

    #region 탐색 상태
    private void SearchState()
    {
        if (!hasPlayedDialogue) // 대사 출력 중이 아니면
        {
            StartCoroutine(SetDialogue("Search")); // 탐색 대사 출력
        }

        float speed = agent.velocity.magnitude; // 에이전트 스피드 설정
        anim.SetFloat("MoveSpeed", speed); // 애니메이션 설정

        if (fieldOfView.visibleTargets.Count > 0) // 시야에 감지된 타겟이 하나 이상 있을 경우
        {
            //isTargeting = true;
            currTarget = fieldOfView.visibleTargets[0]; // 첫번째로 감지된 타겟을 타겟으로 설정
            lastKnownPosition = currTarget.position; // 타겟의 현재 위치를 저장
            agent.SetDestination(currTarget.position); // 타겟 방향으로 이동
        }
        else
        {
            enemyState = EnemyState.Patrol;
        }

        ChangeAttackState(); //공격 상태 전환 함수
    }
    #endregion
    public void RotateSearch()
    {
        StartCoroutine(RotateSearchRoutine());

        //======================
        // 1. 회전값을 기억하고 싶어 
        // 2. 어떤 순간에 회전 값을 기억해야 하지?
        // 3. 각도를 알고 싶은데, rotationAngle 값을 저장하면된다.
        // 4. 어떤 순간에 rotationAngle 값을 저장하고, 필요할때 호출
        //======================
    }

    private IEnumerator RotateSearchRoutine()
    {
        // 현재 회전값 저장
        Quaternion originalRotation = fieldOfView.transform.localRotation;

        float rotationSpeed = 90f;
        float maxRotationAngle = 90f;

        // 일정 시간 동안 회전
        float rotationAngle = Mathf.PingPong(Time.time * rotationSpeed, maxRotationAngle * 2) - maxRotationAngle;
        fieldOfView.transform.localRotation = Quaternion.Euler(0, rotationAngle, 0);

        // 5초간 대기 (회전 유지)
        yield return new WaitForSeconds(5f);

        // 원래 회전값으로 돌아오기
        fieldOfView.transform.localRotation = originalRotation;
    }
    #region 공격 상태
    protected virtual void AttackState()
    {
        if (!hasPlayedDialogue) // 대사 출력 중이 아니면
        {
            StartCoroutine(SetDialogue("Attack")); // 공격 대사 출력
        }

        if (!isAttacking && atkTarget != null) // 공격 중이 아니고 타겟이 존재하면
        {
            anim.SetFloat("MoveSpeed", 0); // 애니메이션 설정 - Idle 실행

            StartCoroutine(RotateDirection()); // 플레이어의 방향으로 회전

            StartCoroutine(AttackRoutine()); // 공격 코루틴 실행

        }

        ChangePatrolState(); // 순찰 상태 전환
    }
    
    private IEnumerator RotateDirection() // 플레이어의 방향으로 회전
    {
        if (atkTarget != null)
        {
            Vector3 directionToTarget = atkTarget.position - transform.position;
            directionToTarget.y = 0; // 수직 방향은 무시하여 수평면에서만 회전하도록 설정
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

            // 적이 타겟 방향으로 회전하도록 보간
            while (Quaternion.Angle(transform.rotation, targetRotation) > 1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f); // 회전 속도 조정
                yield return null;
            }

            // 정확히 타겟 방향으로 회전 완료
            transform.rotation = targetRotation;
        }
    }
    private IEnumerator AttackRoutine()
    {
        isAttacking = true; // 공격 중 설정
        agent.isStopped = true;
        anim.SetTrigger("Attack"); // 공격 애니메이션 실행
        Debug.Log("공격");
        if (enemyType == EnemyType.Range)
        {
            yield return new WaitForSeconds(0.2f);
            GameObject bullet = Instantiate(bulletPrefab, firePos.transform.position, transform.rotation);
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            if (bulletRb != null)
            {
                if (atkTarget != null)
                {
                    // 총알 발사 방향 설정
                    Vector3 directionToTarget = (atkTarget.position - transform.position).normalized;
                    directionToTarget.y = 0; // y축은 0으로 설정하여 총알이 공중으로 솟지 않도록 함

                    // 총알 발사 방향 벡터를 계산
                    Vector3 shootDirection = directionToTarget;

                    // 총알에 힘을 추가
                    bulletRb.AddForce(shootDirection * 100f, ForceMode.Impulse); // 발사 힘 조정
                }
            }
        }

        yield return new WaitForSeconds(attackInterval);

        isAttacking = false; // 공격 중 상태 해제
        agent.isStopped = false; // 에이전트 이동 재개
    }
    #endregion

    #region 스턴
    private EnemyState curState;
    private float stunTimer = 0f;

    public void ChangeStunState(StunType _stunType, float _stunTime, Vector3 _targetPosition = default) // 스턴 상태 전환
    {
        if(enemyState == EnemyState.Stun)
        {
            curState = EnemyState.Patrol;
        }
        else
        {
            curState = enemyState; //직전 상태 저장
        }
        stunType = _stunType;
        stunTime = _stunTime;
        targetPosition = _targetPosition; // Move 타입일 때 사용할 위치
        stunTimer = 0f; // 타이머 초기화
        enemyState = EnemyState.Stun;
    }
    public void StunState()
    {
        stunTimer += Time.deltaTime;

        if (stunTimer >= stunTime)
        {
            EndStunState();
        }

        switch (stunType)
        {
            case StunType.Move: // 특정 장소로 이동하도록
                Move(targetPosition);
                break;
            case StunType.Stun: // 단일, 멈추고 시야 차단
                Stun();
                break;
            case StunType.FixedView: // 단일, 멈추고 시야고정 - 유혹
                FixedView(targetPosition);
                break;
            case StunType.BlockingView: // 범위 내 시야 범위 축소 - 재채기분말(사무라이에게도 통함)
                BlockingView();
                break;
            case StunType.RotateView: // 범위, 시야 돌리기 - 돌던지기
                RotateView(targetPosition);
                break;
        }
    }
    private void EndStunState()
    {
        Debug.Log("끝, curState :" + curState);
        stunTimer = 0f; //타이머 초기화

        // 스턴이 끝났을 때의 상태 복원
        fieldOfView.viewRadius = initialViewRadius;
        anim.SetInteger("StunID", 0);
        agent.isStopped = false;
        
        stunType = StunType.None;
        enemyState = curState; // 저장된 직전 상태로 돌아감
    }
    //StunID(스턴 애니메이션) : 0(None), 1(Stun), 2(BlockingView), 3(FixedView), 4(RotateView)
    private void Move(Vector3 targetPostion) // 특정 장소로 이동하도록
    {
        float speed = agent.velocity.magnitude; // 에이전트 스피드 설정
        anim.SetFloat("MoveSpeed", speed); // 애니메이션 설정
        agent.SetDestination(targetPostion);
    }

    private void Stun() // 단일, 멈추고 시야 차단
    {
        agent.isStopped = true;
        anim.SetFloat("MoveSpeed", 0);
        anim.SetInteger("StunID", 1);
        fieldOfView.viewRadius = 0f;
    }
    private void BlockingView() // 범위 내 시야 범위 축소 - 재채기분말(사무라이에게도 통함)
    {
        agent.isStopped = true;
        anim.SetFloat("MoveSpeed", 0);
        anim.SetInteger("StunID", 2);
        fieldOfView.viewRadius = 0f;
    }

    private void FixedView(Vector3 targetPostion) // 단일, 멈추고 시야고정 - 유혹
    {
        agent.isStopped = true;
        anim.SetFloat("MoveSpeed", 0);
        anim.SetInteger("StunID", 3);

        // 타겟 방향으로 회전
        Vector3 direction = (targetPostion - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
    }

    
    private void RotateView(Vector3 targetPostion) // 범위, 시야 돌리기 - 돌던지기
    {
        // 타겟 방향으로 회전
        Vector3 direction = (targetPostion - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);

        agent.isStopped = true;
        anim.SetFloat("MoveSpeed", 0);
        anim.SetInteger("StunID", 4);

        RotateSearchRoutine();
    }

    public void DrinkSakke(Vector3 targetPostion)
    {
        // 타겟 방향으로 회전
        Vector3 direction = (targetPostion - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);

        agent.isStopped = true;
        anim.SetFloat("MoveSpeed", 0);
        anim.SetInteger("StunID", 5);
    }

    public void PlayWithKuma(Vector3 targetPostion)
    {
        // 타겟 방향으로 회전
        Vector3 direction = (targetPostion - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);

        agent.isStopped = true;
        anim.SetFloat("MoveSpeed", 0);
        anim.SetInteger("StunID", 6);

    }
    #endregion

    #region 사망
    public void Die()
    {
        if (!isDead)
        {
            dialogueCanvas.gameObject.SetActive(false);
            isDead = true;
            agent.isStopped = true;
            agent.enabled = false;
            anim.SetTrigger("Death");
            gameObject.tag = "Corpse";
            if (fieldOfView.isViewMeshVisible) // 시야 범위 메쉬가 활성화되어있다면
            {
                fieldOfView.isViewMeshVisible = false; // 시야 범위 메쉬 비활성화
            }
            // 기타 모든 필요한 컴포넌트 비활성화
            fieldOfView.enabled = false; // 시야 컴포넌트 비활성화
            this.enabled = false; // 적 스크립트 자체 비활성화
        }
    }
    #endregion

    #region 대사 출력
        public virtual IEnumerator SetDialogue(string dialogueType)
        {
            switch(dialogueType)
            {
                case "Patrol":
                    // 랜덤으로 대사 선택
                    if (patrolDialogues.Length > 0)
                    {
                        int randomIndex = Random.Range(0, patrolDialogues.Length);
                        dialogueText.text = patrolDialogues[randomIndex];
                    }
                    else
                    {
                        dialogueText.text = "적이 침투하지 않는지 잘 감시하자!"; // 기본 대사
                    }
                    break;
                case "Search":
                    // 랜덤으로 대사 선택
                    if (searchDialogues.Length > 0)
                    {
                        int randomIndex = Random.Range(0, searchDialogues.Length);
                        dialogueText.text = searchDialogues[randomIndex];
                    }
                    else
                    {
                        dialogueText.text = "침입자다!!"; // 기본 대사
                    }
                    break;
                case "Attack":
                    // 랜덤으로 대사 선택
                    if (attackDialogues.Length > 0)
                    {
                        int randomIndex = Random.Range(0, attackDialogues.Length);
                        dialogueText.text = attackDialogues[randomIndex];
                    }
                    else
                    {
                        dialogueText.text = "죽어랏! 켈켈"; // 기본 대사
                    }
                    break;
            }
        
            hasPlayedDialogue = true; // 대사가 설정되었음을 기록
            yield return new WaitForSeconds(3f);
            hasPlayedDialogue = false;
        }
        #endregion
    
    #region UI
    private void OnMouseDown() // 마우스 클릭했을 때 시야각 표시
    {
        Debug.Log("클릭");
        fieldOfView.isViewMeshVisible = !fieldOfView.isViewMeshVisible;
    }

    private void OnMouseOver() // 마우스를 가져다댔을 때 정보창 표시
    {
        infoCanvas.gameObject.SetActive(true);
    }

    private void OnMouseExit() // 마우스를 뺐을 때 정보창 사라짐
    {
        infoCanvas.gameObject.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red; // 공격 범위 기즈모 색상 설정
        Gizmos.DrawWireSphere(transform.position, attackRange); // 공격 범위 시각화
    }

    private void UpdateUIPosition()  //UI 위치 업데이트
    {
        if (infoCanvas != null)
        {
            // UI 캔버스를 적의 머리 위로 이동
            Vector3 headPosition = transform.position + Vector3.up * 4f; // 머리 위의 위치를 조정
            infoCanvas.transform.position = headPosition;
            dialogueCanvas.transform.position = headPosition;

            // 카메라와의 거리 및 시야에 따라 UI가 항상 카메라를 향하도록 설정
            infoCanvas.transform.forward = Camera.main.transform.forward;
            dialogueCanvas.transform.forward = Camera.main.transform.forward;
        }
    }
    #endregion

    #region 타겟 설정
    public void SetTarget(Transform targetPosition)
    {
        if (enemyState != EnemyState.Attack) // 공격 상태가 아니면 
        {
            atkTarget = targetPosition;
            enemyState = EnemyState.Search; // 탐색 상태로 변경
            agent.SetDestination(atkTarget.position); // 타겟 위치로 이동
        }
    }
    #endregion

    #region 시체 탐지
    private void SearchForCorpses()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, corpseDetectionRadius); // 시체 탐지 반경 내 모든 콜라이더 감지
        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("Corpse")) // collider의 태그가 "Corpse"이면
            {
                Debug.Log("시체 발견");
                lastKnownPosition = collider.transform.position; // 시체의 위치를 마지막으로 알려진 위치로 설정
                agent.SetDestination(lastKnownPosition); // 시체 방향으로 이동
                isSearchingLastPosition = false; // 마지막 위치 수색 플래그 해제
                RotateSearch(); // 시체 주위 탐색 시작
                return; // 시체를 발견하면 더 이상 검색하지 않음
            }
        }
    }
    #endregion
    
}
