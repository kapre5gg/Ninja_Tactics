using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class TeleportLocation : MonoBehaviour
{
    [SerializeField] private Transform target; // �̵��� ���
    [SerializeField] private Transform destination; // ��ǥ ��ġ
    [SerializeField] private float moveSpeed = 6f; // �̵� �ӵ�

    // ���(target)�� ��ǥ ��ġ(destination)�� �ε巴�� �̵���Ű�� �޼���
    public void Teleport(Transform target)
    {
        if (target != null && destination != null)
        {
            StartCoroutine(SmoothTeleport(target));
        }
        else
        {
            Debug.LogWarning("��� �Ǵ� ��ǥ ��ġ�� �������� �ʾҽ��ϴ�.");
        }
    }

    private IEnumerator SmoothTeleport(Transform target)
    {
        NavMeshAgent agent = target.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
        }

        Animator animator = target.GetComponent<Animator>();
        if (animator != null)
        {
            // �̵� �ִϸ��̼� ����
            animator.SetTrigger("Jump");
        }

        Vector3 startPos = target.position; // ���� ��ġ
        Vector3 endPos = destination.position; // ��ǥ ��ġ
        float distance = Vector3.Distance(startPos, endPos); // �̵��� �Ÿ�
        float elapsed = 0f; // ��� �ð�

        while (elapsed < distance / moveSpeed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed * moveSpeed / distance; // ���� ����
            target.position = Vector3.Lerp(startPos, endPos, t); // �����Ͽ� ���ο� ��ġ ���
            yield return null; // ���� �����ӱ��� ���
        }

        target.position = endPos; // ��Ȯ�� ��ǥ ��ġ�� ����

        if (agent != null)
        {
            agent.enabled = true;
        }
    }
}
