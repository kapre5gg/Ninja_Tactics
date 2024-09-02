using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cave : MonoBehaviour
{
    public Transform targetPos;
    public CameraController cam;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.position = targetPos.position;
            cam.transform.position = other.transform.position;
            cam.transform.rotation = Quaternion.identity;
        }
    }
}
