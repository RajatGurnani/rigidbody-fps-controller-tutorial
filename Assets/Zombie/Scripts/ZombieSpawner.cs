using UnityEngine;
using System.Collections.Generic;

public class ZombieSpawner : MonoBehaviour
{
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private int maxZombies = 10;
    [SerializeField] private float spawnRadius = 20f;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private Transform player;

    private List<GameObject> activeZombies = new List<GameObject>();
    private float lastSpawnTime;

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
    }

    void Update()
    {
        // Remove dead zombies
        activeZombies.RemoveAll(z => z == null);

        // Spawn if needed
        if (activeZombies.Count < maxZombies && Time.time - lastSpawnTime > spawnInterval)
        {
            SpawnZombie();
        }
    }

    void SpawnZombie()
    {
        // Random position around player
        Vector3 spawnPos = player.position + Random.insideUnitSphere * spawnRadius;
        spawnPos.y = player.position.y; // Keep on ground

        GameObject zombie = Instantiate(zombiePrefab, spawnPos, Quaternion.identity);
        activeZombies.Add(zombie);
        lastSpawnTime = Time.time;
    }
}