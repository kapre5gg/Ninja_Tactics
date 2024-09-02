using UnityEngine;
using UnityEngine.AI;

public class Cave : MonoBehaviour
{
    public Transform targetPos;
    public CameraController cam;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            NavMeshAgent agent = other.GetComponent<NavMeshAgent>();
            agent.enabled = false;
            other.transform.position = targetPos.position;
            cam.transform.rotation = Quaternion.identity;
            cam.transform.position = other.transform.position + cam.transform.forward * -5 + cam.transform.up * 12;
            agent.enabled = true;
        }
    }
}
