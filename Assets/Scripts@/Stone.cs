using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : MonoBehaviour
{
    private void Start()
    {
        Destroy(gameObject,5f);
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();

            // Enemy 스크립트가 있는 경우
            if (enemy != null)
            {
                //enemy.ChangeStunState();
                Destroy(gameObject);
            }
        }
    }
}
