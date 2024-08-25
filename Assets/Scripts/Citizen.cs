using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Citizen : Enemy
{
    // �����û
    protected override void AttackState()
    {
        if (!isAttacking && atkTarget != null) // ���� ���� �ƴϰ� Ÿ���� �����ϸ�
        {
            StartCoroutine(AttackRoutine()); // ���� �ڷ�ƾ ����
        }
    }
    private IEnumerator AttackRoutine()
    {
        if (fieldOfView.isViewMeshVisible) // �þ� ���� �޽��� Ȱ��ȭ�Ǿ��ִٸ�
        {
            fieldOfView.isViewMeshVisible = false; // �þ� ���� �޽� ��Ȱ��ȭ
        }

        StartCoroutine(SetDialogue("Attack")); // ���� ��� ���
       
        if (!isAttacking && atkTarget != null)
        {
            isAttacking = true; // ���� �� ���� ����

            // ���� ����� ���� ã�� �̵� �� �÷��̾� ��ġ �˸�
            yield return StartCoroutine(MoveToClosestEnemyAndNotify());

            yield return new WaitForSeconds(attackInterval);
            enemyState = EnemyState.Patrol; // ���� ���·� ��ȯ
            isAttacking = false; // ���� �� ���� ����
        }
        

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
            enemyState = EnemyState.Patrol; // ���� ���·� ��ȯ 
            agent.SetDestination(waypoints[currentWaypointIndex].position); // ������Ʈ�� �̵� ������ ���� �ε����� ��������Ʈ�� ����
            if (fieldOfView.isViewMeshVisible) // �þ� ���� �޽��� Ȱ��ȭ�Ǿ��ִٸ�
            {
                fieldOfView.isViewMeshVisible = false; // �þ� ���� �޽� ��Ȱ��ȭ
            }
        }
    }

    private IEnumerator MoveToClosestEnemyAndNotify()
    {
        Vector3 playerLastKnownPosition = atkTarget.transform.position; // �÷��̾��� ���� ��ġ�� �ʱⰪ���� ����
        Guard closestGuard = FindClosestGuard();
        if (closestGuard == null)
        {
            Debug.Log("����� �� ����");
            yield break;
        }
        anim.SetBool("Running", true); // �ִϸ��̼� ����
        // ����� ������ �̵�
        agent.SetDestination(closestGuard.transform.position);
        Debug.Log("������ �̵� ����...");

        // stoppingDistance�� ���� ��ġ�� �����ϰ� ����
        agent.stoppingDistance = 1.0f;  // �����ߴٰ� �Ǵ��� �Ÿ� (��: 1.0m)

        while (true)
        {
            // ������ �Ǵ��ϱ� ���� stoppingDistance�� ���
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                Debug.Log("������ ����");
                anim.SetBool("Running", false); // �ִϸ��̼� ����
                break;
            }

            // ���� ��ġ�� ũ�� ������ ������ �������� ����
            if (Vector3.Distance(agent.destination, closestGuard.transform.position) > agent.stoppingDistance)
            {
                agent.SetDestination(closestGuard.transform.position);
                Debug.Log("���� ��ġ ����");
            }

            yield return null;  // �� �����Ӹ��� Ȯ��
        }
        
        // ������ �Ŀ� �÷��̾ �߰ߵ� ��ġ�� �̵��ϵ��� ������ ���
        closestGuard.MoveToLastKnownPosition(playerLastKnownPosition);
        //agent.SetDestination(playerLastKnownPosition);
    }


    // �ֺ��� �ٸ� ���� �� ���� ����� ���� ã�� �޼���
    private Guard FindClosestGuard()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 50f); // 50m �ݰ� ���� ��� �ݶ��̴� ����
        Guard closestGuard = null;
        float closestDistance = Mathf.Infinity;

        foreach (var collider in hitColliders)
        {
            Guard nearbyGuard = collider.GetComponent<Guard>();
            if (nearbyGuard != null && nearbyGuard != this) // �ڽ��� ������ �ٸ� Enemy ��ü
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

