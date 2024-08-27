using HighlightPlus;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public enum EnemyType { Melee, Range }
public enum EnemyState { Patrol, Search, Attack, Stun }
public enum StunType { None, Move, Stun, FixedView, BlockingView, RotateView };

public class Enemy : MonoBehaviour
{
    #region ���� ����
    public EnemyType enemyType; // �� Ÿ��
    public EnemyState enemyState; // �� ����
    public StunType stunType; // ���� ����

    [Header("Waypoint")]
    [SerializeField] protected Transform[] waypoints; // ��������Ʈ �迭
    [SerializeField] protected float waitTime = 2f;   // �� ��������Ʈ���� ����ϴ� �ð�
    [SerializeField] protected float agentSpeed = 3.5f; // ������Ʈ �ӵ�
    protected int currentWaypointIndex; // ���� ��������Ʈ �ε���

    [Header("Components")]
    protected NavMeshAgent agent; // ����޽�
    [SerializeField] public Animator anim; // �ִϸ��̼�
    [SerializeField] public FieldOfView fieldOfView; // �þ�
    [SerializeField] protected GameObject exclamationMark; // �� �߽߰� �Ӹ� ���� �ߴ� ����ǥ ������Ʈ

    [Header("Canvas")]
    [SerializeField] protected Canvas infoCanvas; // ����â ĵ����
    [SerializeField] protected Canvas dialogueCanvas; // ��� ĵ����
    [SerializeField] protected TextMeshProUGUI dialogueText; // ��� �ؽ�Ʈ

    [Header("Dialogue")]
    [SerializeField] protected string[] patrolDialogues; // ���� ��� �迭
    [SerializeField] protected string[] searchDialogues; // Ž�� ��� �迭
    [SerializeField] protected string[] attackDialogues; // ���� ��� �迭
    protected string[] stunDialogues = new string[5]; // ���� ��� �迭

    //���� ����
    protected bool isWaiting; // ��� ������ ����
    protected bool hasPlayedDialogue = false; // ��� ��� ���� ������ ����
    protected bool isAttacking = false; // ���� ������ ����
    protected bool isRange = false; // ���ݹ����� ���� �ִ��� ����
    private bool isSearchingLastPosition = false; // ������ ��ġ�� ���� ������ ����
    public bool isDead; // �׾����� ����
    private bool isTargeting = false;

    public float stunTime;
    public float initialViewRadius; // (���� ������)�ʱ� �þ� ����
    private Vector3 targetPosition;
    public ParticleSystem[] stunEffects;

    //���� ���� ����
    public Transform atkTarget; // ���� Ÿ�� ��ġ ���� ����
    public Transform currTarget;
    public float attackInterval = 1.5f; // ���� ����
    public float attackRange = 5f; // ���� ����
    public float searchRange = 10f; // Ž�� ����


    //RangeType�� ���� ������
    [SerializeField] protected Transform firePos; // �Ѿ��� ���� ��ġ
    [SerializeField] protected GameObject bulletPrefab; // �Ѿ� ������

    private Vector3 lastKnownPosition; // ���������� �÷��̾ ������ ��ġ

    [Header("Corpse Detection")]
    public float corpseDetectionRadius = 10f; // ��ü Ž�� �ݰ�
    public GameObject sakke;
    #endregion

    #region Start
    private void Start()
    {
        fieldOfView = GetComponentInChildren<FieldOfView>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = agentSpeed;

        if (waypoints.Length > 0)
        {
            currentWaypointIndex = 0; //��������Ʈ �ε��� �ʱ�ȭ
        }

        enemyState = EnemyState.Patrol; //EnemyState �ʱ�ȭ(�ʱ� ���� : Patrol)

        if (fieldOfView != null)
        {
            initialViewRadius = fieldOfView.viewRadius; // �þ� ���� �ʱ�ȭ
        }
    }
    #endregion
    void OnDrawGizmos()
    {
        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, agent.steeringTarget);
        }
    }
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

        UpdateUIPosition(); // UI ��ġ ������Ʈ
    }
    #endregion
    private void ChangeSearchState()
    {
        if (fieldOfView.visibleTargets.Count > 0) // �þ߿� ������ Ÿ���� �ϳ� �̻� ���� ���
        {
            foreach (var target in fieldOfView.visibleTargets)
            {
                if (target.CompareTag("Player")) // Ÿ���� �±װ� "Player"�� ���
                {
                    currTarget = target; // Ÿ���� ���� Ÿ������ ����
                    break; // ù ��° �÷��̾ ã���� ������ ����
                }
            }
        }
        if (currTarget != null)
        {
            enemyState = EnemyState.Search; // Ž�� ���·� ��ȯ

            if (exclamationMark != null && !exclamationMark.activeSelf)
            {
                exclamationMark.SetActive(true); // �� �߰� ��ũ Ȱ��ȭ
            }

            if (!fieldOfView.isViewMeshVisible) // �þ� ���� �޽��� ��Ȱ��ȭ�Ǿ��ִٸ�
            {
                fieldOfView.isViewMeshVisible = true; // �þ� ���� �޽� Ȱ��ȭ
            }
        }
    }

    private void ChangeAttackState()
    {
        agent.isStopped = false;
        // ���� ���� ���� �÷��̾� ����
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange); // ���� ��ġ���� attackRange ������ŭ�� ���� ���� �ȿ� �����ϴ� ��� �ݶ��̴� hitColliders�� ����
        isRange = false;
        atkTarget = null;

        foreach (var collider in hitColliders) // hitColliders�� ��� collider ��ȸ
        {
            if (collider.CompareTag("Player")) // collider�� �±װ� "Player"�̸�
            {
                isRange = true; // ���� ���� �ȿ� �÷��̾ ������
                atkTarget = collider.transform; // Ÿ������ ����
                break;
            }
        }

        if (isRange) // ���� ���� �ȿ� �÷��̾ �����ϸ�
        {

            enemyState = EnemyState.Attack; // ���� ���·� ��ȯ

            if (fieldOfView.isViewMeshVisible) // �þ� ���� �޽��� Ȱ��ȭ�Ǿ��ִٸ�
            {
                fieldOfView.isViewMeshVisible = false; // �þ� ���� �޽� ��Ȱ��ȭ
            }
        }
    }

    private void ChangePatrolState() // ���� ���� ��ȯ
    {
        // Ÿ���� ���� ���� �ȿ� ���� �ִ��� üũ
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange); // ���� ��ġ���� attackRange ������ŭ�� ���� ���� �ȿ� �����ϴ� ��� �ݶ��̴� hitColliders�� ����
        bool targetStillInRange = false;

        foreach (var collider in hitColliders) // hitColliders�� ��� collider ��ȸ
        {
            if (collider.CompareTag("Player"))  // collider�� �±װ� "Player"�̸�
            {
                targetStillInRange = true; // Ÿ���� ���� ������
                break;
            }
        }

        if (!targetStillInRange) // Ÿ���� �������� ������
        {
            Debug.Log("���� ��ȯ");
            isTargeting = false;
            enemyState = EnemyState.Search;
        }
    }

    #region ���� ����
    protected virtual void PatrolState()
    {
        if(waypoints != null && waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[currentWaypointIndex].position); // ��������Ʈ�� �̵� ����

            float speed = agent.velocity.magnitude; // ������Ʈ ���ǵ� ����
            anim.SetFloat("MoveSpeed", speed); // �ִϸ��̼� ����

            if (!agent.pathPending && agent.remainingDistance < 0.5f && !isWaiting) //  ������Ʈ�� ��ǥ �������� ���� ��θ� ��� ���� �ƴϰ� / ������Ʈ�� ���� ��ǥ ������ ���� �����ϰ� / ��� ���� �ƴϸ�
            {
                StartCoroutine(WaitAtWaypoint()); //WaitAtWaypoint() ���� (��������Ʈ �������� ���� �ð� ���)
            }
        }
        else
        {
            agent.isStopped = true;
            anim.SetFloat("MoveSpeed", 0);
            RotateSearch();
        }
        
        if (!hasPlayedDialogue) // ��� ��� ���� �ƴϸ�
        {
            StartCoroutine(SetDialogue("Patrol")); // ���� ��� ���
        }
        if (fieldOfView.visibleTargets.Count > 0) // �þ߿� ������ Ÿ���� �ϳ� �̻� ���� ���
        {
            foreach (var target in fieldOfView.visibleTargets)
            {
                if (target.CompareTag("Player")) // Ÿ���� �±װ� "Player"�� ���
                {
                    currTarget = target; // Ÿ���� ���� Ÿ������ ����
                    break; // ù ��° �÷��̾ ã���� ������ ����
                }
            }
        }
        if (currTarget != null)
        {
            agent.isStopped = false;
            enemyState = EnemyState.Search; // Ž�� ���·� ��ȯ

            if (exclamationMark != null && !exclamationMark.activeSelf)
            {
                exclamationMark.SetActive(true); // �� �߰� ��ũ Ȱ��ȭ
            }

            if (!fieldOfView.isViewMeshVisible) // �þ� ���� �޽��� ��Ȱ��ȭ�Ǿ��ִٸ�
            {
                fieldOfView.isViewMeshVisible = true; // �þ� ���� �޽� Ȱ��ȭ
            }
        }
        //ChangeSearchState(); //Ž�� ���� ��ȯ �Լ�

        ChangeAttackState(); //���� ���� ��ȯ �Լ�
    }

    protected virtual IEnumerator WaitAtWaypoint() // ��������Ʈ �������� ���� �ð� ���
    {
        isWaiting = true; // ��� ��
        agent.isStopped = true; // ������Ʈ �̵� ����
        anim.SetFloat("MoveSpeed", 0); // �ִϸ��̼� ���� - Idle ����
        yield return new WaitForSeconds(waitTime); // waitTime��ŭ ���
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh) // NavMeshAgent�� Ȱ��ȭ�Ǿ� �ְ� NavMesh ���� �ִ��� Ȯ��
        {
            agent.isStopped = false; // ������Ʈ �̵� �簳
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length; // ��������Ʈ �ε����� +1�ϰ� ������ �����ڷ� waypoints.Length�� �Ѿ�� �ʵ��� ����
            agent.SetDestination(waypoints[currentWaypointIndex].position); // ������Ʈ�� �̵� ������ ���� �ε����� ��������Ʈ�� ����
            isWaiting = false; // ��� ��
        }
    }
    #endregion

    #region Ž�� ����
    private void SearchState()
    {
        // ���� ���� ���� �÷��̾� ����
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, searchRange);
        isRange = false;
        currTarget = null;

        foreach (var collider in hitColliders) // hitColliders�� ��� collider ��ȸ
        {
            if (collider.CompareTag("Player")) // collider�� �±װ� "Player"�̸�
            {
                isRange = true; // ���� ���� �ȿ� �÷��̾ ������
                currTarget = collider.transform; // Ÿ������ ����
                break;
            }
        }
        if (currTarget != null)
        {
            float speed = agent.velocity.magnitude; // ������Ʈ ���ǵ� ����
            anim.SetFloat("MoveSpeed", speed); // �ִϸ��̼� ����
            agent.SetDestination(currTarget.transform.position);
        }
        else
        {
            enemyState = EnemyState.Patrol;
        }

        if (!hasPlayedDialogue) // ��� ��� ���� �ƴϸ�
        {
            StartCoroutine(SetDialogue("Search")); // Ž�� ��� ���
        }

        ChangeAttackState(); //���� ���� ��ȯ �Լ�
    }
    #endregion
    public void RotateSearch()
    {
        StartCoroutine(RotateSearchRoutine());
    }

    private IEnumerator RotateSearchRoutine()
    {
        // ���� ȸ���� ����
        Quaternion originalRotation = fieldOfView.transform.localRotation;

        float rotationSpeed = 90f;       // �ʴ� ȸ�� �ӵ�
        float maxRotationAngle = 45f;    // �� ���� ȸ���� �ִ� ����
        float waitTime = 2f;             // ȸ�� �� ��� �ð�
        float rotationInterval = 1f;     // ȸ�� ���� �ֱ�

        while (true) // �ڷ�ƾ�� ���������� ����
        {
            // ���� �ð� ���� ȸ��
            float elapsedTime = 0f;
            while (elapsedTime < rotationInterval)
            {
                // PingPong �Լ��� ����Ͽ� -maxRotationAngle���� +maxRotationAngle���� ���� ����
                float rotationAngle = Mathf.PingPong(Time.time * rotationSpeed, maxRotationAngle * 2) - maxRotationAngle;
                fieldOfView.transform.localRotation = Quaternion.Euler(0, rotationAngle, 0);

                elapsedTime += Time.deltaTime; // �ð� ������Ʈ
                yield return null; // ���� �����ӱ��� ���
            }

            // ���� ����
            Quaternion currentRotation = fieldOfView.transform.localRotation;

            // 2�ʰ� ���
            yield return new WaitForSeconds(waitTime);

            // ���� ȸ�������� ���ƿ���
            fieldOfView.transform.localRotation = originalRotation;

            // 2�ʰ� ���
            yield return new WaitForSeconds(waitTime);

            // �ٽ� ������ ������ ȸ�������� ����
            fieldOfView.transform.localRotation = currentRotation;
        }
    }


    #region ���� ����
    protected virtual void AttackState()
    {
        // ���� ���� ���� �÷��̾� ����
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
        isRange = false;
        atkTarget = null;

        foreach (var collider in hitColliders) // hitColliders�� ��� collider ��ȸ
        {
            if (collider.CompareTag("Player")) // collider�� �±װ� "Player"�̸�
            {
                isRange = true; // ���� ���� �ȿ� �÷��̾ ������
                atkTarget = collider.transform; // Ÿ������ ����
                break;
            }
        }

        if (!hasPlayedDialogue) // ��� ��� ���� �ƴϸ�
        {
            StartCoroutine(SetDialogue("Attack")); // ���� ��� ���
        }

        if (!isAttacking && atkTarget != null) // ���� ���� �ƴϰ� Ÿ���� �����ϸ�
        {
            anim.SetFloat("MoveSpeed", 0); // �ִϸ��̼� ���� - Idle ����

            StartCoroutine(RotateDirection()); // �÷��̾��� �������� ȸ��

            StartCoroutine(AttackRoutine()); // ���� �ڷ�ƾ ����

        }
        if (atkTarget == null)
        {
            enemyState = EnemyState.Search;
        }
    }

    private IEnumerator RotateDirection() // �÷��̾��� �������� ȸ��
    {
        if (atkTarget != null)
        {
            Vector3 directionToTarget = atkTarget.position - transform.position;
            directionToTarget.y = 0; // ���� ������ �����Ͽ� ����鿡���� ȸ���ϵ��� ����
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

            // ���� Ÿ�� �������� ȸ���ϵ��� ����
            while (Quaternion.Angle(transform.rotation, targetRotation) > 1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f); // ȸ�� �ӵ� ����
                yield return null;
            }

            // ��Ȯ�� Ÿ�� �������� ȸ�� �Ϸ�
            transform.rotation = targetRotation;
        }
    }
    private IEnumerator AttackRoutine()
    {
        isAttacking = true; // ���� �� ����
        agent.isStopped = true;
        anim.SetTrigger("Attack"); // ���� �ִϸ��̼� ����
        if (enemyType == EnemyType.Range)
        {
            yield return new WaitForSeconds(0.2f);
            GameObject bullet = Instantiate(bulletPrefab, firePos.transform.position, transform.rotation);
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            if (bulletRb != null)
            {
                if (atkTarget != null)
                {
                    // �Ѿ� �߻� ���� ����
                    Vector3 directionToTarget = (atkTarget.position - transform.position).normalized;
                    directionToTarget.y = 0; // y���� 0���� �����Ͽ� �Ѿ��� �������� ���� �ʵ��� ��

                    // �Ѿ� �߻� ���� ���͸� ���
                    Vector3 shootDirection = directionToTarget;

                    // �Ѿ˿� ���� �߰�
                    bulletRb.AddForce(shootDirection * 100f, ForceMode.Impulse); // �߻� �� ����
                }
            }
        }

        yield return new WaitForSeconds(attackInterval);
        //NinjaController targetNinja = atkTarget.GetComponent<NinjaController>();
        //targetNinja.OnDamage();
        isAttacking = false; // ���� �� ���� ����
        agent.isStopped = false; // ������Ʈ �̵� �簳
    }
    #endregion

    #region ����
    private EnemyState curState;
    private float stunTimer = 0f;

    public void ChangeStunState(StunType _stunType, float _stunTime, Vector3 _targetPosition = default) // ���� ���� ��ȯ
    {
        if (enemyState == EnemyState.Stun)
        {
            curState = EnemyState.Patrol;
        }
        else
        {
            curState = enemyState; //���� ���� ����
        }
        stunType = _stunType;
        stunTime = _stunTime;
        targetPosition = _targetPosition; // Move Ÿ���� �� ����� ��ġ
        stunTimer = 0f; // Ÿ�̸� �ʱ�ȭ
        enemyState = EnemyState.Stun;
    }
    public void StunState()
    {
        stunTimer += Time.deltaTime;

        if (stunTimer >= stunTime)
        {
            EndStunState();
        }
        StartCoroutine(SetDialogue("Stun"));

        switch (stunType)
        {
            case StunType.Move: // Ư�� ��ҷ� �̵��ϵ���
                Move(targetPosition);
                break;
            case StunType.Stun: // ����, ���߰� �þ� ����
                Stun();
                break;
            case StunType.FixedView: // ����, ���߰� �þ߰��� - ��Ȥ
                FixedView(targetPosition);
                break;
            case StunType.BlockingView: // ���� �� �þ� ���� ��� - ��ä��и�(�繫���̿��Ե� ����)
                BlockingView();
                break;
            case StunType.RotateView: // ����, �þ� ������ - ��������
                RotateView(targetPosition);
                break;
        }
    }
    private void EndStunState()
    {
        Debug.Log("��, curState :" + curState);
        stunTimer = 0f; //Ÿ�̸� �ʱ�ȭ

        // ������ ������ ���� ���� ����
        fieldOfView.viewRadius = initialViewRadius;
        anim.SetInteger("StunID", 0);
        foreach (var effect in stunEffects)
        {
            if (effect != null) effect.gameObject.SetActive(false);
        }
        agent.isStopped = false;

        stunType = StunType.None;
        enemyState = curState; // ����� ���� ���·� ���ư�
    }
    //StunID(���� �ִϸ��̼�) : 0(None), 1(Stun), 2(BlockingView), 3(FixedView), 4(RotateView)
    private void Move(Vector3 targetPostion) // Ư�� ��ҷ� �̵��ϵ���
    {
        stunEffects[0].gameObject.SetActive(true);
        float speed = agent.velocity.magnitude; // ������Ʈ ���ǵ� ����
        anim.SetFloat("MoveSpeed", speed); // �ִϸ��̼� ����
        agent.SetDestination(targetPostion);
    }

    private void Stun() // ����, ���߰� �þ� ����
    {
        stunEffects[1].gameObject.SetActive(true);
        agent.isStopped = true;
        anim.SetFloat("MoveSpeed", 0);
        anim.SetInteger("StunID", 1);
        fieldOfView.viewRadius = 0f;
    }
    private void BlockingView() // ���� �� �þ� ���� ��� - ��ä��и�(�繫���̿��Ե� ����)
    {
        stunEffects[2].gameObject.SetActive(true);
        agent.isStopped = true;
        anim.SetFloat("MoveSpeed", 0);
        anim.SetInteger("StunID", 2);
        fieldOfView.viewRadius = 0f;
    }

    private void FixedView(Vector3 targetPostion) // ����, ���߰� �þ߰��� - ��Ȥ
    {
        stunEffects[3].gameObject.SetActive(true);
        agent.isStopped = true;
        anim.SetFloat("MoveSpeed", 0);
        anim.SetInteger("StunID", 3);

        // Ÿ�� �������� ȸ��
        Vector3 direction = (targetPostion - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
    }


    private void RotateView(Vector3 targetPostion) // ����, �þ� ������ - ��������
    {
        stunEffects[4].gameObject.SetActive(true);
        // Ÿ�� �������� ȸ��
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
        //stunEffects[5].gameObject.SetActive(true);
        // Ÿ�� �������� ȸ��
        Vector3 direction = (targetPostion - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        
        agent.isStopped = true;
        anim.SetFloat("MoveSpeed", 0);
        anim.SetInteger("StunID", 5);
    }

    public void PlayWithKuma(Vector3 targetPostion)
    {
        stunEffects[6].gameObject.SetActive(true);
        // Ÿ�� �������� ȸ��
        Vector3 direction = (targetPostion - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);

        agent.isStopped = true;
        anim.SetFloat("MoveSpeed", 0);
        anim.SetInteger("StunID", 6);

    }

    #endregion

    #region ���
    public void Die()
    {
        if (!isDead)
        {
            for (int i = 0; i < stunEffects.Length; i++)
            {
                stunEffects[i].gameObject.SetActive(false);
            }
            HighlightEffect highlightEffect = GetComponent<HighlightEffect>();
            highlightEffect.highlighted = false;
            dialogueCanvas.gameObject.SetActive(false);
            isDead = true;
            agent.isStopped = true;
            agent.enabled = false;
            anim.SetInteger("StunID", 0);
            anim.SetTrigger("Death");
            gameObject.tag = "Corpse";
            fieldOfView.viewMeshGroup.SetActive(false);
            // ��Ÿ ��� �ʿ��� ������Ʈ ��Ȱ��ȭ
            fieldOfView.enabled = false; // �þ� ������Ʈ ��Ȱ��ȭ
            this.enabled = false; // �� ��ũ��Ʈ ��ü ��Ȱ��ȭ
        }
    }

    #endregion

    #region ��� ���
    public virtual IEnumerator SetDialogue(string dialogueType)
    {
        switch (dialogueType)
        {
            case "Patrol":
                // �������� ��� ����
                if (patrolDialogues.Length > 0)
                {
                    int randomIndex = Random.Range(0, patrolDialogues.Length);
                    dialogueText.text = patrolDialogues[randomIndex];
                }
                else
                {
                    dialogueText.text = "���� ħ������ �ʴ��� �� ��������!"; // �⺻ ���
                }
                break;
            case "Search":
                // �������� ��� ����
                if (searchDialogues.Length > 0)
                {
                    int randomIndex = Random.Range(0, searchDialogues.Length);
                    dialogueText.text = searchDialogues[randomIndex];
                }
                else
                {
                    dialogueText.text = "ħ���ڴ�!!"; // �⺻ ���
                }
                break;
            case "Attack":
                // �������� ��� ����
                if (attackDialogues.Length > 0)
                {
                    int randomIndex = Random.Range(0, attackDialogues.Length);
                    dialogueText.text = attackDialogues[randomIndex];
                }
                else
                {
                    dialogueText.text = "�׾��! ����"; // �⺻ ���
                }
                break;
            case "Stun":
                if (stunDialogues.Length > 0)
                {
                    switch(stunType)
                    {
                        case StunType.Move:
                            dialogueText.text = "�� ������ ������.";
                            break;
                        case StunType.Stun:
                            dialogueText.text = "��.. ������ �������ھ�";
                            break;
                        case StunType.FixedView:
                            dialogueText.text = "�� ó�� ���� �ŷ��ִ°�.";
                            break;
                        case StunType.BlockingView:
                            dialogueText.text = "����! ��ä�Ⱑ ��� ���ͼ� ���� �� ���� ����.";
                            break;
                        case StunType.RotateView:
                            dialogueText.text = "�̰� ���� �Ҹ���?";
                            break;
                    }
                }
                break;
        }

        hasPlayedDialogue = true; // ��簡 �����Ǿ����� ���
        yield return new WaitForSeconds(3f);
        hasPlayedDialogue = false;
    }
    #endregion

    #region UI
    private void OnMouseDown() // ���콺 Ŭ������ �� �þ߰� ǥ��
    {
        fieldOfView.isViewMeshVisible = !fieldOfView.isViewMeshVisible;
    }

    private void OnMouseOver() // ���콺�� �����ٴ��� �� ����â ǥ��
    {
        infoCanvas.gameObject.SetActive(true);
    }

    private void OnMouseExit() // ���콺�� ���� �� ����â �����
    {
        infoCanvas.gameObject.SetActive(false);
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red; // ���� ���� ����� ���� ����
    //    Gizmos.DrawWireSphere(transform.position, attackRange); // ���� ���� �ð�ȭ
    //}

    private void UpdateUIPosition()  //UI ��ġ ������Ʈ
    {
        if (infoCanvas != null)
        {
            // UI ĵ������ ���� �Ӹ� ���� �̵�
            Vector3 headPosition = transform.position + Vector3.up * 4f; // �Ӹ� ���� ��ġ�� ����
            infoCanvas.transform.position = headPosition;
            dialogueCanvas.transform.position = headPosition;

            // ī�޶���� �Ÿ� �� �þ߿� ���� UI�� �׻� ī�޶� ���ϵ��� ����
            infoCanvas.transform.forward = Camera.main.transform.forward;
            dialogueCanvas.transform.forward = Camera.main.transform.forward;
        }
    }
    #endregion

    #region Ÿ�� ����
    public void SetTarget(Transform targetPosition)
    {
        if (enemyState != EnemyState.Attack) // ���� ���°� �ƴϸ� 
        {
            atkTarget = targetPosition;
            enemyState = EnemyState.Search; // Ž�� ���·� ����
            agent.SetDestination(atkTarget.position); // Ÿ�� ��ġ�� �̵�
        }
    }
    #endregion

    #region ��ü Ž��
    private void SearchForCorpses()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, corpseDetectionRadius); // ��ü Ž�� �ݰ� �� ��� �ݶ��̴� ����
        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("Corpse")) // collider�� �±װ� "Corpse"�̸�
            {
                Debug.Log("��ü �߰�");
                lastKnownPosition = collider.transform.position; // ��ü�� ��ġ�� ���������� �˷��� ��ġ�� ����
                agent.SetDestination(lastKnownPosition); // ��ü �������� �̵�
                isSearchingLastPosition = false; // ������ ��ġ ���� �÷��� ����
                RotateSearch(); // ��ü ���� Ž�� ����
                return; // ��ü�� �߰��ϸ� �� �̻� �˻����� ����
            }
        }
    }
    #endregion
}
