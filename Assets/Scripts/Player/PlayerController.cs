using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Shooting")]
    public GameObject bulletPrefab;

    [Header("Health")]
    public int maxHP = 100;
    public int currentHP;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public Color hurtFlashColor = Color.red;

    [Header("Gun (rotates with mouse)")]
    public Transform gunPivot; // child transform that holds the gun; if null we rotate root

    // Weapon state
    private WeaponData.WeaponDef currentWeapon;
    private float nextFireTime;
    private int currentAmmo;
    private float ammoRegenTimer;

    // Internal
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 aimDirection;
    private Camera mainCam;
    private bool isDead = false;
    private PlayerStateMachine stateMachine;

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

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Find the GunPivot child for rotation (backward-compatible if missing).
        if (gunPivot == null)
        {
            Transform gp = transform.Find("GunPivot");
            if (gp != null) gunPivot = gp;
        }

        // Make sure the state machine exists, even on older prefabs.
        stateMachine = GetComponent<PlayerStateMachine>();
        if (stateMachine == null) stateMachine = gameObject.AddComponent<PlayerStateMachine>();

        // Start with pistol
        EquipWeapon(WeaponData.WeaponType.Pistol);
    }

    void Update()
    {
        if (isDead || (GameManager.Instance != null && GameManager.Instance.isPaused)) return;

        // Movement input
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;

        // Aim at mouse - only rotates the gun child so the body stays upright.
        if (mainCam != null)
        {
            Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition);
            aimDirection = ((Vector2)mouseWorld - (Vector2)transform.position).normalized;
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

            if (gunPivot != null)
                gunPivot.rotation = Quaternion.Euler(0f, 0f, angle);     // gun sprite faces +X by default
            else
                transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f); // legacy prefab fallback
        }

        // Shoot
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime && currentAmmo > 0)
        {
            Shoot();
            nextFireTime = Time.time + currentWeapon.FireRate;
        }

        // CS:GO-style single key (G):
        //   - Standing on a weapon pickup -> swap with it
        //   - Otherwise -> drop current weapon (only allowed if not the default pistol)
        if (Input.GetKeyDown(KeyCode.G))
        {
            HandleWeaponSwap();
        }

        // Ammo regen
        ammoRegenTimer += Time.deltaTime;
        if (ammoRegenTimer >= 1f / currentWeapon.AmmoRegenRate && currentAmmo < currentWeapon.MaxAmmo)
        {
            currentAmmo++;
            ammoRegenTimer = 0f;
        }

        // Hide interact prompt if no chest nearby
        if (UIManager.Instance != null)
        {
            bool nearChest = false;
            WeaponChest[] chests = Object.FindObjectsByType<WeaponChest>(FindObjectsSortMode.None);
            foreach (var chest in chests)
            {
                if (!chest.isOpened && Vector2.Distance(transform.position, chest.transform.position) < chest.interactRange)
                {
                    nearChest = true;
                    break;
                }
            }
            if (!nearChest) UIManager.Instance.HideInteractPrompt();
        }

        // Feed current data into the state machine.
        if (stateMachine != null)
        {
            stateMachine.Speed = moveInput.magnitude;
            stateMachine.Direction = aimDirection;
        }

        // Update UI.
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHP(currentHP, maxHP);
            UIManager.Instance.UpdateAmmo(currentAmmo, currentWeapon.MaxAmmo);
            UIManager.Instance.UpdateWeaponName(currentWeapon.Name);

            // Top-left debug panel.
            if (stateMachine != null)
            {
                UIManager.Instance.UpdateStateDebug(
                    stateMachine.CurrentState.ToString(),
                    stateMachine.Speed,
                    stateMachine.DirectionString,
                    stateMachine.AimAngle,
                    stateMachine.Grounded);
            }
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

        int pellets = Mathf.Max(1, currentWeapon.PelletCount);
        // Only consume 1 ammo per shot regardless of pellet count
        currentAmmo--;

        for (int i = 0; i < pellets; i++)
        {
            float spread = 0f;
            if (currentWeapon.SpreadAngle > 0)
            {
                if (pellets > 1)
                {
                    // Evenly spread pellets
                    spread = Mathf.Lerp(-currentWeapon.SpreadAngle / 2f, currentWeapon.SpreadAngle / 2f,
                        pellets > 1 ? (float)i / (pellets - 1) : 0.5f);
                }
                else
                {
                    spread = Random.Range(-currentWeapon.SpreadAngle, currentWeapon.SpreadAngle);
                }
            }

            float rad = spread * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(
                aimDirection.x * Mathf.Cos(rad) - aimDirection.y * Mathf.Sin(rad),
                aimDirection.x * Mathf.Sin(rad) + aimDirection.y * Mathf.Cos(rad)
            );

            Vector3 spawnPos = transform.position + (Vector3)(aimDirection * 0.6f);
            GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
            bullet.transform.localScale = Vector3.one * currentWeapon.BulletScale;

            // Set bullet color
            SpriteRenderer bsr = bullet.GetComponent<SpriteRenderer>();
            if (bsr != null) bsr.color = currentWeapon.BulletColor;

            Rigidbody2D bRb = bullet.GetComponent<Rigidbody2D>();
            if (bRb != null) bRb.linearVelocity = dir * currentWeapon.BulletSpeed;

            Bullet b = bullet.GetComponent<Bullet>();
            if (b != null)
            {
                b.damage = currentWeapon.Damage;
                b.effect = currentWeapon.Effect;
                b.effectDuration = currentWeapon.EffectDuration;
                b.effectStrength = currentWeapon.EffectStrength;
            }

            Destroy(bullet, 3f);
        }

        if (AudioManager.Instance != null) AudioManager.Instance.PlayShoot();
        if (stateMachine != null) stateMachine.TriggerShoot();

        // Muzzle flash + spark particles at the gun tip
        Vector3 muzzlePos = transform.position + (Vector3)(aimDirection * 0.65f);
        ParticleFx.Flash(muzzlePos, currentWeapon.BulletColor, 1.2f, 0.06f);
        ParticleFx.Burst(muzzlePos, currentWeapon.BulletColor, 5, 4f, 0.18f, 0.10f);
    }

    /// <summary>
    /// Single-key weapon interaction. If a WeaponPickup is in range, swap with it.
    /// Otherwise drop the current weapon - but never drop the default pistol
    /// (otherwise the floor fills with pistols).
    /// </summary>
    void HandleWeaponSwap()
    {
        WeaponPickup nearest = FindNearestPickup();
        if (nearest != null)
        {
            nearest.DoSwap(this);
            return;
        }
        if (currentWeapon.Type == WeaponData.WeaponType.Pistol) return; // can't drop the default
        Vector3 dropPos = transform.position + (Vector3)(aimDirection * 0.8f);
        WeaponPickup.Spawn(dropPos, currentWeapon.Type);
        EquipWeapon(WeaponData.WeaponType.Pistol);
    }

    WeaponPickup FindNearestPickup()
    {
        WeaponPickup[] all = Object.FindObjectsByType<WeaponPickup>(FindObjectsSortMode.None);
        WeaponPickup best = null;
        float bestDist = float.MaxValue;
        foreach (var p in all)
        {
            float d = Vector2.Distance(transform.position, p.transform.position);
            if (d <= p.interactRange && d < bestDist)
            {
                best = p;
                bestDist = d;
            }
        }
        return best;
    }

    public WeaponData.WeaponType GetCurrentWeaponType() => currentWeapon.Type;

    public void EquipWeapon(WeaponData.WeaponType type)
    {
        currentWeapon = WeaponData.Get(type);
        currentAmmo = currentWeapon.MaxAmmo;
        ammoRegenTimer = 0f;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHP -= damage;
        currentHP = Mathf.Max(0, currentHP);

        if (AudioManager.Instance != null) AudioManager.Instance.PlayPlayerHurt();
        if (stateMachine != null) stateMachine.TriggerHurt();

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
        if (stateMachine != null) stateMachine.TriggerDeath();
        if (GameManager.Instance != null) GameManager.Instance.GameOver();
    }

    public int GetCurrentHP() => currentHP;
    public int GetMaxHP() => maxHP;
    public int GetCurrentAmmo() => currentAmmo;
    public int GetMaxAmmo() => currentWeapon.MaxAmmo;
    public string GetWeaponName() => currentWeapon.Name;
}
