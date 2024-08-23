using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject spearPrefab;
    public GameObject spearPrefab2;
    public bool isCrouch;
    [SerializeField]
    float speed = 8.0f;

    Rigidbody rb;

    Vector3 velocity;
    float rotDegree;

    public bool isCarryingCorpse = false; // ��ü�� ��� �ִ��� ����
    public GameObject carriedCorpse; // ��� �ִ� ��ü ������Ʈ

    private void Start()
    {
        rb = transform.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        Vector3 mousePos = Input.mousePosition;

        float dz = mousePos.z - rb.position.z;
        float dx = mousePos.x - rb.position.x;
        rotDegree = -(Mathf.Rad2Deg * Mathf.Atan2(dz, dx) - 90);
        velocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized * speed;

        if(Input.GetKeyDown(KeyCode.Space))
        {
            isCrouch = !isCrouch; 
        }
        if (Input.GetKeyDown(KeyCode.LeftControl)) // Ctrl Ű�� ������ �� ��ü�� ��� �õ�
        {
            if (isCarryingCorpse)
            {
                DropCorpse(); // ��ü�� ��� �ִٸ� ��������
            }
            else
            {
                TryPickupCorpse(); // ��ü�� ��� ���� �ʴٸ� ��� �õ�
            }
        }
        if (Input.GetMouseButtonDown(0)) // ���콺 ��Ŭ������ ���� ����
        {
            ThrowSpearAtMouse();
        }
        if (Input.GetMouseButtonDown(1))
        {
            ThrowSpearAtMouse2();
        }

        MoveCorpse();
    }

    private void FixedUpdate()
    {
        rb.MoveRotation(Quaternion.Euler(0, rotDegree, 0));
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }
    private void ThrowSpearAtMouse()
    {
        // ���콺 ��ġ�� ���� ��ǥ�� ��ȯ
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // ���̰� �浹�ϴ� ������ ã��
        if (Physics.Raycast(ray, out hit))
        {
            // ����� �ν��Ͻ��� ����
            GameObject spear = Instantiate(spearPrefab, transform.position, Quaternion.identity);

            // ���� Rigidbody�� �־�� ��
            Rigidbody rb = spear.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // ���콺 Ŭ�� ��ġ���� ������ ���
                Vector3 direction = (hit.point - transform.position).normalized;

                // ���� �ش� �������� ����
                rb.AddForce(direction * 1000);
            }
        }
    }

    private void ThrowSpearAtMouse2()
    {
        // ���콺 ��ġ�� ���� ��ǥ�� ��ȯ
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // ���̰� �浹�ϴ� ������ ã��
        if (Physics.Raycast(ray, out hit))
        {
            // ����� �ν��Ͻ��� ����
            GameObject spear = Instantiate(spearPrefab2, transform.position, Quaternion.identity);

            // ���� Rigidbody�� �־�� ��
            Rigidbody rb = spear.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // ���콺 Ŭ�� ��ġ���� ������ ���
                Vector3 direction = (hit.point - transform.position).normalized;

                // ���� �ش� �������� ����
                rb.AddForce(direction * 1000);
            }
        }
    }

    private void TryPickupCorpse()
    {
        Debug.Log("��ü���");
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 3f);
        foreach (var collider in hitColliders)   
        {
            if (collider.CompareTag("Corpse")) // ��ü �±׸� ���� ������Ʈ�� �ִ��� Ȯ��
            {
                carriedCorpse = collider.gameObject;
                isCarryingCorpse = true;
            }
        }
    }

    private void DropCorpse()
    {
        if (carriedCorpse != null)
        {
            carriedCorpse = null;
        }
        isCarryingCorpse = false;
    }

    private void MoveCorpse()
    {
        if(isCarryingCorpse)
        {
            carriedCorpse.transform.position = transform.position;
        }
    }
        
}