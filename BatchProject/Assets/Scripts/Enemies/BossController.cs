using UnityEngine;

public class BossController : EnemyBase
{
    [Header("Boss Settings")]
    public string bossName = "Shadow Boss";
    public int phase = 1;

    [Header("Phase 2 - Ranged")]
    public GameObject orbPrefab;
    public float orbFireRate = 2f;
    private float orbTimer;

    [Header("Phase 3 - Summon")]
    public GameObject walkerPrefab;
    public float summonInterval = 8f;
    private float summonTimer;

    [Header("Charge")]
    public float chargeSpeed = 10f;
    public float chargeCooldown = 3f;
    private float chargeTimer;
    private bool isCharging = false;
    private Vector2 chargeDir;
    private float chargeMoveDuration;

    protected override void Start()
    {
        base.Start();
        currentHP = maxHP;
        chargeTimer = 2f;
        orbTimer = orbFireRate;
        summonTimer = summonInterval;
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

        // Phase behaviors
        switch (phase)
        {
            case 1: Phase1_Charge(); break;
            case 2: Phase1_Charge(); Phase2_Orbs(); break;
            case 3: Phase1_Charge(); Phase2_Orbs(); Phase3_Summon(); break;
        }
    }

    void Phase1_Charge()
    {
        if (isCharging)
        {
            rb.linearVelocity = chargeDir * chargeSpeed;
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
        if (orbTimer <= 0 && DistanceToPlayer() < 15f)
        {
            // Fire orb(s)
            Vector2 dir = DirectionToPlayer();
            int count = phase >= 3 ? 3 : 1;
            for (int i = 0; i < count; i++)
            {
                float angle = (i - count / 2f) * 15f;
                Vector2 orbDir = RotateVector(dir, angle);
                Vector3 spawnPos = transform.position + (Vector3)(orbDir * 1f);
                GameObject orb = Instantiate(orbPrefab, spawnPos, Quaternion.identity);
                CasterOrb co = orb.GetComponent<CasterOrb>();
                if (co != null) { co.damage = 25; co.speed = 5f; }
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
            if (p != null) p.TakeDamage(30);
        }
    }

    protected override void Die()
    {
        if (UIManager.Instance != null) UIManager.Instance.HideBossHP();
        base.Die();
    }
}
