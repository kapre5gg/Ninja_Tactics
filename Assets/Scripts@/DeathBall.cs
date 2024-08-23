using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathBall : MonoBehaviour
{
    private void Start()
    {
        Destroy(gameObject, 5f);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();

            // Enemy ��ũ��Ʈ�� �ִ� ���
            if (enemy != null)
            {
                enemy.Die();
                Destroy(gameObject);
            }
        }
    }
}
