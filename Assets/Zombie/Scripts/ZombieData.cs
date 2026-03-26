using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
[CreateAssetMenu(fileName = "ZombieData", menuName = "Zombie/Zombie Data")]
public class ZombieData : ScriptableObject
{
    [Header("Health")]
    public float maxHealth = 100f;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float minSpeed = 3f;
    public float maxSpeed = 5f;


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


    // public float GetRandomSpeed()
    // {

    // }
}


#if UNITY_EDITOR
[CustomEditor(typeof(ZombieData))]
public class Zombie_Data : Editor
{
    public override void OnInspectorGUI()
    {
        ZombieData zombieData = target as ZombieData;

        base.OnInspectorGUI();

        if (GUILayout.Button("Randomize Speed"))
        {
            zombieData.moveSpeed = Random.Range(zombieData.minSpeed, zombieData.maxSpeed);
        }
    }
}

#endif