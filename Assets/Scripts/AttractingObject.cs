using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public enum ObjectType { Sakke, Kuma }
public class AttractingObject : MonoBehaviour
{
    [SerializeField] private ObjectType type;
    [SerializeField] private int targetRange;
    private bool enemyAttracted = false;
    [SerializeField] private Animator anim;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private ParticleSystem soundEffect;
    [SerializeField] private float stunTime;
    private void Start()
    {
        if (type == ObjectType.Kuma)
        {
            anim = GetComponent<Animator>();
            agent = GetComponent<NavMeshAgent>();
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue; // ���� ���� ����� ���� ����
        Gizmos.DrawWireSphere(transform.position, targetRange); // ���� ���� �ð�ȭ
    }

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            if (type == ObjectType.Sakke && enemy.enemyState == EnemyState.Stun)
            {
                enemy.DrinkSakke(transform.position);
            }
            else if (type == ObjectType.Kuma && enemy.enemyState == EnemyState.Stun)
            {
                anim.SetTrigger("Performance");
                enemy.PlayWithKuma(transform.position);
            }
        }
    }

    private Enemy FindClosestEnemy()
    {
        float closestDistance = Mathf.Infinity; // ���� ����� ������ �Ÿ�
        Enemy closestEnemy = null;

        // ���� ���� ���� ��� �ݶ��̴� ����
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, targetRange);

        foreach (Collider hitCollider in hitColliders)
        {
            // Enemy �±׸� ���� ������Ʈ���� Ȯ��
            if (hitCollider.CompareTag("Enemy"))
            {
                // Enemy ��ũ��Ʈ�� ������
                Enemy enemy = hitCollider.GetComponent<Enemy>();

                if (enemy != null)
                {
                    // ���� ������Ʈ�� Enemy ������ �Ÿ� ���
                    float distanceToEnemy = Vector3.Distance(transform.position, hitCollider.transform.position);

                    // ���� ����� ������ Ȯ��
                    if (distanceToEnemy < closestDistance)
                    {
                        closestDistance = distanceToEnemy;
                        closestEnemy = enemy;
                    }
                }
            }
        }

        // ���� ����� �� ��ȯ (������ null ��ȯ)
        return closestEnemy;
    }

    private void AttractionEnemy()
    {
        Enemy closestEnemy = FindClosestEnemy();
        if (closestEnemy != null)
        {
            if (closestEnemy.enemyState != EnemyState.Stun)
            {
                closestEnemy.ChangeStunState(StunType.Move, stunTime, transform.position);
            }
        }
    }

    private void AttractionRangeAllEnemy()
    {
            // ���� ���� ���� ��� �ݶ��̴� ����
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, targetRange);

            foreach (Collider hitCollider in hitColliders)
            {
                // Enemy �±׸� ���� ������Ʈ���� Ȯ��
                if (hitCollider.CompareTag("Enemy"))
                {
                    // Enemy ��ũ��Ʈ�� ������
                    Enemy enemy = hitCollider.GetComponent<Enemy>();

                    if (enemy != null)
                    {
                        if(enemy.enemyState != EnemyState.Stun)
                        {
                            enemy.ChangeStunState(StunType.Move, stunTime, transform.position);
                        } 
                    }
                }
            }
    }

    private void FixedView()
    {
        // ���� ���� ���� ��� �ݶ��̴� ����
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, targetRange);

        foreach (Collider hitCollider in hitColliders)
        {
            // Enemy �±׸� ���� ������Ʈ���� Ȯ��
            if (hitCollider.CompareTag("Enemy"))
            {
                // Enemy ��ũ��Ʈ�� ������
                Enemy enemy = hitCollider.GetComponent<Enemy>();

                if (enemy != null)
                {
                    if (enemy.enemyState != EnemyState.Stun)
                    {
                        enemy.ChangeStunState(StunType.FixedView, stunTime, transform.position);
                    }
                }
            }
        }
    }

    private void Stun()
    {
        // ���� ���� ���� ��� �ݶ��̴� ����
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, targetRange);

        foreach (Collider hitCollider in hitColliders)
        {
            // Enemy �±׸� ���� ������Ʈ���� Ȯ��
            if (hitCollider.CompareTag("Enemy"))
            {
                // Enemy ��ũ��Ʈ�� ������
                Enemy enemy = hitCollider.GetComponent<Enemy>();

                if (enemy != null)
                {
                    if (enemy.enemyState != EnemyState.Stun)
                    {
                        enemy.ChangeStunState(StunType.Stun, stunTime);
                    }
                }
            }
        }
    }

    private void BlockingView()
    {
        // ���� ���� ���� ��� �ݶ��̴� ����
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, targetRange);

        foreach (Collider hitCollider in hitColliders)
        {
            // Enemy �±׸� ���� ������Ʈ���� Ȯ��
            if (hitCollider.CompareTag("Enemy"))
            {
                // Enemy ��ũ��Ʈ�� ������
                Enemy enemy = hitCollider.GetComponent<Enemy>();

                if (enemy != null)
                {
                    if (enemy.enemyState != EnemyState.Stun)
                    {
                        enemy.ChangeStunState(StunType.BlockingView, stunTime);
                    }
                }
            }
        }
    }

    private void RotateView()
    {
        // ���� ���� ���� ��� �ݶ��̴� ����
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, targetRange);

        foreach (Collider hitCollider in hitColliders)
        {
            // Enemy �±׸� ���� ������Ʈ���� Ȯ��
            if (hitCollider.CompareTag("Enemy"))
            {
                // Enemy ��ũ��Ʈ�� ������
                Enemy enemy = hitCollider.GetComponent<Enemy>();

                if (enemy != null)
                {
                    if (enemy.enemyState != EnemyState.Stun)
                    {
                        enemy.ChangeStunState(StunType.RotateView, stunTime, transform.position);
                    }
                }
            }
        }
    }

    private void Update()
    {
        if(type == ObjectType.Sakke)
        {
            AttractionEnemy();
        }
        if (type == ObjectType.Kuma)
        {
            float speed = agent.velocity.magnitude; // ������Ʈ ���ǵ� ����
            anim.SetFloat("MoveSpeed", speed); // �ִϸ��̼� ����

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                anim.SetTrigger("Howling");
                soundEffect.Play();
                Stun();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                anim.SetTrigger("Howling");
                soundEffect.Play();
                BlockingView();
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                anim.SetTrigger("Howling");
                soundEffect.Play();
                FixedView();
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                anim.SetTrigger("Howling");
                soundEffect.Play();
                RotateView();
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                anim.SetTrigger("Howling");
                soundEffect.Play();
                AttractionRangeAllEnemy();
            }

            // ���콺 Ŭ������ �̵� ����
            if (Input.GetMouseButtonDown(0)) // ���콺 ���� Ŭ��
            {
                MoveToMousePosition();
            }
        }
    }

    private void MoveToMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // hit.point�� Ŭ���� ��ġ�� ���� ��ǥ�Դϴ�.
            Vector3 targetPosition = hit.point;

            if (agent != null)
            {
                agent.SetDestination(targetPosition);
            }
            else
            {
                // NavMeshAgent�� ������ Transform�� ���� �̵�
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 15);
            }
        }
    }
}

