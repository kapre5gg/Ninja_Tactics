using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class TeleportLocation : MonoBehaviour
{
    [SerializeField] private Transform target; // �̵��� ���
    [SerializeField] private Transform destination; // ��ǥ ��ġ
    [SerializeField] private float moveSpeed = 6f; // �̵� �ӵ�
    [SerializeField] private GameObject hookIcon; // ���콺 ���� �� ǥ���� ������
    private Camera mainCamera; // ���� ī�޶�

    private void Start()
    {
        // ���� ī�޶� �����ɴϴ�
        mainCamera = Camera.main;
        if (hookIcon != null)
        {
            hookIcon.SetActive(false);
        }
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // 3D ������Ʈ�� �浹���� ��
                if (hit.transform == transform)
                {
                    Debug.Log("Ŭ��");
                    // ������Ʈ�� ��ǥ�� �����Ͽ� �ڷ���Ʈ ó��
                    Teleport(target);
                }
            }
        }

        // ���콺 ���� �� ó��
        HandleMouseOver();
    }

    private void HandleMouseOver()
    {
        if (hookIcon != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == transform)
                {
                    // ���콺�� 3D ������Ʈ ���� ���� ��
                    hookIcon.SetActive(true);

                    // hookIcon ��ġ�� 3D ������Ʈ�� ���� ����
                    Vector3 screenPosition = mainCamera.WorldToScreenPoint(transform.position);
                    hookIcon.transform.position = screenPosition;
                }
                else
                {
                    // ���콺�� 3D ������Ʈ�� ����� ��
                    hookIcon.SetActive(false);
                }
            }
            else
            {
                // ���콺�� ȭ�� ������ ������ ��
                hookIcon.SetActive(false);
            }
        }
    }

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
