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
            //NinjaController targetNinja = other.GetComponent<NinjaController>();
            //targetNinja.OnDamage();
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("√—æÀ¿Ã ªÁ∂Û¡¸");
        }
    }
}
