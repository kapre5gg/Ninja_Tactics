using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mugiwara : Enemy
{
    //���� ����
    //��������(���� �ɷ��� �� ����)

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
}
