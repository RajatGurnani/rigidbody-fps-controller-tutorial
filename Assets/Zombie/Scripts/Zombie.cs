using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody))]
public class Zombie : MonoBehaviour, IDamageable
{
    [SerializeField] private ZombieData data;
    [SerializeField] private Transform player;

    private Animator animator;
    private Rigidbody rb;
    private Collider col;

    private float currentHealth;
    private bool isDead = false;
    private float lastAttackTime;

    // Cached per-frame movement intent, written in Update, consumed in FixedUpdate
    private Vector3 pendingMoveDirection;
    private Quaternion pendingRotation;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        currentHealth = data.maxHealth;

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        float scaleMultiplier = transform.lossyScale.magnitude;
        float scaledAttackRange = data.attackRange * scaleMultiplier;
        float scaledFollowDistance = data.followDistance * scaleMultiplier;

        // Compute facing direction toward player (ignore Y)
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
            pendingRotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * data.rotationSpeed);
        else
            pendingRotation = transform.rotation;

        if (distance <= scaledAttackRange)
        {
            // Attack — stop moving
            pendingMoveDirection = Vector3.zero;
            animator.SetBool("IsMoving", false);

            if (Time.time - lastAttackTime > data.attackCooldown)
            {
                lastAttackTime = Time.time;
                animator.SetTrigger("Attack");
                player.GetComponent<IDamageable>()?.TakeDamage(data.damage);
            }
        }
        else if (distance <= scaledFollowDistance)
        {
            // Chase
            pendingMoveDirection = direction;
            animator.SetBool("IsMoving", true);
        }
        else
        {
            // Idle
            pendingMoveDirection = Vector3.zero;
            animator.SetBool("IsMoving", false);
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        // Rotation
        rb.MoveRotation(pendingRotation);

        // Movement — preserve current Y velocity so gravity still applies
        if (pendingMoveDirection != Vector3.zero)
        {
            float scaleMultiplier = transform.lossyScale.magnitude;
            float scaledMoveSpeed = data.moveSpeed * scaleMultiplier;

            Vector3 targetVelocity = pendingMoveDirection * scaledMoveSpeed;
            targetVelocity.y = rb.linearVelocity.y; // keep gravity
            rb.linearVelocity = targetVelocity;
        }
        else
        {
            // Bleed off horizontal velocity when idle or attacking
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0 && !isDead)
            Die();
    }

    void Die()
    {
        isDead = true;
        animator.enabled = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        if (col != null) col.enabled = false;

        Destroy(gameObject, 5f);
    }
}