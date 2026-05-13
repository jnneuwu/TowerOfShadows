using UnityEngine;

/// <summary>
/// Holds shared sprites used by particle effects and weapon pickups.
/// Placed in each scene by PrototypeBuilder; pushes its sprites into the
/// static fields that runtime systems read from.
/// </summary>
public class FxAssetHolder : MonoBehaviour
{
    public Sprite lightSprite;
    public Sprite weaponIconSprite;

    void Awake()
    {
        if (lightSprite != null) ParticleFx.SetLightSprite(lightSprite);
        if (weaponIconSprite != null) WeaponPickup.gunIconSprite = weaponIconSprite;
    }
}
