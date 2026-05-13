using UnityEngine;

public enum BulletEffect { None, Burn, Slow, Pierce }

public static class WeaponData
{
    public enum WeaponType
    {
        Pistol,
        MachineGun,
        Shotgun,
        Sniper,
        FlameGun,
        FrostGun,
        LaserGun
    }

    public struct WeaponDef
    {
        public string Name;
        public WeaponType Type;
        public int Damage;
        public float FireRate;
        public int MaxAmmo;
        public float BulletSpeed;
        public float AmmoRegenRate;
        public int PelletCount;
        public float SpreadAngle;
        public BulletEffect Effect;
        public float EffectDuration;
        public float EffectStrength;
        public Color BulletColor;
        public float BulletScale;
    }

    public static readonly WeaponDef[] Weapons = new[]
    {
        new WeaponDef
        {
            Name = "Pistol", Type = WeaponType.Pistol,
            Damage = 10, FireRate = 0.3f, MaxAmmo = 30,
            BulletSpeed = 12f, AmmoRegenRate = 2f,
            PelletCount = 1, SpreadAngle = 0f,
            Effect = BulletEffect.None,
            BulletColor = new Color(1f, 0.97f, 0.45f),
            BulletScale = 0.42f
        },
        new WeaponDef
        {
            Name = "Machine Gun", Type = WeaponType.MachineGun,
            Damage = 6, FireRate = 0.08f, MaxAmmo = 80,
            BulletSpeed = 14f, AmmoRegenRate = 4f,
            PelletCount = 1, SpreadAngle = 5f,
            Effect = BulletEffect.None,
            BulletColor = new Color(1f, 0.85f, 0.3f),
            BulletScale = 0.3f
        },
        new WeaponDef
        {
            Name = "Shotgun", Type = WeaponType.Shotgun,
            Damage = 8, FireRate = 0.55f, MaxAmmo = 20,
            BulletSpeed = 10f, AmmoRegenRate = 1f,
            PelletCount = 5, SpreadAngle = 30f,
            Effect = BulletEffect.None,
            BulletColor = new Color(1f, 0.6f, 0.2f),
            BulletScale = 0.35f
        },
        new WeaponDef
        {
            Name = "Sniper Rifle", Type = WeaponType.Sniper,
            Damage = 45, FireRate = 0.9f, MaxAmmo = 10,
            BulletSpeed = 22f, AmmoRegenRate = 0.8f,
            PelletCount = 1, SpreadAngle = 0f,
            Effect = BulletEffect.Pierce,
            BulletColor = new Color(0.4f, 1f, 0.4f),
            BulletScale = 0.5f
        },
        new WeaponDef
        {
            Name = "Flame Gun", Type = WeaponType.FlameGun,
            Damage = 5, FireRate = 0.1f, MaxAmmo = 50,
            BulletSpeed = 8f, AmmoRegenRate = 3f,
            PelletCount = 1, SpreadAngle = 8f,
            Effect = BulletEffect.Burn, EffectDuration = 3f, EffectStrength = 4f,
            BulletColor = new Color(1f, 0.4f, 0.1f),
            BulletScale = 0.38f
        },
        new WeaponDef
        {
            Name = "Frost Gun", Type = WeaponType.FrostGun,
            Damage = 8, FireRate = 0.2f, MaxAmmo = 35,
            BulletSpeed = 10f, AmmoRegenRate = 2f,
            PelletCount = 1, SpreadAngle = 0f,
            Effect = BulletEffect.Slow, EffectDuration = 2f, EffectStrength = 0.4f,
            BulletColor = new Color(0.5f, 0.85f, 1f),
            BulletScale = 0.4f
        },
        new WeaponDef
        {
            Name = "Laser Gun", Type = WeaponType.LaserGun,
            Damage = 18, FireRate = 0.22f, MaxAmmo = 25,
            BulletSpeed = 18f, AmmoRegenRate = 1.5f,
            PelletCount = 1, SpreadAngle = 0f,
            Effect = BulletEffect.Pierce,
            BulletColor = new Color(0.9f, 0.3f, 1f),
            BulletScale = 0.35f
        }
    };

    public static WeaponDef Get(WeaponType type)
    {
        return Weapons[(int)type];
    }
}
