using System.Collections.Generic;
using UnityEngine;

public class Barrel : MonoBehaviour
{
    private Rigidbody rb;
    public List<ParticleSystem> explosions;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Bullet"))
        {
            // rb.AddExplosionForce(1000f, transform.position, 100f);
            rb.AddExplosionForce((collision.rigidbody.mass * collision.rigidbody.linearVelocity.magnitude), collision.GetContact(0).point, Mathf.Lerp(0, 100, collision.rigidbody.linearVelocity.magnitude));
            int rand = Random.Range(0, explosions.Count);
            var ps = Instantiate(explosions[rand], collision.GetContact(0).point, Quaternion.identity);
            ps.transform.localScale = Vector3.one * 5f;
            ps.transform.position += Vector3.up * 3f;
            // ps.Play();
            // explosions[rand].gameObject.SetActive(false);
            // explosions[rand].gameObject.SetActive(true);
        }
    }
}
