using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class PrototypeAssets
{
    public const string GeneratedRoot = "Assets/Generated";
    public const string SpriteFolder = GeneratedRoot + "/Sprites";
    public const string TileFolder = GeneratedRoot + "/Tiles";
    public const string PrefabFolder = GeneratedRoot + "/Prefabs";

    enum Shape
    {
        Square,
        Circle,
        Diamond
    }

    public sealed class Bundle
    {
        public Tile backgroundTile;
        public Tile floorTile;
        public Tile wallTopTile;
        public Tile wallFrontTile;
        public Tile wallShadowTile;
        public GameObject playerPrefab;
        public GameObject bulletPrefab;
        public GameObject orbPrefab;
        public GameObject healthPickupPrefab;
        public GameObject cratePrefab;
        public GameObject portalPrefab;
        public GameObject walkerPrefab;
        public GameObject chargerPrefab;
        public GameObject casterPrefab;
        public GameObject finalBossPrefab;
    }

    public static void EnsureFolders()
    {
        EnsureFolder("Assets", "Generated");
        EnsureFolder(GeneratedRoot, "Sprites");
        EnsureFolder(GeneratedRoot, "Tiles");
        EnsureFolder(GeneratedRoot, "Prefabs");
    }

    static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }

    public static Bundle Build()
    {
        EnsureFolders();

        Sprite backgroundSprite = CreateSprite("background_grid.png", MakeBackgroundTexture());
        Sprite floorSprite = CreateSprite("floor_tile.png", MakeFloorTexture());
        Sprite wallTopSprite = CreateSprite("wall_top.png", MakeWallTopTexture());
        Sprite wallFrontSprite = CreateSprite("wall_front.png", MakeWallFrontTexture());
        Sprite wallShadowSprite = CreateSprite("wall_shadow.png", MakeWallShadowTexture());
        Sprite playerSprite = CreateSprite("player.png", MakeFlatTexture(new Color(0.15f, 0.9f, 1f), Shape.Circle));
        Sprite walkerSprite = CreateSprite("walker.png", MakeFlatTexture(new Color(0.53f, 0.33f, 0.8f), Shape.Circle));
        Sprite chargerSprite = CreateSprite("charger.png", MakeFlatTexture(new Color(1f, 0.35f, 0.5f), Shape.Circle));
        Sprite casterSprite = CreateSprite("caster.png", MakeFlatTexture(new Color(0.35f, 0.7f, 1f), Shape.Circle));
        Sprite bossSprite = CreateSprite("boss.png", MakeFlatTexture(new Color(0.45f, 0.1f, 0.55f), Shape.Circle));
        Sprite bulletSprite = CreateSprite("bullet.png", MakeBulletTexture());
        Sprite orbSprite = CreateSprite("orb.png", MakeFlatTexture(new Color(0.35f, 0.7f, 1f), Shape.Circle));
        Sprite pickupSprite = CreateSprite("pickup.png", MakeFlatTexture(new Color(0.2f, 1f, 0.4f), Shape.Diamond));
        Sprite crateSprite = CreateSprite("crate.png", MakeCrateTexture());
        Sprite portalSprite = CreateSprite("portal.png", MakePortalTexture());

        Bundle bundle = new Bundle
        {
            backgroundTile = CreateTile("BackgroundTile.asset", backgroundSprite),
            floorTile = CreateTile("FloorTile.asset", floorSprite),
            wallTopTile = CreateTile("WallTopTile.asset", wallTopSprite),
            wallFrontTile = CreateTile("WallFrontTile.asset", wallFrontSprite),
            wallShadowTile = CreateTile("WallShadowTile.asset", wallShadowSprite)
        };

        bundle.bulletPrefab = CreateBulletPrefab(bulletSprite);
        bundle.orbPrefab = CreateOrbPrefab(orbSprite);
        bundle.healthPickupPrefab = CreateHealthPickupPrefab(pickupSprite);
        bundle.cratePrefab = CreateCratePrefab(crateSprite);
        bundle.portalPrefab = CreatePortalPrefab(portalSprite);
        bundle.playerPrefab = CreatePlayerPrefab(playerSprite, bundle.bulletPrefab);
        bundle.walkerPrefab = CreateWalkerPrefab(walkerSprite, bundle.healthPickupPrefab);
        bundle.chargerPrefab = CreateChargerPrefab(chargerSprite, bundle.healthPickupPrefab);
        bundle.casterPrefab = CreateCasterPrefab(casterSprite, bundle.healthPickupPrefab, bundle.orbPrefab);
        bundle.finalBossPrefab = CreateFinalBossPrefab(bossSprite, bundle.orbPrefab, bundle.walkerPrefab, bundle.healthPickupPrefab);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return bundle;
    }

    static Sprite CreateSprite(string fileName, Texture2D texture)
    {
        string path = SpriteFolder + "/" + fileName;
        File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.filterMode = FilterMode.Point;
        importer.spritePixelsPerUnit = 64f;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static Tile CreateTile(string fileName, Sprite sprite)
    {
        string path = TileFolder + "/" + fileName;
        Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
        if (tile == null)
        {
            tile = ScriptableObject.CreateInstance<Tile>();
            AssetDatabase.CreateAsset(tile, path);
        }

        tile.sprite = sprite;
        tile.color = Color.white;
        tile.colliderType = Tile.ColliderType.Grid;
        EditorUtility.SetDirty(tile);
        return tile;
    }

    static Texture2D MakeBackgroundTexture()
    {
        Texture2D tex = NewTexture();
        Color baseColor = new Color(0.36f, 0.37f, 0.39f);
        Color lineColor = new Color(0.56f, 0.57f, 0.6f);
        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                bool gridLine = x < 2 || y < 2 || x >= tex.width - 2 || y >= tex.height - 2;
                tex.SetPixel(x, y, gridLine ? lineColor : baseColor);
            }
        }
        tex.Apply();
        return tex;
    }

    static Texture2D MakeFloorTexture()
    {
        Texture2D tex = NewTexture();
        Color baseColor = new Color(0.29f, 0.2f, 0.37f);
        Color edgeColor = new Color(0.43f, 0.34f, 0.52f);
        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                bool edge = x < 4 || y < 4 || x >= tex.width - 4 || y >= tex.height - 4;
                tex.SetPixel(x, y, edge ? edgeColor : baseColor);
            }
        }
        tex.Apply();
        return tex;
    }

    static Texture2D MakeWallTopTexture()
    {
        Texture2D tex = NewTexture();
        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                bool bevel = x < 5 || x > tex.width - 6 || y < 5 || y > tex.height - 6;
                Color color = bevel
                    ? new Color(0.66f, 0.68f, 0.74f)
                    : new Color(0.48f, 0.5f, 0.56f);
                tex.SetPixel(x, y, color);
            }
        }
        tex.Apply();
        return tex;
    }

    static Texture2D MakeWallFrontTexture()
    {
        Texture2D tex = NewTexture();
        Color clear = new Color(0f, 0f, 0f, 0f);
        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                if (y > 34)
                {
                    tex.SetPixel(x, y, clear);
                    continue;
                }

                bool lip = y > 26;
                Color color = lip
                    ? new Color(0.25f, 0.27f, 0.32f, 1f)
                    : new Color(0.17f, 0.19f, 0.24f, 1f);
                tex.SetPixel(x, y, color);
            }
        }
        tex.Apply();
        return tex;
    }

    static Texture2D MakeWallShadowTexture()
    {
        Texture2D tex = NewTexture();
        Color clear = new Color(0f, 0f, 0f, 0f);
        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                if (y < 40)
                {
                    tex.SetPixel(x, y, clear);
                    continue;
                }

                float t = Mathf.InverseLerp(40f, 63f, y);
                tex.SetPixel(x, y, new Color(0f, 0f, 0f, Mathf.Lerp(0.28f, 0.02f, t)));
            }
        }
        tex.Apply();
        return tex;
    }

    static Texture2D MakeCrateTexture()
    {
        Texture2D tex = NewTexture();
        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                bool border = x < 6 || y < 6 || x >= tex.width - 6 || y >= tex.height - 6;
                tex.SetPixel(x, y, border ? new Color(0.55f, 0.32f, 0.14f) : new Color(0.83f, 0.54f, 0.2f));
            }
        }
        tex.Apply();
        return tex;
    }

    static Texture2D MakePortalTexture()
    {
        Texture2D tex = NewTexture();
        Vector2 center = new Vector2(31.5f, 31.5f);
        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                Color color = dist <= 12f
                    ? new Color(0.92f, 0.72f, 1f)
                    : dist <= 22f
                        ? new Color(0.63f, 0.28f, 0.86f)
                        : dist <= 28f
                            ? new Color(0.33f, 0.08f, 0.45f)
                            : new Color(0f, 0f, 0f, 0f);
                tex.SetPixel(x, y, color);
            }
        }
        tex.Apply();
        return tex;
    }

    static Texture2D MakeBulletTexture()
    {
        Texture2D tex = NewTexture();
        Vector2 center = new Vector2(31.5f, 31.5f);
        Color clear = new Color(0f, 0f, 0f, 0f);
        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                Color color = dist <= 7f
                    ? new Color(1f, 0.97f, 0.45f)
                    : dist <= 11f
                        ? new Color(1f, 0.75f, 0.18f)
                        : clear;
                tex.SetPixel(x, y, color);
            }
        }
        tex.Apply();
        return tex;
    }

    static Texture2D MakeFlatTexture(Color fill, Shape shape)
    {
        Texture2D tex = NewTexture();
        Vector2 center = new Vector2(31.5f, 31.5f);
        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                bool draw = shape switch
                {
                    Shape.Circle => Vector2.Distance(new Vector2(x, y), center) <= 26f,
                    Shape.Diamond => Mathf.Abs(x - center.x) + Mathf.Abs(y - center.y) <= 30f,
                    _ => true
                };
                tex.SetPixel(x, y, draw ? fill : new Color(0f, 0f, 0f, 0f));
            }
        }
        tex.Apply();
        return tex;
    }

    static Texture2D NewTexture()
    {
        return new Texture2D(64, 64, TextureFormat.RGBA32, false);
    }

    static GameObject CreateBulletPrefab(Sprite sprite)
    {
        GameObject root = new GameObject("Bullet");
        ConfigureCircleBody(root, sprite, 18, 0.08f, true);
        root.transform.localScale = new Vector3(0.42f, 0.42f, 1f);
        root.GetComponent<Rigidbody2D>().collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        Bullet bullet = root.AddComponent<Bullet>();
        bullet.damage = 10;
        bullet.isPlayerBullet = true;
        return SavePrefab(root, "Bullet.prefab");
    }

    static GameObject CreateOrbPrefab(Sprite sprite)
    {
        GameObject root = new GameObject("CasterOrb");
        ConfigureCircleBody(root, sprite, 18, 0.2f, true);
        CasterOrb orb = root.AddComponent<CasterOrb>();
        orb.speed = 4f;
        orb.damage = 20;
        return SavePrefab(root, "CasterOrb.prefab");
    }

    static GameObject CreateHealthPickupPrefab(Sprite sprite)
    {
        GameObject root = new GameObject("HealthPickup");
        ConfigureCircleBody(root, sprite, 12, 0.3f, true);
        root.AddComponent<HealthPickup>();
        return SavePrefab(root, "HealthPickup.prefab");
    }

    static GameObject CreateCratePrefab(Sprite sprite)
    {
        GameObject root = new GameObject("ExplosiveCrate");
        SpriteRenderer sr = root.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 8;
        BoxCollider2D col = root.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.75f, 0.75f);
        root.AddComponent<ExplosiveCrate>();
        return SavePrefab(root, "ExplosiveCrate.prefab");
    }

    static GameObject CreatePortalPrefab(Sprite sprite)
    {
        GameObject root = new GameObject("Portal");
        ConfigureCircleBody(root, sprite, 5, 0.45f, true);
        root.AddComponent<Portal>();
        return SavePrefab(root, "Portal.prefab");
    }

    static GameObject CreatePlayerPrefab(Sprite sprite, GameObject bulletPrefab)
    {
        GameObject root = new GameObject("Player");
        root.tag = "Player";
        ConfigureCircleBody(root, sprite, 10, 0.38f, false);
        PlayerController controller = root.AddComponent<PlayerController>();
        controller.bulletPrefab = bulletPrefab;
        controller.spriteRenderer = root.GetComponent<SpriteRenderer>();
        return SavePrefab(root, "Player.prefab");
    }

    static GameObject CreateWalkerPrefab(Sprite sprite, GameObject pickupPrefab)
    {
        GameObject root = new GameObject("ShadowWalker");
        ConfigureCircleBody(root, sprite, 8, 0.38f, false);
        ShadowWalker enemy = root.AddComponent<ShadowWalker>();
        enemy.healthPickupPrefab = pickupPrefab;
        enemy.dropChance = 0.2f;
        return SavePrefab(root, "ShadowWalker.prefab");
    }

    static GameObject CreateChargerPrefab(Sprite sprite, GameObject pickupPrefab)
    {
        GameObject root = new GameObject("ShadowCharger");
        ConfigureCircleBody(root, sprite, 8, 0.42f, false);
        ShadowCharger enemy = root.AddComponent<ShadowCharger>();
        enemy.healthPickupPrefab = pickupPrefab;
        enemy.dropChance = 0.2f;
        return SavePrefab(root, "ShadowCharger.prefab");
    }

    static GameObject CreateCasterPrefab(Sprite sprite, GameObject pickupPrefab, GameObject orbPrefab)
    {
        GameObject root = new GameObject("ShadowCaster");
        ConfigureCircleBody(root, sprite, 8, 0.4f, false);
        ShadowCaster enemy = root.AddComponent<ShadowCaster>();
        enemy.healthPickupPrefab = pickupPrefab;
        enemy.dropChance = 0.2f;
        enemy.orbPrefab = orbPrefab;
        return SavePrefab(root, "ShadowCaster.prefab");
    }

    static GameObject CreateFinalBossPrefab(Sprite sprite, GameObject orbPrefab, GameObject walkerPrefab, GameObject pickupPrefab)
    {
        GameObject root = new GameObject("ShadowKing");
        ConfigureCircleBody(root, sprite, 8, 0.65f, false);
        root.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        BossController boss = root.AddComponent<BossController>();
        boss.maxHP = 300;
        boss.bossName = "Shadow King";
        boss.orbPrefab = orbPrefab;
        boss.walkerPrefab = walkerPrefab;
        boss.healthPickupPrefab = pickupPrefab;
        boss.dropChance = 0f;
        return SavePrefab(root, "ShadowKing.prefab");
    }

    static void ConfigureCircleBody(GameObject root, Sprite sprite, int sortingOrder, float radius, bool trigger)
    {
        SpriteRenderer sr = root.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = sortingOrder;
        Rigidbody2D rb = root.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        CircleCollider2D col = root.AddComponent<CircleCollider2D>();
        col.radius = radius;
        col.isTrigger = trigger;
    }

    static GameObject SavePrefab(GameObject root, string fileName)
    {
        string path = PrefabFolder + "/" + fileName;
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }
}
