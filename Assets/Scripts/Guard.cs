using System.Collections;
using UnityEngine;

public class Guard : Enemy
{
    // 교란당함
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
    // 도움제공
    public void MoveToLastKnownPosition(Vector3 position)
    {
        // 적이 플레이어의 마지막 위치로 이동하도록 설정
        agent.SetDestination(position);
        Debug.Log("플레이어가 마지막으로 발견된 위치로 이동 중...");
        Debug.Log(position);
    }
}
