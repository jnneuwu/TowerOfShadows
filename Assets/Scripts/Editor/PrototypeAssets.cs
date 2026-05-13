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
        public GameObject weaponChestPrefab;
        public Sprite lightSprite;       // radial gradient for SimpleLight
        public Sprite torchSprite;       // small torch wall sprite
        public Sprite weaponIconSprite;  // gun icon for floor pickups
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
        Sprite playerSprite = CreateSprite("player.png", MakeChibiPlayerTexture());
        Sprite gunSprite = CreateSprite("gun.png", MakeGunTexture());
        Sprite walkerSprite = CreateSprite("walker.png", MakeShadowWalkerTexture());
        Sprite chargerSprite = CreateSprite("charger.png", MakeShadowChargerTexture());
        Sprite casterSprite = CreateSprite("caster.png", MakeShadowCasterTexture());
        Sprite bossSprite = CreateSprite("boss.png", MakeShadowKingTexture());
        Sprite bulletSprite = CreateSprite("bullet.png", MakeBulletTexture());
        Sprite orbSprite = CreateSprite("orb.png", MakeFlatTexture(new Color(0.35f, 0.7f, 1f), Shape.Circle));
        Sprite pickupSprite = CreateSprite("pickup.png", MakeFlatTexture(new Color(0.2f, 1f, 0.4f), Shape.Diamond));
        Sprite crateSprite = CreateSprite("crate.png", MakeCrateTexture());
        Sprite portalSprite = CreateSprite("portal.png", MakePortalTexture());
        Sprite chestSprite = CreateSprite("chest.png", MakeChestTexture());
        Sprite lightSprite = CreateSprite("light.png", MakeLightTexture());
        Sprite torchSprite = CreateSprite("torch.png", MakeTorchTexture());
        Sprite weaponIconSprite = CreateSprite("weapon_icon.png", MakeWeaponIconTexture());

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
        bundle.playerPrefab = CreatePlayerPrefab(playerSprite, gunSprite, bundle.bulletPrefab);
        bundle.walkerPrefab = CreateWalkerPrefab(walkerSprite, bundle.healthPickupPrefab);
        bundle.chargerPrefab = CreateChargerPrefab(chargerSprite, bundle.healthPickupPrefab);
        bundle.casterPrefab = CreateCasterPrefab(casterSprite, bundle.healthPickupPrefab, bundle.orbPrefab);
        bundle.finalBossPrefab = CreateFinalBossPrefab(bossSprite, bundle.orbPrefab, bundle.walkerPrefab, bundle.healthPickupPrefab);
        bundle.weaponChestPrefab = CreateWeaponChestPrefab(chestSprite);
        bundle.lightSprite = lightSprite;
        bundle.torchSprite = torchSprite;
        bundle.weaponIconSprite = weaponIconSprite;

        // Wire static references the runtime depends on
        WeaponPickup.gunIconSprite = weaponIconSprite;
        ParticleFx.SetLightSprite(lightSprite);

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
        Color baseColor = new Color(0.06f, 0.06f, 0.08f);
        Color lineColor = new Color(0.10f, 0.10f, 0.13f);
        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                bool gridLine = x < 1 || y < 1 || x >= tex.width - 1 || y >= tex.height - 1;
                tex.SetPixel(x, y, gridLine ? lineColor : baseColor);
            }
        }
        tex.Apply();
        return tex;
    }

    static Texture2D MakeFloorTexture()
    {
        Texture2D tex = NewTexture();
        Color baseColor = new Color(0.22f, 0.24f, 0.30f);
        Color baseColor2 = new Color(0.20f, 0.22f, 0.28f);
        Color gridLine = new Color(0.16f, 0.17f, 0.22f);
        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                bool line = x < 2 || y < 2;
                bool checker = ((x / 32) + (y / 32)) % 2 == 0;
                tex.SetPixel(x, y, line ? gridLine : (checker ? baseColor : baseColor2));
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
                bool outerBevel = x < 3 || x > tex.width - 4 || y < 3 || y > tex.height - 4;
                bool innerBevel = x < 6 || x > tex.width - 7 || y < 6 || y > tex.height - 7;
                Color color;
                if (outerBevel)
                    color = new Color(0.45f, 0.42f, 0.52f);
                else if (innerBevel)
                    color = new Color(0.38f, 0.35f, 0.45f);
                else
                    color = new Color(0.30f, 0.28f, 0.38f);
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
                if (y > 40)
                {
                    tex.SetPixel(x, y, clear);
                    continue;
                }

                bool highlight = y > 34;
                bool lip = y > 24;
                Color color;
                if (highlight)
                    color = new Color(0.32f, 0.30f, 0.38f, 1f);
                else if (lip)
                    color = new Color(0.18f, 0.16f, 0.24f, 1f);
                else
                    color = new Color(0.12f, 0.11f, 0.17f, 1f);
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
                if (y < 32)
                {
                    tex.SetPixel(x, y, clear);
                    continue;
                }

                float t = Mathf.InverseLerp(32f, 63f, y);
                tex.SetPixel(x, y, new Color(0f, 0f, 0f, Mathf.Lerp(0.45f, 0.02f, t)));
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

    static GameObject CreatePlayerPrefab(Sprite bodySprite, Sprite gunSprite, GameObject bulletPrefab)
    {
        // Root: physics + controller (no SpriteRenderer, never rotates).
        GameObject root = new GameObject("Player");
        root.tag = "Player";
        Rigidbody2D rb = root.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        CircleCollider2D col = root.AddComponent<CircleCollider2D>();
        col.radius = 0.32f;
        col.isTrigger = false;

        // Body child: pixel character, always upright.
        GameObject body = new GameObject("Body");
        body.transform.SetParent(root.transform, false);
        SpriteRenderer bodySR = body.AddComponent<SpriteRenderer>();
        bodySR.sprite = bodySprite;
        bodySR.sortingOrder = 10;

        // GunPivot child: rotated toward the mouse so the gun swings around the player.
        GameObject gunPivot = new GameObject("GunPivot");
        gunPivot.transform.SetParent(root.transform, false);
        gunPivot.transform.localPosition = Vector3.zero;

        // Gun child: offset to the front-right of the pivot so it visually "points" outward.
        GameObject gun = new GameObject("Gun");
        gun.transform.SetParent(gunPivot.transform, false);
        gun.transform.localPosition = new Vector3(0.35f, -0.05f, 0f);
        SpriteRenderer gunSR = gun.AddComponent<SpriteRenderer>();
        gunSR.sprite = gunSprite;
        gunSR.sortingOrder = 11;

        // Wire up controller + state machine. Body's renderer handles the hurt-flash tint.
        PlayerController controller = root.AddComponent<PlayerController>();
        controller.bulletPrefab = bulletPrefab;
        controller.spriteRenderer = bodySR;
        controller.gunPivot = gunPivot.transform;
        root.AddComponent<PlayerStateMachine>();

        return SavePrefab(root, "Player.prefab");
    }

    // ----- Pixel-art helpers -----
    // Sprites are drawn on a 32x32 logical grid; each logical pixel becomes a 2x2 block
    // in the real 64x64 texture, giving the chunky retro look.

    static void ClearTex(Texture2D tex)
    {
        Color clear = new Color(0f, 0f, 0f, 0f);
        for (int y = 0; y < tex.height; y++)
            for (int x = 0; x < tex.width; x++)
                tex.SetPixel(x, y, clear);
    }

    /// <summary>Render one logical pixel as a 2x2 block.</summary>
    static void Px(Texture2D tex, int lx, int ly, Color c)
    {
        if (lx < 0 || ly < 0 || lx >= 32 || ly >= 32) return;
        int bx = lx * 2, by = ly * 2;
        tex.SetPixel(bx,     by,     c);
        tex.SetPixel(bx + 1, by,     c);
        tex.SetPixel(bx,     by + 1, c);
        tex.SetPixel(bx + 1, by + 1, c);
    }

    /// <summary>Draw a horizontal run of pixels in [x1, x2).</summary>
    static void PxRow(Texture2D tex, int x1, int x2, int y, Color c)
    {
        for (int x = x1; x < x2; x++) Px(tex, x, y, c);
    }

    /// <summary>Fill a rectangle of pixels in [x1, x2) x [y1, y2).</summary>
    static void PxRect(Texture2D tex, int x1, int x2, int y1, int y2, Color c)
    {
        for (int y = y1; y < y2; y++)
            for (int x = x1; x < x2; x++)
                Px(tex, x, y, c);
    }

    // Chibi pixel character, front view (the body that always stays upright).
    static Texture2D MakeChibiPlayerTexture()
    {
        Texture2D tex = NewTexture();
        ClearTex(tex);

        Color black     = new Color(0.05f, 0.05f, 0.08f);
        Color hairBrown = new Color(0.55f, 0.35f, 0.18f);
        Color skin      = new Color(0.95f, 0.65f, 0.20f); // orange body
        Color skinDark  = new Color(0.78f, 0.50f, 0.12f); // shading
        Color mouth     = new Color(0.90f, 0.30f, 0.10f);
        Color eyeBrown  = new Color(0.40f, 0.25f, 0.08f);
        Color blue      = new Color(0.28f, 0.42f, 0.65f); // blue limbs
        Color blueDark  = new Color(0.18f, 0.30f, 0.50f);
        Color whiteBelt = new Color(0.92f, 0.92f, 0.95f);
        Color tanBoots  = new Color(0.85f, 0.78f, 0.55f);

        // Hair (top of the head).
        PxRow(tex, 13, 19, 28, black);
        PxRow(tex, 12, 20, 27, black);
        PxRow(tex, 12, 20, 26, black);
        Px(tex, 13, 25, hairBrown);
        Px(tex, 17, 25, hairBrown);

        // Face.
        PxRect(tex, 12, 20, 21, 25, skin);
        Px(tex, 13, 23, eyeBrown); Px(tex, 14, 23, eyeBrown);
        Px(tex, 17, 23, eyeBrown); Px(tex, 18, 23, eyeBrown);
        Px(tex, 15, 21, mouth);    Px(tex, 16, 21, mouth);
        PxRow(tex, 18, 20, 21, skinDark);

        // Torso (orange tunic) with shaded right side.
        PxRect(tex, 13, 19, 14, 20, skin);
        PxRect(tex, 17, 19, 14, 20, skinDark);

        // Arms and hands (blue, extending sideways).
        PxRect(tex, 8, 13, 18, 20, blue);
        PxRect(tex, 19, 24, 18, 20, blue);
        PxRect(tex, 7, 9, 18, 20, blueDark);
        PxRect(tex, 23, 25, 18, 20, blueDark);

        // White belt.
        PxRow(tex, 12, 20, 13, whiteBelt);

        // Blue legs/pants, split down the middle.
        PxRect(tex, 13, 19, 8, 13, blue);
        Px(tex, 15, 8, blueDark);
        Px(tex, 16, 8, blueDark);

        // Small white strip above the boots.
        Px(tex, 13, 7, whiteBelt); Px(tex, 14, 7, whiteBelt);
        Px(tex, 17, 7, whiteBelt); Px(tex, 18, 7, whiteBelt);

        // Tan boots.
        PxRect(tex, 13, 15, 5, 7, tanBoots);
        PxRect(tex, 17, 19, 5, 7, tanBoots);

        tex.Apply();
        return tex;
    }

    // Standalone pistol sprite parented to GunPivot.
    // Default orientation is +X (right); GunPivot rotates so the gun swings around the player.
    static Texture2D MakeGunTexture()
    {
        Texture2D tex = NewTexture();
        ClearTex(tex);

        Color barrel    = new Color(0.18f, 0.18f, 0.22f);
        Color barrelLit = new Color(0.45f, 0.45f, 0.50f);
        Color barrelDk  = new Color(0.06f, 0.06f, 0.10f);
        Color grip      = new Color(0.60f, 0.40f, 0.18f);
        Color gripDk    = new Color(0.40f, 0.25f, 0.08f);

        // Horizontal barrel with a light top edge and dark bottom edge.
        PxRect(tex, 8, 26, 15, 18, barrel);
        PxRow(tex, 9, 25, 17, barrelLit);
        PxRow(tex, 8, 26, 14, barrelDk);
        Px(tex, 26, 16, barrelDk);
        Px(tex, 26, 15, barrelDk);

        // Wooden grip dropping below the barrel.
        PxRect(tex, 11, 15, 11, 15, grip);
        PxRect(tex, 14, 15, 11, 15, gripDk);
        PxRect(tex, 11, 15, 11, 12, gripDk);

        tex.Apply();
        return tex;
    }

    // ShadowWalker - floating purple ghost with two glowing eyes.
    static Texture2D MakeShadowWalkerTexture()
    {
        Texture2D tex = NewTexture();
        ClearTex(tex);

        Color ghost     = new Color(0.45f, 0.25f, 0.65f); // main purple
        Color ghostDk   = new Color(0.30f, 0.15f, 0.50f); // shadow
        Color ghostLt   = new Color(0.60f, 0.40f, 0.85f); // highlight
        Color eyeWhite  = new Color(0.95f, 0.95f, 1f);
        Color eyePupil  = new Color(0.05f, 0.02f, 0.10f);

        // Round head dome.
        PxRow(tex, 13, 19, 29, ghost);
        PxRow(tex, 11, 21, 28, ghost);
        PxRow(tex, 10, 22, 27, ghost);
        PxRow(tex, 10, 22, 26, ghost);
        PxRow(tex, 13, 17, 28, ghostLt);

        // Trapezoidal body widening downward.
        for (int y = 14; y < 26; y++)
        {
            int width = 12 + (25 - y) / 2;
            int x1 = 16 - width / 2;
            int x2 = 16 + (width + 1) / 2;
            PxRow(tex, x1, x2, y, ghost);
        }

        // Wispy bottom edge (three small arcs).
        PxRow(tex, 9, 13, 13, ghost);
        PxRow(tex, 14, 18, 13, ghost);
        PxRow(tex, 19, 23, 13, ghost);
        Px(tex, 10, 12, ghost); Px(tex, 11, 12, ghost);
        Px(tex, 15, 12, ghost); Px(tex, 16, 12, ghost);
        Px(tex, 20, 12, ghost); Px(tex, 21, 12, ghost);
        PxRow(tex, 9, 11, 13, ghostDk);
        PxRow(tex, 21, 23, 13, ghostDk);

        // Glowing white eyes with black pupils.
        PxRect(tex, 12, 15, 22, 25, eyeWhite);
        PxRect(tex, 17, 20, 22, 25, eyeWhite);
        Px(tex, 13, 23, eyePupil); Px(tex, 14, 23, eyePupil);
        Px(tex, 18, 23, eyePupil); Px(tex, 19, 23, eyePupil);

        tex.Apply();
        return tex;
    }

    // ShadowCharger - red horned beast that charges at the player.
    static Texture2D MakeShadowChargerTexture()
    {
        Texture2D tex = NewTexture();
        ClearTex(tex);

        Color body     = new Color(0.75f, 0.20f, 0.25f); // dark red
        Color bodyDk   = new Color(0.50f, 0.10f, 0.15f);
        Color bodyLt   = new Color(0.95f, 0.35f, 0.40f);
        Color hornBone = new Color(0.85f, 0.78f, 0.55f); // bone-tan horns
        Color hornDk   = new Color(0.55f, 0.45f, 0.25f);
        Color eyeRed   = new Color(1f, 0.85f, 0.20f);    // glowing yellow eyes

        // Two long horns on top.
        Px(tex, 11, 29, hornBone); Px(tex, 11, 28, hornBone); Px(tex, 12, 27, hornBone);
        Px(tex, 20, 29, hornBone); Px(tex, 20, 28, hornBone); Px(tex, 19, 27, hornBone);
        Px(tex, 12, 26, hornDk); Px(tex, 13, 26, hornBone);
        Px(tex, 18, 26, hornBone); Px(tex, 19, 26, hornDk);

        // Head with a lighter top edge.
        PxRect(tex, 11, 21, 22, 26, body);
        PxRow(tex, 12, 20, 26, body);
        PxRow(tex, 12, 20, 25, bodyLt);

        // Glowing eyes and angry V-shaped brow.
        Px(tex, 13, 23, eyeRed); Px(tex, 14, 23, eyeRed);
        Px(tex, 17, 23, eyeRed); Px(tex, 18, 23, eyeRed);
        Px(tex, 12, 24, bodyDk); Px(tex, 14, 24, bodyDk);
        Px(tex, 17, 24, bodyDk); Px(tex, 19, 24, bodyDk);

        // Stocky body with bottom + left shadow.
        PxRect(tex, 10, 22, 14, 22, body);
        PxRow(tex, 10, 22, 14, bodyDk);
        PxRect(tex, 10, 12, 14, 22, bodyDk);

        // Four stubby legs with darker hooves.
        PxRect(tex, 11, 13, 10, 14, body);
        PxRect(tex, 14, 16, 10, 14, body);
        PxRect(tex, 17, 19, 10, 14, body);
        PxRect(tex, 19, 21, 10, 14, body);
        PxRow(tex, 11, 13, 10, hornDk);
        PxRow(tex, 14, 16, 10, hornDk);
        PxRow(tex, 17, 19, 10, hornDk);
        PxRow(tex, 19, 21, 10, hornDk);

        tex.Apply();
        return tex;
    }

    // ShadowCaster - hooded blue wizard with glowing eyes that fires orbs.
    static Texture2D MakeShadowCasterTexture()
    {
        Texture2D tex = NewTexture();
        ClearTex(tex);

        Color robe     = new Color(0.20f, 0.35f, 0.65f); // dark-blue robe
        Color robeDk   = new Color(0.10f, 0.20f, 0.45f);
        Color robeLt   = new Color(0.30f, 0.50f, 0.85f);
        Color hood     = new Color(0.10f, 0.18f, 0.35f); // near-black hood
        Color hoodTip  = new Color(0.05f, 0.10f, 0.25f);
        Color faceDark = new Color(0.05f, 0.05f, 0.10f); // shadow under the hood
        Color eyeGlow  = new Color(0.4f, 0.95f, 1f);     // glowing blue eyes

        // Pointed hood tip.
        Px(tex, 16, 30, hoodTip);
        PxRow(tex, 15, 18, 29, hood);
        PxRow(tex, 14, 19, 28, hood);
        PxRow(tex, 13, 20, 27, hood);

        // Hood sides hanging down.
        PxRow(tex, 12, 21, 26, hood);
        PxRow(tex, 11, 22, 25, hood);
        PxRow(tex, 11, 22, 24, hood);
        PxRow(tex, 11, 22, 23, hood);
        PxRow(tex, 11, 22, 22, hood);

        // Dark area inside the hood + the two glowing eyes peeking through.
        PxRect(tex, 13, 20, 22, 26, faceDark);
        Px(tex, 14, 24, eyeGlow); Px(tex, 15, 24, eyeGlow);
        Px(tex, 17, 24, eyeGlow); Px(tex, 18, 24, eyeGlow);

        // Robe body (trapezoid).
        for (int y = 12; y < 22; y++)
        {
            int width = 10 + (21 - y) / 3;
            int x1 = 16 - width / 2;
            int x2 = 16 + (width + 1) / 2;
            PxRow(tex, x1, x2, y, robe);
        }
        // Bright vertical trim down the centre of the robe.
        for (int y = 12; y <= 18; y++) PxRow(tex, 16, 17, y, robeLt);
        // Darker hem at the bottom.
        PxRow(tex, 9, 23, 11, robeDk);
        PxRow(tex, 9, 23, 10, robeDk);

        tex.Apply();
        return tex;
    }

    // ShadowKing (final boss) - dark-purple king with a gold crown and red eyes.
    static Texture2D MakeShadowKingTexture()
    {
        Texture2D tex = NewTexture();
        ClearTex(tex);

        Color crownGold = new Color(1f, 0.82f, 0.20f);    // gold crown
        Color crownDk   = new Color(0.65f, 0.50f, 0.10f);
        Color body      = new Color(0.30f, 0.10f, 0.45f); // dark-purple body
        Color bodyDk    = new Color(0.18f, 0.05f, 0.32f);
        Color bodyLt    = new Color(0.50f, 0.22f, 0.65f);
        Color cape      = new Color(0.18f, 0.05f, 0.30f); // black-purple cape
        Color faceShadow= new Color(0.10f, 0.05f, 0.18f);
        Color eyeRed    = new Color(1f, 0.20f, 0.20f);    // glowing red eyes
        Color jewel     = new Color(0.95f, 0.20f, 0.50f); // crown jewel

        // Crown spikes + horizontal band + central jewel.
        PxRect(tex, 11, 13, 28, 30, crownGold);
        PxRect(tex, 15, 17, 28, 30, crownGold);
        PxRect(tex, 19, 21, 28, 30, crownGold);
        PxRect(tex, 10, 22, 26, 28, crownGold);
        PxRow(tex, 10, 22, 26, crownDk);
        Px(tex, 16, 27, jewel);

        // Wide purple face with a lighter highlight row.
        PxRect(tex, 10, 22, 21, 26, body);
        PxRow(tex, 10, 22, 26, bodyDk);
        PxRow(tex, 11, 21, 25, bodyLt);

        // Glowing red eyes and angry brow.
        Px(tex, 12, 23, eyeRed); Px(tex, 13, 23, eyeRed); Px(tex, 14, 23, eyeRed);
        Px(tex, 17, 23, eyeRed); Px(tex, 18, 23, eyeRed); Px(tex, 19, 23, eyeRed);
        Px(tex, 12, 24, bodyDk); Px(tex, 14, 24, bodyDk);
        Px(tex, 17, 24, bodyDk); Px(tex, 19, 24, bodyDk);

        // Beard.
        PxRect(tex, 11, 21, 19, 22, faceShadow);
        Px(tex, 13, 18, faceShadow); Px(tex, 14, 18, faceShadow);
        Px(tex, 17, 18, faceShadow); Px(tex, 18, 18, faceShadow);

        // Royal robe with a brighter strip down the centre.
        PxRect(tex, 9, 23, 13, 19, body);
        for (int y = 13; y <= 17; y++) PxRow(tex, 15, 17, y, bodyLt);

        // Cape spreading out and below + small dark fringe.
        PxRect(tex, 7, 25, 8, 13, cape);
        PxRect(tex, 8, 24, 5, 8, cape);
        Px(tex, 7, 7, bodyDk); Px(tex, 24, 7, bodyDk);
        Px(tex, 8, 5, bodyDk); Px(tex, 23, 5, bodyDk);

        tex.Apply();
        return tex;
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
        boss.maxHP = 800;                  // raised from 300 for more challenging fight
        boss.contactDamage = 25;            // touching the boss really hurts now
        boss.bossName = "Shadow King";
        boss.orbPrefab = orbPrefab;
        boss.walkerPrefab = walkerPrefab;
        boss.healthPickupPrefab = pickupPrefab;
        boss.dropChance = 0f;
        return SavePrefab(root, "ShadowKing.prefab");
    }

    static Texture2D MakeChestTexture()
    {
        Texture2D tex = NewTexture();
        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                bool outside = x < 6 || x >= 58 || y < 10 || y >= 54;
                bool border = x < 10 || x >= 54 || y < 14 || y >= 50;
                bool latch = x >= 26 && x < 38 && y >= 28 && y < 38;
                Color color;
                if (outside)
                    color = new Color(0f, 0f, 0f, 0f);
                else if (latch)
                    color = new Color(1f, 0.85f, 0.3f);
                else if (border)
                    color = new Color(0.6f, 0.42f, 0.15f);
                else
                    color = new Color(0.85f, 0.6f, 0.2f);
                tex.SetPixel(x, y, color);
            }
        }
        tex.Apply();
        return tex;
    }

    static GameObject CreateWeaponChestPrefab(Sprite sprite)
    {
        GameObject root = new GameObject("WeaponChest");
        SpriteRenderer sr = root.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 8;
        root.AddComponent<WeaponChest>();
        return SavePrefab(root, "WeaponChest.prefab");
    }

    // Lab 6: light overlay sprite (radial gradient, white -> transparent).
    static Texture2D MakeLightTexture()
    {
        Texture2D tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        Vector2 c = new Vector2(64f, 64f);
        for (int y = 0; y < 128; y++)
        {
            for (int x = 0; x < 128; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), c) / 64f;
                float a = Mathf.Clamp01(1f - d);
                a = a * a; // soften the falloff (squared falloff = nicer light)
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        return tex;
    }

    // Lab 6: small wall-mounted torch sprite (used as a static light source).
    static Texture2D MakeTorchTexture()
    {
        Texture2D tex = NewTexture();
        ClearTex(tex);
        Color woodDk  = new Color(0.30f, 0.18f, 0.08f);
        Color wood    = new Color(0.55f, 0.32f, 0.14f);
        Color flame1  = new Color(1.00f, 0.75f, 0.20f);
        Color flame2  = new Color(1.00f, 0.45f, 0.10f);
        Color flame3  = new Color(0.95f, 0.95f, 0.55f);
        // Wooden handle (vertical)
        PxRect(tex, 14, 18, 4, 18, wood);
        PxRect(tex, 14, 16, 4, 18, woodDk);
        // Bowl
        PxRect(tex, 12, 20, 17, 21, woodDk);
        // Flame layers
        PxRect(tex, 13, 19, 21, 24, flame2);
        PxRect(tex, 14, 18, 22, 27, flame1);
        PxRect(tex, 15, 17, 24, 29, flame3);
        Px(tex, 16, 30, flame3);
        tex.Apply();
        return tex;
    }

    // Weapon pickup icon: a clearly readable gun silhouette.
    // White body + dark outline so it stands out on every background.
    static Texture2D MakeWeaponIconTexture()
    {
        Texture2D tex = NewTexture();
        ClearTex(tex);
        Color body    = new Color(0.95f, 0.95f, 1.0f);
        Color outline = new Color(0.06f, 0.06f, 0.10f);
        Color grip    = new Color(0.55f, 0.35f, 0.18f);
        Color gripDk  = new Color(0.30f, 0.18f, 0.08f);

        // Long barrel (horizontal, takes most of the sprite)
        PxRect(tex, 6, 28, 14, 19, body);
        // Outline top + bottom
        PxRow(tex, 6, 28, 19, outline);
        PxRow(tex, 6, 28, 13, outline);
        // Outline left + right
        for (int y = 13; y < 20; y++) { Px(tex, 5, y, outline); Px(tex, 28, y, outline); }
        // Front sight (small bump on top of muzzle)
        Px(tex, 25, 20, outline); Px(tex, 26, 20, outline);
        Px(tex, 25, 21, body);    Px(tex, 26, 21, body);
        // Trigger guard (under barrel, middle)
        PxRow(tex, 14, 18, 12, outline);
        Px(tex, 14, 11, outline); Px(tex, 18, 11, outline);
        // Grip (wooden, slants down-right)
        PxRect(tex, 12, 17, 7, 13, grip);
        // Grip outline
        for (int y = 7; y < 13; y++) { Px(tex, 11, y, outline); Px(tex, 17, y, outline); }
        PxRow(tex, 12, 17, 6, outline);
        // Grip shading
        PxRect(tex, 15, 17, 7, 13, gripDk);

        tex.Apply();
        return tex;
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
