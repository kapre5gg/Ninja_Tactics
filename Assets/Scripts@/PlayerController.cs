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

    public bool isCarryingCorpse = false; // 시체를 들고 있는지 여부
    public GameObject carriedCorpse; // 들고 있는 시체 오브젝트

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
        if (Input.GetKeyDown(KeyCode.LeftControl)) // Ctrl 키를 눌렀을 때 시체를 들기 시도
        {
            if (isCarryingCorpse)
            {
                DropCorpse(); // 시체를 들고 있다면 내려놓기
            }
            else
            {
                TryPickupCorpse(); // 시체를 들고 있지 않다면 들기 시도
            }
        }
        if (Input.GetMouseButtonDown(0)) // 마우스 좌클릭으로 스페어를 던짐
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
        // 마우스 위치를 월드 좌표로 변환
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // 레이가 충돌하는 지점을 찾음
        if (Physics.Raycast(ray, out hit))
        {
            // 스페어 인스턴스를 생성
            GameObject spear = Instantiate(spearPrefab, transform.position, Quaternion.identity);

            // 스페어에 Rigidbody가 있어야 함
            Rigidbody rb = spear.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // 마우스 클릭 위치로의 방향을 계산
                Vector3 direction = (hit.point - transform.position).normalized;

                // 스페어를 해당 방향으로 던짐
                rb.AddForce(direction * 1000);
            }
        }
    }

    private void ThrowSpearAtMouse2()
    {
        // 마우스 위치를 월드 좌표로 변환
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // 레이가 충돌하는 지점을 찾음
        if (Physics.Raycast(ray, out hit))
        {
            // 스페어 인스턴스를 생성
            GameObject spear = Instantiate(spearPrefab2, transform.position, Quaternion.identity);

            // 스페어에 Rigidbody가 있어야 함
            Rigidbody rb = spear.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // 마우스 클릭 위치로의 방향을 계산
                Vector3 direction = (hit.point - transform.position).normalized;

                // 스페어를 해당 방향으로 던짐
                rb.AddForce(direction * 1000);
            }
        }
    }

    private void TryPickupCorpse()
    {
        Debug.Log("시체들기");
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 3f);
        foreach (var collider in hitColliders)   
        {
            if (collider.CompareTag("Corpse")) // 시체 태그를 가진 오브젝트가 있는지 확인
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