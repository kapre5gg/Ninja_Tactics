using System.Collections;
using UnityEngine;

public class Guard : Enemy
{
    // ��������
    private IEnumerator StunRoutine()
    {
            agent.isStopped = true;
            anim.SetFloat("MoveSpeed", 0);
            anim.SetBool("isStunned", true);
            fieldOfView.viewRadius = 0f;
            yield return new WaitForSeconds(stunTime);
            fieldOfView.viewRadius = initialViewRadius;
            anim.SetBool("isStunned", false);

            agent.isStopped = false;
    }
    // ��������
    public void MoveToLastKnownPosition(Vector3 position)
    {
        // ���� �÷��̾��� ������ ��ġ�� �̵��ϵ��� ����
        agent.SetDestination(position);
        Debug.Log("�÷��̾ ���������� �߰ߵ� ��ġ�� �̵� ��...");
        Debug.Log(position);
    }
}
