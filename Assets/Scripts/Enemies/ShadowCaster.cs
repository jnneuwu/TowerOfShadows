using UnityEngine;

public class ShadowCaster : EnemyBase
{
    [Header("Caster Settings")]
    public GameObject orbPrefab;
    public float fireInterval = 3f;
    public float orbSpeed = 4f;
    public int orbDamage = 20;

    private float fireTimer;

    protected override void Start()
    {
        base.Start();
        // Class defaults only fill in when stats weren't explicitly set elsewhere.
        if (maxHP == 20) { maxHP = 50; currentHP = maxHP; }
        if (contactDamage == 10) contactDamage = 5;
        if (detectionRange == 6f) detectionRange = 10f;
        fireTimer = fireInterval;
    }

    void Update()
    {
        if (isDead) return;
        rb.linearVelocity = Vector2.zero; // Static enemy

        if (DistanceToPlayer() < detectionRange)
        {
            fireTimer -= Time.deltaTime;
            // Face player
            Vector2 dir = DirectionToPlayer();
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

            if (fireTimer <= 0)
            {
                FireOrb();
                fireTimer = fireInterval;
            }
        }
    }

    void FireOrb()
    {
        if (orbPrefab == null || player == null) return;

        Vector2 dir = DirectionToPlayer();
        Vector3 spawnPos = transform.position + (Vector3)(dir * 0.8f);
        GameObject orb = Instantiate(orbPrefab, spawnPos, Quaternion.identity);

        CasterOrb orbScript = orb.GetComponent<CasterOrb>();
        if (orbScript != null)
        {
            orbScript.speed = orbSpeed;
            orbScript.damage = orbDamage;
            orbScript.target = player;
        }
        else
        {
            Rigidbody2D orbRb = orb.GetComponent<Rigidbody2D>();
            if (orbRb != null) orbRb.linearVelocity = dir * orbSpeed;
            Bullet b = orb.GetComponent<Bullet>();
            if (b != null) { b.damage = orbDamage; b.isPlayerBullet = false; }
            Destroy(orb, 6f);
        }
    }
}
