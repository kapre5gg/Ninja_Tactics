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
        Gizmos.color = Color.blue; // 공격 범위 기즈모 색상 설정
        Gizmos.DrawWireSphere(transform.position, targetRange); // 공격 범위 시각화
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
        float closestDistance = Mathf.Infinity; // 가장 가까운 적과의 거리
        Enemy closestEnemy = null;

        // 공격 범위 내의 모든 콜라이더 감지
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, targetRange);

        foreach (Collider hitCollider in hitColliders)
        {
            // Enemy 태그를 가진 오브젝트인지 확인
            if (hitCollider.CompareTag("Enemy"))
            {
                // Enemy 스크립트를 가져옴
                Enemy enemy = hitCollider.GetComponent<Enemy>();

                if (enemy != null)
                {
                    // 현재 오브젝트와 Enemy 사이의 거리 계산
                    float distanceToEnemy = Vector3.Distance(transform.position, hitCollider.transform.position);

                    // 가장 가까운 적인지 확인
                    if (distanceToEnemy < closestDistance)
                    {
                        closestDistance = distanceToEnemy;
                        closestEnemy = enemy;
                    }
                }
            }
        }

        // 가장 가까운 적 반환 (없으면 null 반환)
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
            // 공격 범위 내의 모든 콜라이더 감지
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, targetRange);

            foreach (Collider hitCollider in hitColliders)
            {
                // Enemy 태그를 가진 오브젝트인지 확인
                if (hitCollider.CompareTag("Enemy"))
                {
                    // Enemy 스크립트를 가져옴
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
        // 공격 범위 내의 모든 콜라이더 감지
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, targetRange);

        foreach (Collider hitCollider in hitColliders)
        {
            // Enemy 태그를 가진 오브젝트인지 확인
            if (hitCollider.CompareTag("Enemy"))
            {
                // Enemy 스크립트를 가져옴
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
        // 공격 범위 내의 모든 콜라이더 감지
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, targetRange);

        foreach (Collider hitCollider in hitColliders)
        {
            // Enemy 태그를 가진 오브젝트인지 확인
            if (hitCollider.CompareTag("Enemy"))
            {
                // Enemy 스크립트를 가져옴
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
        // 공격 범위 내의 모든 콜라이더 감지
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, targetRange);

        foreach (Collider hitCollider in hitColliders)
        {
            // Enemy 태그를 가진 오브젝트인지 확인
            if (hitCollider.CompareTag("Enemy"))
            {
                // Enemy 스크립트를 가져옴
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
        // 공격 범위 내의 모든 콜라이더 감지
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, targetRange);

        foreach (Collider hitCollider in hitColliders)
        {
            // Enemy 태그를 가진 오브젝트인지 확인
            if (hitCollider.CompareTag("Enemy"))
            {
                // Enemy 스크립트를 가져옴
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
            float speed = agent.velocity.magnitude; // 에이전트 스피드 설정
            anim.SetFloat("MoveSpeed", speed); // 애니메이션 설정

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

            // 마우스 클릭으로 이동 제어
            if (Input.GetMouseButtonDown(0)) // 마우스 왼쪽 클릭
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
            // hit.point가 클릭된 위치의 월드 좌표입니다.
            Vector3 targetPosition = hit.point;

            if (agent != null)
            {
                agent.SetDestination(targetPosition);
            }
            else
            {
                // NavMeshAgent가 없으면 Transform을 직접 이동
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 15);
            }
        }
    }
}

