using UnityEngine;

public class ShadowWalker : EnemyBase
{
    [Header("Walker Settings")]
    public float patrolSpeed = 1.5f;
    public float followSpeed = 2.5f;
    public Transform[] waypoints;

    private int currentWP = 0;
    private bool isFollowing = false;

    protected override void Start()
    {
        base.Start();
        // Apply class defaults only when the value hasn't been customised externally
        // (e.g. by PrototypeBuilder for floor bosses).
        if (maxHP == 20) { maxHP = 20; currentHP = maxHP; }
        if (contactDamage == 10) contactDamage = 10;
        if (detectionRange == 6f) detectionRange = 5f;
    }

    void Update()
    {
        if (isDead) return;
        if (DistanceToPlayer() < detectionRange)
        {
            isFollowing = true;
            FollowPlayer();
        }
        else
        {
            isFollowing = false;
            Patrol();
        }
    }

    void Patrol()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            // Random wander
            if (rb.linearVelocity.magnitude < 0.1f)
            {
                Vector2 dir = Random.insideUnitCircle.normalized;
                rb.linearVelocity = dir * patrolSpeed * slowFactor;
            }
            return;
        }
        Transform target = waypoints[currentWP];
        Vector2 dir2 = ((Vector2)target.position - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir2 * patrolSpeed * slowFactor;
        if (Vector2.Distance(transform.position, target.position) < 0.5f)
        {
            currentWP = (currentWP + 1) % waypoints.Length;
        }
    }

    void FollowPlayer()
    {
        Vector2 dir = DirectionToPlayer();
        rb.linearVelocity = dir * followSpeed * slowFactor;
    }
}
