using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform[] waypoints; // ��������Ʈ �迭
    public float waitTime = 2f;   // �� ��������Ʈ���� ����ϴ� �ð�
    public float agentSpeed = 3.5f; // ������Ʈ �ӵ�

    private NavMeshAgent agent;
    private int currentWaypointIndex;
    private bool waiting;
    [SerializeField] private Animator anim;
    [SerializeField] private FieldOfView fieldOfView;
    [SerializeField] private GameObject mark;
    [SerializeField] private Canvas textCanvas;
    [SerializeField] private Canvas enemyCanvas;
    [SerializeField] private TextMeshProUGUI enemyText;

    public float attackInterval = 1.5f; // ���� ����
    private bool isAttacking = false; // ���� ���¸� �����ϴ� �÷���

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
                enemyText.text = "ħ���ڴ�!!";
                //visibleTargets�� ù ��° Ÿ�� ��ġ�� �̵�
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
                // ������Ʈ�� ���� ��������Ʈ�� �����ߴ��� Ȯ��
                if (!agent.pathPending && agent.remainingDistance < 0.5f && !waiting)
                {
                    StartCoroutine(WaitAtWaypoint());
                }

                // ������Ʈ�� �ӵ��� ���� �ִϸ����� �Ķ���͸� ������Ʈ
                float speed = agent.velocity.magnitude;
                anim.SetFloat("MoveSpeed", speed);
            }
        }
    }

    // ��������Ʈ���� ��� �� ���� ��������Ʈ�� �̵�
    IEnumerator WaitAtWaypoint()
    {
        waiting = true;
        agent.isStopped = true; // ��� ���� �� ������Ʈ ����
        anim.SetFloat("MoveSpeed", 0); // ��� ���� �� �ִϸ��̼� ����
        yield return new WaitForSeconds(waitTime);

        agent.isStopped = false; // �̵� �簳
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
    // Ʈ���Ÿ� ����� �� ȣ��Ǵ� �޼���
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<NinjaController>().OnDamage();
            print("����");
            StopCoroutine(AttackRoutine(other.transform));
            isAttacking = false; // ���� ���� �ʱ�ȭ
        }
    }

    // ������ �����ϴ� �ڷ�ƾ
    private IEnumerator AttackRoutine(Transform player)
    {
        isAttacking = true;
        while (isAttacking)
        {
            // ���� �ִϸ��̼� ����
            anim.SetTrigger("Attack");

            // ���� �ִϸ��̼��� ���� �Ǵ� ���� �����̸� ����� ��� �ð�
            yield return new WaitForSeconds(attackInterval);
        }
    }
    private void OnMouseDown()
    {
        Debug.Log("Ŭ��");
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
        print("���� �¾� ����");
        Destroy(gameObject);
    }
    public void Alarm()
    {
        print("����¼�(�Ҹ�����)");
    }
}