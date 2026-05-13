using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 12f;
    public float fireRate = 0.15f;
    public int maxAmmo = 50;
    public float ammoRegenRate = 2f;

    [Header("Health")]
    public int maxHP = 100;
    public int currentHP;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public Color hurtFlashColor = Color.red;

    // Internal
    private Rigidbody2D rb;
    private float nextFireTime;
    private int currentAmmo;
    private float ammoRegenTimer;
    private Vector2 moveInput;
    private Vector2 aimDirection;
    private Camera mainCam;
    private bool isDead = false;

    public static PlayerController Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.linearDamping = 5f;

        mainCam = Camera.main;
        currentHP = maxHP;
        currentAmmo = maxAmmo;

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        if (isDead || (GameManager.Instance != null && GameManager.Instance.isPaused)) return;

        // Movement input
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;

        // Aim at mouse
        if (mainCam != null)
        {
            Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition);
            aimDirection = ((Vector2)mouseWorld - (Vector2)transform.position).normalized;
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }

        // Shoot
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime && currentAmmo > 0)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }

        // Ammo regen
        ammoRegenTimer += Time.deltaTime;
        if (ammoRegenTimer >= 1f / ammoRegenRate && currentAmmo < maxAmmo)
        {
            currentAmmo++;
            ammoRegenTimer = 0f;
        }

        // Update UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHP(currentHP, maxHP);
            UIManager.Instance.UpdateAmmo(currentAmmo, maxAmmo);
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;
        rb.linearVelocity = moveInput * moveSpeed;
    }

    void Shoot()
    {
        if (bulletPrefab == null) return;
        currentAmmo--;

        Vector3 spawnPos = transform.position + (Vector3)(aimDirection * 0.6f);
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        Rigidbody2D bRb = bullet.GetComponent<Rigidbody2D>();
        if (bRb != null) bRb.linearVelocity = aimDirection * bulletSpeed;

        if (AudioManager.Instance != null) AudioManager.Instance.PlayShoot();

        Destroy(bullet, 3f);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHP -= damage;
        currentHP = Mathf.Max(0, currentHP);

        if (AudioManager.Instance != null) AudioManager.Instance.PlayPlayerHurt();

        // Flash red
        if (spriteRenderer != null)
        {
            CancelInvoke("ResetColor");
            spriteRenderer.color = hurtFlashColor;
            Invoke("ResetColor", 0.15f);
        }

        if (currentHP <= 0) Die();
    }

    void ResetColor()
    {
        if (spriteRenderer != null) spriteRenderer.color = Color.white;
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayHealthPickup();
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        if (GameManager.Instance != null) GameManager.Instance.GameOver();
    }

    public int GetCurrentHP() => currentHP;
    public int GetMaxHP() => maxHP;
    public int GetCurrentAmmo() => currentAmmo;
    public int GetMaxAmmo() => maxAmmo;
}
