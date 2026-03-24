using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Drag in your bullet prefabs. Pool keys are taken from prefab names automatically.
/// Uses Unity's built-in ObjectPool — auto-expandable, no size cap.
/// </summary>
public class BulletPool : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Singleton
    // -------------------------------------------------------------------------

    public static BulletPool Instance { get; private set; }

    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------

    [Header("Bullet Prefabs")]
    [Tooltip("Drag your bullet prefabs here. The prefab name becomes the pool key.")]
    [SerializeField] private List<Bullet> prefabs = new List<Bullet>();

    [Tooltip("How many instances to pre-warm per pool on Awake.")]
    [SerializeField] private int defaultCapacity = 10;

    [Tooltip("Hard upper limit Unity will keep in memory per pool (excess are destroyed).")]
    [SerializeField] private int maxSize = 100;

    // -------------------------------------------------------------------------
    // Runtime
    // -------------------------------------------------------------------------

    private class PoolEntry
    {
        public Bullet prefab;
        public ObjectPool<Bullet> pool;
        public Transform container;
    }

    private Dictionary<string, PoolEntry> _entries;

    // -------------------------------------------------------------------------
    // Unity
    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildPools();
    }

    // -------------------------------------------------------------------------
    // Setup
    // -------------------------------------------------------------------------

    private void BuildPools()
    {
        _entries = new Dictionary<string, PoolEntry>();

        foreach (Bullet prefab in prefabs)
        {
            if (prefab == null) continue;

            string key = prefab.name;

            if (_entries.ContainsKey(key))
            {
                Debug.LogWarning($"[BulletPool] Duplicate prefab name '{key}' — skipping.");
                continue;
            }

            Transform container = new GameObject($"Pool_{key}").transform;
            container.SetParent(transform);

            // Capture locals for lambdas
            Bullet    capturedPrefab     = prefab;
            string    capturedKey        = key;
            Transform capturedContainer  = container;

            var entry = new PoolEntry { prefab = prefab, container = container };

            entry.pool = new ObjectPool<Bullet>(
                createFunc:      ()       => CreateBullet(capturedPrefab, capturedKey, capturedContainer, entry.pool),
                actionOnGet:     bullet   => { bullet.gameObject.SetActive(true); bullet.OnSpawn(); },
                actionOnRelease: bullet   => { bullet.gameObject.SetActive(false); bullet.transform.SetParent(capturedContainer); },
                actionOnDestroy: bullet   => Destroy(bullet.gameObject),
                collectionCheck: false,
                defaultCapacity: defaultCapacity,
                maxSize:         maxSize
            );

            _entries[key] = entry;
        }
    }

    private Bullet CreateBullet(Bullet prefab, string key, Transform container, ObjectPool<Bullet> pool)
    {
        Bullet bullet = Instantiate(prefab, container);
        bullet.Initialise(key, pool);
        bullet.gameObject.SetActive(false);
        return bullet;
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>Spawn a bullet by prefab name.</summary>
    public Bullet Get(string key, Vector3 position, Quaternion rotation)
    {
        if (!_entries.TryGetValue(key, out PoolEntry entry))
        {
            Debug.LogError($"[BulletPool] No pool found for '{key}'. Check the prefab name matches.");
            return null;
        }

        Bullet bullet = entry.pool.Get();
        bullet.transform.SetPositionAndRotation(position, rotation);
        bullet.OnSpawn();
        return bullet;
    }

    /// <summary>Spawn a bullet using the prefab reference directly.</summary>
    public Bullet Get(Bullet prefab, Vector3 position, Quaternion rotation)
        => Get(prefab.name, position, rotation);
}