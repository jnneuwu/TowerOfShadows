using UnityEngine;

public class ShadowCharger : EnemyBase
{
    [Header("Charger Settings")]
    public float chargeSpeed = 12f;
    public float windupTime = 0.5f;
    public float stunDuration = 2f;
    public float chargeCooldown = 3f;
    public int chargeDamage = 30;
    public float chargeDistance = 8f;

    private enum State { Idle, Windup, Charging, Stunned, Cooldown }
    private State state = State.Idle;
    private float stateTimer;
    private Vector2 chargeDir;
    private float chargeTimer;

    protected override void Start()
    {
        base.Start();
        maxHP = 80;
        currentHP = maxHP;
        contactDamage = 15;
        detectionRange = 8f;
    }

    void Update()
    {
        if (isDead) return;
        switch (state)
        {
            case State.Idle:
                rb.linearVelocity = Vector2.zero;
                if (DistanceToPlayer() < detectionRange)
                {
                    state = State.Windup;
                    stateTimer = windupTime;
                    chargeDir = DirectionToPlayer();
                    // Visual telegraph - scale pulse
                    if (sr != null) sr.color = new Color(1f, 0.5f, 0.5f);
                }
                break;

            case State.Windup:
                rb.linearVelocity = Vector2.zero;
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    state = State.Charging;
                    chargeTimer = 0.8f;
                    contactDamage = chargeDamage;
                    if (sr != null) sr.color = Color.white;
                }
                break;

            case State.Charging:
                rb.linearVelocity = chargeDir * chargeSpeed;
                chargeTimer -= Time.deltaTime;
                if (chargeTimer <= 0)
                {
                    state = State.Cooldown;
                    stateTimer = chargeCooldown;
                    contactDamage = 15;
                    rb.linearVelocity = Vector2.zero;
                }
                break;

            case State.Stunned:
                rb.linearVelocity = Vector2.zero;
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    state = State.Cooldown;
                    stateTimer = chargeCooldown * 0.5f;
                    if (sr != null) sr.color = Color.white;
                }
                break;

            case State.Cooldown:
                rb.linearVelocity = Vector2.zero;
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0) state = State.Idle;
                break;
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (isDead) return;
        if (state == State.Charging)
        {
            if (col.gameObject.CompareTag("Wall") || col.gameObject.layer == LayerMask.NameToLayer("Wall")
                || col.gameObject.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>() != null)
            {
                // Stunned!
                state = State.Stunned;
                stateTimer = stunDuration;
                rb.linearVelocity = Vector2.zero;
                contactDamage = 15;
                if (sr != null) sr.color = Color.yellow;
            }

            PlayerController p = col.gameObject.GetComponent<PlayerController>();
            if (p != null) p.TakeDamage(chargeDamage);
        }
    }

    public override void TakeDamage(int damage)
    {
        int actual = state == State.Stunned ? Mathf.RoundToInt(damage * 1.5f) : damage;
        base.TakeDamage(actual);
    }
}
