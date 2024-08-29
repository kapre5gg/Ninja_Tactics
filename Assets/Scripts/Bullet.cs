using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private void Update()
    {
        Destroy(gameObject, 5f);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject);
            Debug.Log("�Ѿ��� �÷��̾�� ������ ����");
        }
        else
        {
            Debug.Log("�Ѿ��� �����");
        }
    }
}
