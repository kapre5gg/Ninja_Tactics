using UnityEngine;
using System.Collections;

public class PointerManager : MonoBehaviour
{
    [Header("������ ��ƼŬ �ý���")]
    public ParticleSystem pointerParticleSystem;

    public float pointerDuration = 0.5f; // �����Ͱ� ȭ�鿡 ǥ�õǴ� �ð� (��)

    [Header("������ ������")]
    public float pointerYOffset = 0.5f; // �����Ͱ� �������� ���� �� �ְ� �ϴ� y ������

    void Start()
    {
        if (pointerParticleSystem == null)
        {
            Debug.LogError("Pointer ParticleSystem�� �Ҵ��ϼ���.");
            return;
        }

        pointerParticleSystem.Stop(); // �ʱ⿡�� ��Ȱ��ȭ
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // ���콺 ���� ��ư Ŭ��
        {
            Vector2 screenPosition = Input.mousePosition; // ���콺 Ŭ�� ��ġ (��ũ�� ��ǥ)
            ShowPointer(screenPosition);
        }
    }

    void ShowPointer(Vector2 screenPosition)
    {
        if (pointerParticleSystem == null)
            return;

        // Raycasting ����Ͽ� ȭ�� ��ǥ�� ���� ��ǥ�� ��ȯ
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        // ����ĳ��Ʈ�� �浹�ϴ� ������ ã�� ���� Ư�� ���̾ ������ �� �ֽ��ϴ�.
        if (Physics.Raycast(ray, out hit))
        {
            // �����͸� �浹 ��ġ�� ��ġ�ϵ�, y �������� ����
            Vector3 worldPosition = hit.point;
            worldPosition.y += pointerYOffset; // y ������ ����
            pointerParticleSystem.transform.position = worldPosition;

            // ��ƼŬ �ý��� ���
            pointerParticleSystem.Play();

            // �����Ͱ� ���� �ð� �� ������� ��
            Invoke("HidePointer", pointerDuration);
        }
    }

    void HidePointer()
    {
        if (pointerParticleSystem != null)
        {
            // ��ƼŬ �ý��� ����
            pointerParticleSystem.Stop();
        }
    }
}
