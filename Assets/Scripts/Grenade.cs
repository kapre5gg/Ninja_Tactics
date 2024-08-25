using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GrenadeType { Die, Stun};
public class Grenade : MonoBehaviour
{
    public GrenadeType grenadeType;
    [SerializeField] private float stunTime;
    [SerializeField] private GameObject meshObj;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private GameObject flameEffect;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private int explosionTime;
    [SerializeField] private float explosionRadius;
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeMagnitude = 0.4f;
    //[SerializeField] private string explosionSound;

    private CameraShake cameraShake;

    // Start is called before the first frame update
    void Start()
    {
        cameraShake = Camera.main.GetComponent<CameraShake>();
        StartCoroutine(Explosion());
    }

    IEnumerator Explosion()
    {
        yield return new WaitForSeconds(explosionTime);
        //SoundManager.instance.PlaySE(explosionSound);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        meshObj.SetActive(false);
        explosionEffect.SetActive(true);

        if (cameraShake != null)
        {
            StartCoroutine(cameraShake.Shake(shakeDuration, shakeMagnitude));
        }

        RaycastHit[] raycastHits = Physics.SphereCastAll(transform.position, explosionRadius, Vector3.up, 0f, LayerMask.GetMask("Enemy"));
        foreach (RaycastHit hitObj in raycastHits)
        {
            if(grenadeType == GrenadeType.Die)
            {
                hitObj.transform.GetComponent<Enemy>().Die();
            }
            else
            {
                hitObj.transform.GetComponent<Enemy>().ChangeStunState(StunType.Stun, stunTime);
            }
            
        }
        flameEffect.SetActive(true);
        Destroy(gameObject, explosionTime + 3f);
    }

    // 기즈모로 폭발 반경을 표시
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
