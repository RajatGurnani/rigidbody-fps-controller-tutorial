using UnityEngine;

[CreateAssetMenu(fileName = "ZombieData", menuName = "Zombie/Zombie Data")]
public class ZombieData : ScriptableObject
{
    [Header("Health")]
    public float maxHealth = 100f;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 5f;
    public float followDistance = 1f;

    [Header("Combat")]
    public float damage = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 1f;

    [Header("Ragdoll")]
    public bool enableActiveRagdoll = false;
    public float jointSpring = 1000f;
    public float jointDamper = 100f;
    public bool enableRagdollOnDeath = true;
}