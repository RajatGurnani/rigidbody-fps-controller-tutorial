using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Attach to every bullet prefab.
/// Requires a Rigidbody on the same GameObject.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------

    [Header("Movement")]
    [SerializeField] private float speed = 30f;

    [Tooltip("Optional gravity multiplier (0 = no extra gravity).")]
    [SerializeField] private float gravityScale = 0f;

    [Header("Lifetime")]
    [Tooltip("Auto-return to pool after this many seconds. 0 = disabled.")]
    [SerializeField] private float lifetime = 3f;

    [Header("Damage")]
    [SerializeField] private float damage = 10f;

    [Header("Collision")]
    [SerializeField] private LayerMask hitLayers = ~0;
    [SerializeField] private bool deactivateOnHit = true;

    [Header("Impact Effect")]
    [SerializeField] private GameObject impactEffectPrefab;
    [SerializeField] private TrailRenderer _trailRenderer;

    // -------------------------------------------------------------------------
    // Runtime
    // -------------------------------------------------------------------------

    private string _poolKey;
    private ObjectPool<Bullet> _pool;
    private Rigidbody _rb;
    public Rigidbody RB
    {
        get
        {
            return _rb;
        }
    }
    private Coroutine _lifetimeCoroutine;
    private bool _active;

    // -------------------------------------------------------------------------
    // Called once by BulletPool after Instantiate
    // -------------------------------------------------------------------------

    public void Initialise(string poolKey, ObjectPool<Bullet> pool)
    {
        _poolKey = poolKey;
        _pool = pool;
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
        _trailRenderer = GetComponentInChildren<TrailRenderer>();
        if (_trailRenderer != null)
        {
            _trailRenderer.emitting = false;
            _trailRenderer.Clear();
        }
        gameObject.tag = "Bullet";
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.tag = "Bullet";
        }
    }

    // -------------------------------------------------------------------------
    // Called by ObjectPool's actionOnGet
    // -------------------------------------------------------------------------

    public void OnSpawn()
    {
        _active = true;

        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.linearVelocity = transform.forward * speed;

        if (_lifetimeCoroutine != null) StopCoroutine(_lifetimeCoroutine);
        if (lifetime > 0f) _lifetimeCoroutine = StartCoroutine(LifetimeRoutine());
        if (_trailRenderer != null)
        {
            _trailRenderer.emitting = true;
            _trailRenderer.Clear();
        }
    }

    // -------------------------------------------------------------------------
    // Lifetime
    // -------------------------------------------------------------------------

    private IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(lifetime);
        Release();
    }

    private void Release()
    {
        if (!_active) return;
        _active = false;

        if (_lifetimeCoroutine != null)
        {
            StopCoroutine(_lifetimeCoroutine);
            _lifetimeCoroutine = null;
        }

        _pool.Release(this);   // Unity ObjectPool handles the rest
        if (_trailRenderer != null)
        {
            _trailRenderer.emitting = false;
            _trailRenderer.Clear();
        }
    }

    // -------------------------------------------------------------------------
    // Physics
    // -------------------------------------------------------------------------

    private void FixedUpdate()
    {
        if (!_active || gravityScale == 0f) return;
        _rb.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);
    }

    // -------------------------------------------------------------------------
    // Collision
    // -------------------------------------------------------------------------

    private void OnCollisionEnter(Collision collision)
    {
        if (!_active) return;
        if ((hitLayers.value & (1 << collision.gameObject.layer)) == 0) return;

        HandleHit(collision.gameObject,
                  collision.GetContact(0).point,
                  collision.GetContact(0).normal);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_active) return;
        if ((hitLayers.value & (1 << other.gameObject.layer)) == 0) return;

        HandleHit(other.gameObject, transform.position, -transform.forward);
    }

    private void HandleHit(GameObject target, Vector3 point, Vector3 normal)
    {
        if (target.TryGetComponent<IDamageable>(out var damageable))
            damageable.TakeDamage(damage);

        if (impactEffectPrefab != null)
            Destroy(Instantiate(impactEffectPrefab, point, Quaternion.LookRotation(normal)), 2f);

        if (deactivateOnHit) Release();
    }

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    public float Speed => speed;
    public float Damage => damage;
    public string PoolKey => _poolKey;
}

// -------------------------------------------------------------------------
// Damage interface — implement on any destructible object
// -------------------------------------------------------------------------

// public interface IDamageable
// {
//     void TakeDamage(float amount);
// }