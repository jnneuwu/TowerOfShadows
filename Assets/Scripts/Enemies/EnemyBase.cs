using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    public int maxHP = 20;
    public int currentHP;
    public int contactDamage = 10;
    public float detectionRange = 6f;

    [Header("Drops")]
    public GameObject healthPickupPrefab;
    [Range(0f, 1f)] public float dropChance = 0.2f;
    public bool isFloorBoss = false;

    protected Rigidbody2D rb;
    protected Transform player;
    protected SpriteRenderer sr;
    protected bool isDead = false;

    public RoomManager room;

    // Status effects
    [HideInInspector] public float slowFactor = 1f;
    private Coroutine burnCoroutine;
    private Coroutine slowCoroutine;
    private float burnAccum;
    private Color baseColor = Color.white;

    protected virtual void Start()
    {
        currentHP = maxHP;
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.linearDamping = 3f;

        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();

        FindPlayer();
    }

    protected void FindPlayer()
    {
        if (PlayerController.Instance != null)
            player = PlayerController.Instance.transform;
        else
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    protected float DistanceToPlayer()
    {
        if (player == null) { FindPlayer(); return 999f; }
        return Vector2.Distance(transform.position, player.position);
    }

    protected Vector2 DirectionToPlayer()
    {
        if (player == null) { FindPlayer(); return Vector2.zero; }
        return ((Vector2)player.position - (Vector2)transform.position).normalized;
    }

    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHP -= damage;
        if (sr != null)
        {
            CancelInvoke("ResetColor");
            sr.color = Color.red;
            Invoke("ResetColor", 0.1f);
        }
        if (currentHP <= 0) Die();
    }

    void ResetColor()
    {
        if (sr != null) sr.color = baseColor;
    }

    public void ApplyBurn(float duration, float dps)
    {
        if (isDead) return;
        if (burnCoroutine != null) StopCoroutine(burnCoroutine);
        burnCoroutine = StartCoroutine(BurnRoutine(duration, dps));
    }

    IEnumerator BurnRoutine(float duration, float dps)
    {
        float timer = duration;
        burnAccum = 0f;
        baseColor = new Color(1f, 0.5f, 0.2f);
        while (timer > 0 && !isDead)
        {
            timer -= Time.deltaTime;
            burnAccum += dps * Time.deltaTime;
            if (burnAccum >= 1f)
            {
                int dmg = Mathf.FloorToInt(burnAccum);
                burnAccum -= dmg;
                currentHP -= dmg;
                if (sr != null) sr.color = new Color(1f, 0.4f, 0.1f);
                if (currentHP <= 0) { Die(); yield break; }
            }
            yield return null;
        }
        baseColor = Color.white;
        if (sr != null) sr.color = Color.white;
    }

    public void ApplySlow(float duration, float factor)
    {
        if (isDead) return;
        if (slowCoroutine != null) StopCoroutine(slowCoroutine);
        slowCoroutine = StartCoroutine(SlowRoutine(duration, factor));
    }

    IEnumerator SlowRoutine(float duration, float factor)
    {
        slowFactor = factor;
        baseColor = new Color(0.5f, 0.85f, 1f);
        if (sr != null) sr.color = baseColor;
        yield return new WaitForSeconds(duration);
        slowFactor = 1f;
        baseColor = Color.white;
        if (sr != null) sr.color = Color.white;
    }

    protected virtual void Die()
    {
        isDead = true;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayEnemyDeath();

        // Death particle burst (uses the enemy's own colour)
        Color burstColor = (sr != null) ? sr.color : Color.white;
        if (burstColor.a < 0.5f) burstColor = Color.white;
        ParticleFx.Burst(transform.position, burstColor, 16, 5f, 0.5f, 0.2f);

        if (healthPickupPrefab != null && Random.value <= dropChance)
        {
            Instantiate(healthPickupPrefab, transform.position, Quaternion.identity);
        }

        if (room != null)
        {
            if (isFloorBoss) room.OnBossDefeated(this);
            else room.OnEnemyDied(this);
        }
        Destroy(gameObject);
    }

    protected virtual void OnCollisionStay2D(Collision2D col)
    {
        if (isDead) return;
        PlayerController p = col.gameObject.GetComponent<PlayerController>();
        if (p != null)
        {
            p.TakeDamage(contactDamage);
        }
    }
}
