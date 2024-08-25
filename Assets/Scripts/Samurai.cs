using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Samurai : Enemy
{
    //교란내성 사무라이갑주 변장간파

    //교란내성(스턴 걸렸을 때 무시)
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

    //사무라이갑주(사무라이에게만 데미지 입음)

    //변장간파(쿠노이치의 변장에 속지 않음)


