using UnityEngine;

public class AvatarRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 100f; // ȸ�� �ӵ�

    private bool isDragging = false;
    private Vector3 lastMousePosition;

    void Update()
    {
        // ���콺 ���� ��ư�� ���� ��
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
        }

        // ���콺 ���� ��ư�� �� ��
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        // �巡�� ���� ��
        if (isDragging)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            float rotationY = delta.x * rotationSpeed * Time.deltaTime;

            transform.Rotate(Vector3.up, -rotationY, Space.World);

            lastMousePosition = Input.mousePosition;
        }
    }
}
