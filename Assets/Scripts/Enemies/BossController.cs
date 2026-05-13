using UnityEngine;

public class BossController : EnemyBase
{
    [Header("Boss Settings")]
    public string bossName = "Shadow Boss";
    public int phase = 1;

    [Header("Phase 2 - Ranged")]
    public GameObject orbPrefab;
    public float orbFireRate = 1.4f;
    private float orbTimer;

    [Header("Phase 3 - Summon")]
    public GameObject walkerPrefab;
    public float summonInterval = 6f;
    private float summonTimer;

    [Header("Charge")]
    public float chargeSpeed = 14f;
    public float chargeCooldown = 2f;
    private float chargeTimer;
    private bool isCharging = false;
    private Vector2 chargeDir;
    private float chargeMoveDuration;

    [Header("Ring Blast (Phase 3)")]
    public float ringBlastInterval = 5f;
    private float ringBlastTimer;

    protected override void Start()
    {
        base.Start();
        currentHP = maxHP;
        chargeTimer = 1.5f;
        orbTimer = orbFireRate;
        summonTimer = summonInterval;
        ringBlastTimer = ringBlastInterval;
    }

    void Update()
    {
        if (isDead) return;

        // Update phase based on HP
        float hpPct = (float)currentHP / maxHP;
        if (hpPct <= 0.3f) phase = 3;
        else if (hpPct <= 0.7f) phase = 2;
        else phase = 1;

        // Update UI
        if (UIManager.Instance != null)
            UIManager.Instance.ShowBossHP(bossName, hpPct);

        // Phase behaviors (each phase adds onto the previous)
        switch (phase)
        {
            case 1: Phase1_Charge(); break;
            case 2: Phase1_Charge(); Phase2_Orbs(); break;
            case 3: Phase1_Charge(); Phase2_Orbs(); Phase3_Summon(); Phase3_RingBlast(); break;
        }
    }

    void Phase3_RingBlast()
    {
        if (orbPrefab == null) return;
        ringBlastTimer -= Time.deltaTime;
        if (ringBlastTimer > 0) return;
        ringBlastTimer = ringBlastInterval;

        // 12 orbs in a ring around the boss
        const int n = 12;
        for (int i = 0; i < n; i++)
        {
            float angle = i * (360f / n);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector3 spawnPos = transform.position + (Vector3)(dir * 1.2f);
            GameObject orb = Instantiate(orbPrefab, spawnPos, Quaternion.identity);
            CasterOrb co = orb.GetComponent<CasterOrb>();
            if (co != null) { co.damage = 20; co.speed = 6f; co.turnRate = 0f; }
            var orbRb = orb.GetComponent<Rigidbody2D>();
            if (orbRb != null) orbRb.linearVelocity = dir * 6f;
        }
        ParticleFx.Burst(transform.position, new Color(0.4f, 0.7f, 1f), 24, 6f, 0.6f, 0.25f);
        ParticleFx.Flash(transform.position, new Color(0.5f, 0.7f, 1f), 3.5f, 0.18f);
    }

    void Phase1_Charge()
    {
        if (isCharging)
        {
            rb.linearVelocity = chargeDir * chargeSpeed * slowFactor;
            chargeMoveDuration -= Time.deltaTime;
            if (chargeMoveDuration <= 0)
            {
                isCharging = false;
                rb.linearVelocity = Vector2.zero;
                chargeTimer = chargeCooldown;
            }
            return;
        }

        chargeTimer -= Time.deltaTime;
        if (chargeTimer <= 0 && DistanceToPlayer() < 15f)
        {
            // Wind up
            chargeDir = DirectionToPlayer();
            isCharging = true;
            chargeMoveDuration = 0.6f;
            if (sr != null) sr.color = new Color(1f, 0.3f, 0.3f);
            Invoke("ResetBossColor", 0.5f);
        }
    }

    void Phase2_Orbs()
    {
        if (orbPrefab == null) return;
        orbTimer -= Time.deltaTime;
        if (orbTimer <= 0 && DistanceToPlayer() < 18f)
        {
            Vector2 dir = DirectionToPlayer();
            int count = phase >= 3 ? 5 : 3;   // more orbs per volley
            for (int i = 0; i < count; i++)
            {
                float angle = (i - count / 2f) * 12f;
                Vector2 orbDir = RotateVector(dir, angle);
                Vector3 spawnPos = transform.position + (Vector3)(orbDir * 1f);
                GameObject orb = Instantiate(orbPrefab, spawnPos, Quaternion.identity);
                CasterOrb co = orb.GetComponent<CasterOrb>();
                if (co != null) { co.damage = 30; co.speed = 7f; }
            }
            orbTimer = orbFireRate;
        }
    }

    void Phase3_Summon()
    {
        summonTimer -= Time.deltaTime;
        if (summonTimer <= 0)
        {
            // Spawn walkers around boss
            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = Random.insideUnitCircle * 3f;
                Vector3 pos = transform.position + (Vector3)offset;
                GameObject walker = new GameObject("SummonedWalker");
                walker.transform.position = pos;
                walker.transform.localScale = Vector3.one * 0.5f;
                var wsr = walker.AddComponent<SpriteRenderer>();
                wsr.color = new Color(0.53f, 0.33f, 0.8f);
                wsr.sortingOrder = 4;
                // Create a simple square sprite at runtime
                Texture2D tex = new Texture2D(4, 4);
                Color[] colors = new Color[16];
                for (int c = 0; c < 16; c++) colors[c] = Color.white;
                tex.SetPixels(colors);
                tex.Apply();
                wsr.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
                wsr.color = new Color(0.53f, 0.33f, 0.8f);

                var wrb = walker.AddComponent<Rigidbody2D>();
                wrb.gravityScale = 0;
                wrb.freezeRotation = true;
                var wcol = walker.AddComponent<CircleCollider2D>();
                wcol.radius = 0.4f;
                var sw = walker.AddComponent<ShadowWalker>();
                sw.maxHP = 15;
                sw.currentHP = 15;

                if (room != null) room.enemies.Add(sw);
                sw.room = room;
            }
            summonTimer = summonInterval;
        }
    }

    Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }

    void ResetBossColor()
    {
        if (sr != null) sr.color = Color.white;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (isCharging)
        {
            // Hit wall = stop charge
            isCharging = false;
            rb.linearVelocity = Vector2.zero;
            chargeTimer = chargeCooldown * 0.5f;

            PlayerController p = col.gameObject.GetComponent<PlayerController>();
            if (p != null) p.TakeDamage(45);
        }
    }

    protected override void Die()
    {
        if (UIManager.Instance != null) UIManager.Instance.HideBossHP();
        // Big death effect
        ParticleFx.Burst(transform.position, new Color(0.6f, 0.2f, 0.8f), 40, 8f, 0.9f, 0.3f);
        ParticleFx.Flash(transform.position, new Color(1f, 0.4f, 0.8f), 4f, 0.4f);
        base.Die();
    }
}
