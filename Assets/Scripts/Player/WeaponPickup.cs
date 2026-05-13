using UnityEngine;

/// <summary>
/// A weapon lying on the floor. Press G nearby to swap with it (CS:GO style).
/// No light halo - just the gun sprite tinted in the bullet colour.
/// </summary>
public class WeaponPickup : MonoBehaviour
{
    public WeaponData.WeaponType weaponType;
    public float interactRange = 1.6f;

    public static Sprite gunIconSprite;

    SpriteRenderer iconSR;

    void Start()
    {
        WeaponData.WeaponDef def = WeaponData.Get(weaponType);

        // Compact size so the floor doesn't get cluttered
        transform.localScale = Vector3.one * 0.85f;

        // Gun icon - tinted in the bullet colour for quick identification
        iconSR = gameObject.GetComponent<SpriteRenderer>();
        if (iconSR == null) iconSR = gameObject.AddComponent<SpriteRenderer>();
        iconSR.sprite = gunIconSprite;
        iconSR.color = def.BulletColor;
        iconSR.sortingOrder = 9;
    }

    void Update()
    {
        var player = PlayerController.Instance;
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.transform.position);
        if (dist > interactRange) return;

        WeaponData.WeaponDef weapon = WeaponData.Get(weaponType);
        if (UIManager.Instance != null)
            UIManager.Instance.ShowInteractPrompt("Press G to swap to [" + weapon.Name + "]");
    }

    /// <summary>Performs the actual swap. Called by PlayerController when G is pressed.</summary>
    public void DoSwap(PlayerController player)
    {
        WeaponData.WeaponType oldType = player.GetCurrentWeaponType();
        player.EquipWeapon(weaponType);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayChestOpen();
        // Only drop the previous weapon if it wasn't the default pistol -
        // otherwise the floor fills up with pistols every swap.
        if (oldType != WeaponData.WeaponType.Pistol)
            Spawn(transform.position, oldType);
        Destroy(gameObject);
    }

    /// <summary>Spawn a weapon pickup at the given world position.</summary>
    public static GameObject Spawn(Vector3 pos, WeaponData.WeaponType type)
    {
        GameObject obj = new GameObject("WeaponPickup_" + type);
        obj.transform.position = pos;
        var p = obj.AddComponent<WeaponPickup>();
        p.weaponType = type;
        return obj;
    }
}
