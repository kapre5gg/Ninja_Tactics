using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GHTeleportLocation : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform destination; // ��ǥ ��ġ
    [SerializeField] private float moveSpeed = 1f; // �̵� �ӵ�
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
    private void OnTriggerEnter(Collider other) //�ݶ��̴��� �ε������� �Ʒ� �ڵ� ����
    {
        if (other.CompareTag("Player")) //�÷��̾� �±� �����Ҷ�
        {
            player = other.transform; //�÷��̾�� Ʈ�������̶�� ��.
            switchUpdate(gameObject.tag); //switchUpdate�Լ��� ���� 
        }
    }
    void switchUpdate(string _tag) //1. ��ٸ� ������, 2. ���� ������, 3. �����̵�
    {
        switch (_tag)
        {
            case "Ladder":
                // Ÿ�� �������� ȸ��
                Vector3 direction = (transform.position - player.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                player.rotation = Quaternion.Slerp(player.rotation, lookRotation, Time.deltaTime * 10f);
                DBManager.instance.myCon.ChangeAnimBool("Climbing", true); //�±׸� �˼��ؼ� ��ٸ��̸� ��ٸ� �ִϸ��̼� ����
                Teleport(player);
                break;

            case "Jump":
                DBManager.instance.myCon.ChangeAnim("Jump");
                break;

            case "Cave":
                player.position = destination.position;
                break;
        }

    }

    public void Teleport(Transform target)
    {
        if (target != null && destination != null)
        {
            Debug.Log("�ڷ���Ʈ");
            StartCoroutine(SmoothTeleport(target));
        }
        else
        {
            Debug.LogWarning("��� �Ǵ� ��ǥ ��ġ�� �������� �ʾҽ��ϴ�.");
        }
    }
    private IEnumerator SmoothTeleport(Transform target)
    {
        Debug.Log("�������ڷ�����");
        NavMeshAgent agent = target.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
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
        DBManager.instance.myCon.ChangeAnimBool("Climbing", false);
    }

}