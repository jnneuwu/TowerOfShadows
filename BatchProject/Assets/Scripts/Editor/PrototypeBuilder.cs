using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public static class PrototypeBuilder
{
    const string SceneFolder = "Assets/Scenes";
    const string BuildFolder = "Build/Windows";

    [MenuItem("Tower of Shadows/Prototype/Generate Content")]
    public static void GenerateContent()
    {
        EnsureProjectFolders();
        EnsureTagsAndLayers();
        PrototypeAssets.Bundle bundle = PrototypeAssets.Build();

        CreateMainMenuScene();
        foreach (PrototypeData.LevelDef level in PrototypeData.Levels)
        {
            CreateLevelScene(level, bundle);
        }
        CreateVictoryScene();
        ApplyBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(SceneFolder + "/MainMenu.unity");
    }

    [MenuItem("Tower of Shadows/Prototype/Build Windows Zip")]
    public static void BuildWindowsZip()
    {
        GenerateContent();

        if (!Directory.Exists(BuildFolder))
        {
            Directory.CreateDirectory(BuildFolder);
        }

        string exePath = Path.Combine(BuildFolder, "TowerOfShadows.exe");
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[]
            {
                SceneFolder + "/MainMenu.unity",
                SceneFolder + "/Floor1.unity",
                SceneFolder + "/Floor2.unity",
                SceneFolder + "/Floor3.unity",
                SceneFolder + "/Victory.unity",
            },
            locationPathName = exePath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new System.Exception("Windows build failed.");
        }

        string zipPath = Path.Combine("Build", "TowerOfShadows_Windows.zip");
        if (File.Exists(zipPath)) File.Delete(zipPath);
        ZipFile.CreateFromDirectory(BuildFolder, zipPath);
        AssetDatabase.Refresh();
    }

    static void EnsureProjectFolders()
    {
        EnsureFolder("Assets", "Scenes");
        EnsureFolder("Assets", "Prefabs");
        EnsureFolder("Assets", "Sprites");
    }

    static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }

    static void EnsureTagsAndLayers()
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if (assets == null || assets.Length == 0) return;
        SerializedObject tagManager = new SerializedObject(assets[0]);
        AddTag(tagManager.FindProperty("tags"), "Player");
        AddTag(tagManager.FindProperty("tags"), "Wall");
        EnsureLayer(tagManager.FindProperty("layers"), "Wall");
        tagManager.ApplyModifiedPropertiesWithoutUndo();
    }

    static void AddTag(SerializedProperty tags, string tag)
    {
        for (int i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        }

        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
    }

    static void EnsureLayer(SerializedProperty layers, string layerName)
    {
        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(i);
            if (layer.stringValue == layerName) return;
            if (string.IsNullOrEmpty(layer.stringValue))
            {
                layer.stringValue = layerName;
                return;
            }
        }
    }

    static void CreateMainMenuScene()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera(new Color(0.08f, 0.08f, 0.1f), false);
        CreateEventSystem();
        Canvas canvas = CreateCanvas("MainMenuCanvas");

        GameObject actionsObject = new GameObject("MenuActions");
        MenuActions actions = actionsObject.AddComponent<MenuActions>();

        CreateCenteredText(canvas.transform, "TOWER OF SHADOWS", new Vector2(0f, 300f), 64, new Vector2(960f, 100f), Color.white);
        CreateCenteredText(canvas.transform, "3-floor prototype dungeon run", new Vector2(0f, 244f), 24, new Vector2(520f, 42f), new Color(0.94f, 0.88f, 0.7f));

        GameObject missionPanel = CreatePanel(canvas.transform, "MissionPanel", new Vector2(0.5f, 0.5f), new Vector2(-280f, 35f), new Vector2(470f, 350f), new Color(0f, 0f, 0f, 0.48f));
        CreateText(missionPanel.transform, "MISSION", new Vector2(24f, -28f), new Vector2(360f, 40f), TextAnchor.MiddleLeft, 28, new Vector2(0f, 1f), Color.white);
        CreateText(missionPanel.transform, "Play through 3 floors and clear rooms.", new Vector2(24f, -86f), new Vector2(400f, 34f), TextAnchor.MiddleLeft, 22, new Vector2(0f, 1f), new Color(0.88f, 0.9f, 0.95f));
        CreateText(missionPanel.transform, "Defeat the boss to open the portal.", new Vector2(24f, -132f), new Vector2(400f, 34f), TextAnchor.MiddleLeft, 22, new Vector2(0f, 1f), new Color(0.88f, 0.9f, 0.95f));
        CreateText(missionPanel.transform, "Walk into the portal to reach the next floor.", new Vector2(24f, -178f), new Vector2(420f, 34f), TextAnchor.MiddleLeft, 22, new Vector2(0f, 1f), new Color(0.88f, 0.9f, 0.95f));
        CreateText(missionPanel.transform, "Controls: WASD move | Mouse aim | Left Click shoot | ESC pause", new Vector2(24f, -246f), new Vector2(420f, 60f), TextAnchor.UpperLeft, 20, new Vector2(0f, 1f), new Color(0.95f, 0.84f, 0.42f));

        GameObject legendPanel = CreatePanel(canvas.transform, "LegendPanel", new Vector2(0.5f, 0.5f), new Vector2(280f, 35f), new Vector2(470f, 350f), new Color(0f, 0f, 0f, 0.48f));
        CreateText(legendPanel.transform, "LEGEND", new Vector2(24f, -28f), new Vector2(360f, 40f), TextAnchor.MiddleLeft, 28, new Vector2(0f, 1f), Color.white);
        CreateLegendRow(legendPanel.transform, new Vector2(24f, -88f), new Color(0.15f, 0.9f, 1f), "Cyan circle: player placeholder.");
        CreateLegendRow(legendPanel.transform, new Vector2(24f, -136f), new Color(0.53f, 0.33f, 0.8f), "Purple circle: enemy placeholder.");
        CreateLegendRow(legendPanel.transform, new Vector2(24f, -184f), new Color(0.83f, 0.54f, 0.2f), "Yellow square: explosive bomb crate.");
        CreateLegendRow(legendPanel.transform, new Vector2(24f, -232f), new Color(1f, 0.78f, 0.34f), "Yellow ring: blast radius for enemies and player.");
        CreateLegendRow(legendPanel.transform, new Vector2(24f, -280f), new Color(0.63f, 0.28f, 0.86f), "Purple portal and gray walls: exit and solid collision.");

        Button start = CreateButton(canvas.transform, "PLAY", new Vector2(0f, -248f));
        Button quit = CreateButton(canvas.transform, "QUIT", new Vector2(0f, -326f));
        UnityEventTools.AddPersistentListener(start.onClick, actions.StartGame);
        UnityEventTools.AddPersistentListener(quit.onClick, actions.QuitGame);

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), SceneFolder + "/MainMenu.unity");
    }

    static void CreateVictoryScene()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera(new Color(0.06f, 0.04f, 0.08f), false);
        CreateEventSystem();
        Canvas canvas = CreateCanvas("VictoryCanvas");

        GameObject actionsObject = new GameObject("MenuActions");
        MenuActions actions = actionsObject.AddComponent<MenuActions>();

        CreateCenteredText(canvas.transform, "VICTORY", new Vector2(0f, 160f), 60, new Vector2(800f, 120f), new Color(1f, 0.88f, 0.25f));
        CreateCenteredText(canvas.transform, "The prototype tower has been cleared.", new Vector2(0f, 85f), 24, new Vector2(900f, 60f), Color.white);
        Button restart = CreateButton(canvas.transform, "RESTART", new Vector2(0f, -5f));
        Button menu = CreateButton(canvas.transform, "MAIN MENU", new Vector2(0f, -85f));
        UnityEventTools.AddPersistentListener(restart.onClick, actions.RestartGame);
        UnityEventTools.AddPersistentListener(menu.onClick, actions.LoadMainMenu);

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), SceneFolder + "/Victory.unity");
    }

    static void CreateLevelScene(PrototypeData.LevelDef level, PrototypeAssets.Bundle bundle)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Camera camera = CreateCamera(new Color(0.05f, 0.05f, 0.07f), true);
        CreateEventSystem();

        GameObject managers = new GameObject("PersistentManagers");
        GameManager gm = managers.AddComponent<GameManager>();
        gm.currentFloor = level.FloorIndex;
        gm.totalFloors = 3;
        managers.AddComponent<AudioManager>();

        BuildTilemap(level, bundle);

        GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(bundle.playerPrefab);
        player.name = "Player";
        player.transform.position = level.PlayerSpawn;
        camera.GetComponent<CameraFollow>().target = player.transform;

        Canvas canvas = CreateCanvas("HUDCanvas");
        BuildGameplayUi(canvas.transform);

        GameObject portal = (GameObject)PrefabUtility.InstantiatePrefab(bundle.portalPrefab);
        portal.name = "Portal";
        portal.transform.position = level.PortalSpawn;

        Dictionary<string, RoomManager> roomManagers = CreateRoomManagers(level, portal.GetComponent<Portal>());
        Dictionary<string, RoomDoor> doors = CreateDoors(level, bundle.wallTopTile.sprite);
        foreach (PrototypeData.DoorDef door in level.Doors)
        {
            roomManagers[door.RoomKey].doorsToOpen.Add(doors[door.RoomKey]);
        }

        foreach (PrototypeData.EnemySpawn spawn in level.Enemies)
        {
            GameObject enemy = CreateEnemy(spawn.Kind, bundle);
            enemy.transform.position = spawn.Position;
            enemy.transform.SetParent(roomManagers[spawn.RoomKey].transform);
        }

        foreach (Vector2 cratePos in level.Crates)
        {
            GameObject crate = (GameObject)PrefabUtility.InstantiatePrefab(bundle.cratePrefab);
            crate.transform.position = cratePos;
        }

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), SceneFolder + "/" + level.SceneName + ".unity");
    }

    static Dictionary<string, RoomManager> CreateRoomManagers(PrototypeData.LevelDef level, Portal portal)
    {
        Dictionary<string, RoomManager> map = new Dictionary<string, RoomManager>();
        foreach (PrototypeData.RoomDef room in level.Rooms)
        {
            if (room.Key == "Spawn") continue;
            GameObject root = new GameObject(room.Key);
            RoomManager manager = root.AddComponent<RoomManager>();
            manager.doorsToOpen = new List<RoomDoor>();
            manager.isBossRoom = room.IsBossRoom;
            if (room.IsBossRoom) manager.portal = portal;
            map[room.Key] = manager;
        }
        return map;
    }

    static Dictionary<string, RoomDoor> CreateDoors(PrototypeData.LevelDef level, Sprite sprite)
    {
        Dictionary<string, RoomDoor> map = new Dictionary<string, RoomDoor>();
        int wallLayer = LayerMask.NameToLayer("Wall");
        const float closedThickness = 0.9f;
        const float passageSpan = 3.4f;
        foreach (PrototypeData.DoorDef door in level.Doors)
        {
            GameObject doorObject = new GameObject(door.RoomKey + "_Door");
            doorObject.transform.position = door.Position;
            SpriteRenderer sr = doorObject.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(0.2f, 0.8f, 0.3f, 1f);
            sr.sortingOrder = 4;
            doorObject.transform.localScale = door.Horizontal
                ? new Vector3(passageSpan, closedThickness, 1f)
                : new Vector3(closedThickness, passageSpan, 1f);
            BoxCollider2D col = doorObject.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
            if (wallLayer >= 0) doorObject.layer = wallLayer;
            map[door.RoomKey] = doorObject.AddComponent<RoomDoor>();
        }
        return map;
    }

    static GameObject CreateEnemy(PrototypeData.EnemyKind kind, PrototypeAssets.Bundle bundle)
    {
        GameObject enemy = kind switch
        {
            PrototypeData.EnemyKind.Walker => (GameObject)PrefabUtility.InstantiatePrefab(bundle.walkerPrefab),
            PrototypeData.EnemyKind.Charger => (GameObject)PrefabUtility.InstantiatePrefab(bundle.chargerPrefab),
            PrototypeData.EnemyKind.Caster => (GameObject)PrefabUtility.InstantiatePrefab(bundle.casterPrefab),
            PrototypeData.EnemyKind.FinalBoss => (GameObject)PrefabUtility.InstantiatePrefab(bundle.finalBossPrefab),
            PrototypeData.EnemyKind.Floor1Boss => (GameObject)PrefabUtility.InstantiatePrefab(bundle.chargerPrefab),
            _ => (GameObject)PrefabUtility.InstantiatePrefab(bundle.casterPrefab),
        };

        if (kind == PrototypeData.EnemyKind.Floor1Boss)
        {
            enemy.name = "ShadowGuardian";
            enemy.transform.localScale = new Vector3(1.4f, 1.4f, 1f);
            ShadowCharger boss = enemy.GetComponent<ShadowCharger>();
            boss.maxHP = 140;
            boss.currentHP = 140;
            boss.chargeDamage = 35;
            boss.chargeCooldown = 2.2f;
            boss.isFloorBoss = true;
        }
        else if (kind == PrototypeData.EnemyKind.Floor2Boss)
        {
            enemy.name = "ShadowArchon";
            enemy.transform.localScale = new Vector3(1.45f, 1.45f, 1f);
            ShadowCaster boss = enemy.GetComponent<ShadowCaster>();
            boss.maxHP = 180;
            boss.currentHP = 180;
            boss.fireInterval = 1.6f;
            boss.orbDamage = 25;
            boss.orbSpeed = 5.5f;
            boss.isFloorBoss = true;
        }
        else if (kind == PrototypeData.EnemyKind.FinalBoss)
        {
            BossController boss = enemy.GetComponent<BossController>();
            boss.isFloorBoss = true;
        }

        return enemy;
    }

    static void BuildTilemap(PrototypeData.LevelDef level, PrototypeAssets.Bundle bundle)
    {
        HashSet<Vector3Int> floorCells = new HashSet<Vector3Int>();
        foreach (PrototypeData.RoomDef room in level.Rooms) FillRect(floorCells, room.Area);
        foreach (PrototypeData.CorridorDef corridor in level.Corridors) FillCorridor(floorCells, corridor);

        foreach (RectInt blocker in level.Blockers)
        {
            for (int x = blocker.xMin; x < blocker.xMax; x++)
            for (int y = blocker.yMin; y < blocker.yMax; y++)
                floorCells.Remove(new Vector3Int(x, y, 0));
        }

        HashSet<Vector3Int> wallCells = new HashSet<Vector3Int>();
        foreach (Vector3Int floor in floorCells)
        {
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                Vector3Int candidate = new Vector3Int(floor.x + dx, floor.y + dy, 0);
                if (!floorCells.Contains(candidate)) wallCells.Add(candidate);
            }
        }

        foreach (RectInt blocker in level.Blockers) FillRect(wallCells, blocker);

        HashSet<Vector3Int> frontFaceCells = new HashSet<Vector3Int>();
        HashSet<Vector3Int> shadowCells = new HashSet<Vector3Int>();
        foreach (Vector3Int wall in wallCells)
        {
            Vector3Int below = new Vector3Int(wall.x, wall.y - 1, 0);
            if (!floorCells.Contains(below)) continue;
            frontFaceCells.Add(wall);
            shadowCells.Add(below);
        }

        int minX = int.MaxValue;
        int maxX = int.MinValue;
        int minY = int.MaxValue;
        int maxY = int.MinValue;
        foreach (Vector3Int cell in wallCells)
        {
            if (cell.x < minX) minX = cell.x;
            if (cell.x > maxX) maxX = cell.x;
            if (cell.y < minY) minY = cell.y;
            if (cell.y > maxY) maxY = cell.y;
        }

        RectInt backgroundRect = new RectInt(minX - 4, minY - 4, (maxX - minX) + 9, (maxY - minY) + 9);
        HashSet<Vector3Int> backgroundCells = new HashSet<Vector3Int>();
        FillRect(backgroundCells, backgroundRect);

        GameObject geometryObject = new GameObject("RuntimeLevelGeometry");
        RuntimeLevelGeometry runtimeGeometry = geometryObject.AddComponent<RuntimeLevelGeometry>();
        runtimeGeometry.backgroundTile = bundle.backgroundTile;
        runtimeGeometry.floorTile = bundle.floorTile;
        runtimeGeometry.wallShadowTile = bundle.wallShadowTile;
        runtimeGeometry.wallTopTile = bundle.wallTopTile;
        runtimeGeometry.wallFrontTile = bundle.wallFrontTile;
        runtimeGeometry.backgroundCells = ToSortedArray(backgroundCells);
        runtimeGeometry.floorCells = ToSortedArray(floorCells);
        runtimeGeometry.shadowCells = ToSortedArray(shadowCells);
        runtimeGeometry.wallCells = ToSortedArray(wallCells);
        runtimeGeometry.frontFaceCells = ToSortedArray(frontFaceCells);
        EditorUtility.SetDirty(runtimeGeometry);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    static void FillRect(HashSet<Vector3Int> set, RectInt rect)
    {
        for (int x = rect.xMin; x < rect.xMax; x++)
        for (int y = rect.yMin; y < rect.yMax; y++)
            set.Add(new Vector3Int(x, y, 0));
    }

    static void FillCorridor(HashSet<Vector3Int> set, PrototypeData.CorridorDef corridor)
    {
        int half = Mathf.Max(1, corridor.Width / 2);
        int minX = Mathf.Min(corridor.From.x, corridor.To.x);
        int maxX = Mathf.Max(corridor.From.x, corridor.To.x);
        int minY = Mathf.Min(corridor.From.y, corridor.To.y);
        int maxY = Mathf.Max(corridor.From.y, corridor.To.y);
        FillRect(set, new RectInt(minX, corridor.From.y - half, maxX - minX + 1, corridor.Width));
        FillRect(set, new RectInt(corridor.To.x - half, minY, corridor.Width, maxY - minY + 1));
    }

    static Vector3Int[] ToSortedArray(HashSet<Vector3Int> cells)
    {
        List<Vector3Int> ordered = new List<Vector3Int>(cells);
        ordered.Sort((a, b) =>
        {
            int yCompare = a.y.CompareTo(b.y);
            if (yCompare != 0) return yCompare;
            int xCompare = a.x.CompareTo(b.x);
            if (xCompare != 0) return xCompare;
            return a.z.CompareTo(b.z);
        });
        return ordered.ToArray();
    }

    static Camera CreateCamera(Color background, bool followPlayer)
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 8f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = background;
        cameraObject.AddComponent<AudioListener>();
        if (followPlayer) cameraObject.AddComponent<CameraFollow>();
        return camera;
    }

    static Canvas CreateCanvas(string name)
    {
        GameObject canvasObject = new GameObject(name);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    static void CreateEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null) return;
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    static void BuildGameplayUi(Transform canvas)
    {
        GameObject uiObject = new GameObject("UIManager", typeof(RectTransform));
        uiObject.transform.SetParent(canvas, false);
        RectTransform uiRect = uiObject.GetComponent<RectTransform>();
        uiRect.anchorMin = Vector2.zero;
        uiRect.anchorMax = Vector2.one;
        uiRect.pivot = new Vector2(0.5f, 0.5f);
        uiRect.offsetMin = Vector2.zero;
        uiRect.offsetMax = Vector2.zero;
        UIManager ui = uiObject.AddComponent<UIManager>();
        ui.hpText = CreateText(uiObject.transform, "100 / 100", new Vector2(-48f, -34f), new Vector2(220f, 30f), TextAnchor.MiddleRight, 24, new Vector2(1f, 1f), Color.white);
        ui.hpSlider = CreateSlider(uiObject.transform, new Vector2(-48f, -68f), new Vector2(420f, 28f), new Color(0.2f, 0.85f, 0.3f), new Vector2(1f, 1f));
        ui.ammoText = CreateText(uiObject.transform, "AMMO: 50 / 50", new Vector2(48f, 42f), new Vector2(360f, 42f), TextAnchor.MiddleLeft, 32, new Vector2(0f, 0f), new Color(0.96f, 0.96f, 1f));
        ui.floorText = CreateText(uiObject.transform, "FLOOR 1 / 3", new Vector2(0f, -34f), new Vector2(260f, 32f), TextAnchor.MiddleCenter, 24, new Vector2(0.5f, 1f), new Color(0.9f, 0.88f, 0.72f));
        ui.bossHPPanel = CreatePanel(uiObject.transform, "BossPanel", new Vector2(0.5f, 1f), new Vector2(0f, -58f), new Vector2(520f, 96f), new Color(0f, 0f, 0f, 0.52f));
        ui.bossNameText = CreateText(ui.bossHPPanel.transform, "SHADOW KING", new Vector2(0f, 20f), new Vector2(340f, 30f), TextAnchor.MiddleCenter, 24, new Vector2(0.5f, 0.5f));
        ui.bossHPSlider = CreateSlider(ui.bossHPPanel.transform, new Vector2(0f, -14f), new Vector2(380f, 24f), new Color(0.9f, 0.2f, 0.25f), new Vector2(0.5f, 0.5f));
        ui.pausePanel = BuildPausePanel(uiObject.transform, ui);
        ui.deathPanel = BuildDeathPanel(uiObject.transform, ui);
        ui.pausePanel.SetActive(false);
        ui.deathPanel.SetActive(false);
        ui.bossHPPanel.SetActive(false);
    }

    static GameObject BuildPausePanel(Transform parent, UIManager ui)
    {
        GameObject panel = CreateFullPanel(parent, "PausePanel", new Color(0f, 0f, 0f, 0.75f));
        CreateText(panel.transform, "PAUSED", new Vector2(0f, 120f), new Vector2(420f, 70f), TextAnchor.MiddleCenter, 48, new Vector2(0.5f, 0.5f));
        Button resume = CreateButton(panel.transform, "RESUME", new Vector2(0f, 30f));
        Button restart = CreateButton(panel.transform, "RESTART", new Vector2(0f, -45f));
        Button menu = CreateButton(panel.transform, "MAIN MENU", new Vector2(0f, -120f));
        UnityEventTools.AddPersistentListener(resume.onClick, ui.OnResumeClick);
        UnityEventTools.AddPersistentListener(restart.onClick, ui.OnRestartClick);
        UnityEventTools.AddPersistentListener(menu.onClick, ui.OnMainMenuClick);
        return panel;
    }

    static GameObject BuildDeathPanel(Transform parent, UIManager ui)
    {
        GameObject panel = CreateFullPanel(parent, "DeathPanel", new Color(0f, 0f, 0f, 0.8f));
        CreateText(panel.transform, "YOU DIED", new Vector2(0f, 120f), new Vector2(420f, 70f), TextAnchor.MiddleCenter, 52, new Vector2(0.5f, 0.5f), new Color(1f, 0.3f, 0.3f));
        Button restart = CreateButton(panel.transform, "RESTART", new Vector2(0f, 20f));
        Button menu = CreateButton(panel.transform, "MAIN MENU", new Vector2(0f, -55f));
        UnityEventTools.AddPersistentListener(restart.onClick, ui.OnRestartClick);
        UnityEventTools.AddPersistentListener(menu.onClick, ui.OnMainMenuClick);
        return panel;
    }

    static Button CreateButton(Transform parent, string label, Vector2 anchoredPos)
    {
        GameObject obj = DefaultControls.CreateButton(new DefaultControls.Resources());
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = new Vector2(260f, 56f);
        Text text = obj.GetComponentInChildren<Text>();
        text.text = label;
        text.font = GetBuiltinFont();
        return obj.GetComponent<Button>();
    }

    static void CreateLegendRow(Transform parent, Vector2 anchoredPos, Color swatchColor, string textValue)
    {
        GameObject row = new GameObject("LegendRow", typeof(RectTransform));
        row.transform.SetParent(parent, false);
        RectTransform rowRect = row.GetComponent<RectTransform>();
        rowRect.anchorMin = rowRect.anchorMax = rowRect.pivot = new Vector2(0f, 1f);
        rowRect.anchoredPosition = anchoredPos;
        rowRect.sizeDelta = new Vector2(420f, 34f);

        CreatePanel(row.transform, "Swatch", new Vector2(0f, 0.5f), new Vector2(14f, 0f), new Vector2(22f, 22f), swatchColor);
        CreateText(row.transform, textValue, new Vector2(44f, 0f), new Vector2(360f, 34f), TextAnchor.MiddleLeft, 18, new Vector2(0f, 0.5f), new Color(0.9f, 0.92f, 0.96f));
    }

    static Text CreateText(Transform parent, string value, Vector2 anchoredPos, Vector2 size, TextAnchor anchor, int fontSize, Vector2 pivotAnchor, Color? color = null)
    {
        GameObject obj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = pivotAnchor;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;
        Text text = obj.GetComponent<Text>();
        text.font = GetBuiltinFont();
        text.text = value;
        text.fontSize = fontSize;
        text.alignment = anchor;
        text.color = color ?? Color.white;
        return text;
    }

    static GameObject CreatePanel(Transform parent, string name, Vector2 anchor, Vector2 anchoredPos, Vector2 size, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;
        Image image = obj.GetComponent<Image>();
        image.color = color;
        return obj;
    }

    static GameObject CreateFullPanel(Transform parent, string name, Color color)
    {
        GameObject obj = CreatePanel(parent, name, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, color);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        return obj;
    }

    static Slider CreateSlider(Transform parent, Vector2 anchoredPos, Vector2 size, Color fillColor, Vector2 anchor)
    {
        GameObject obj = DefaultControls.CreateSlider(new DefaultControls.Resources());
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;
        Slider slider = obj.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        obj.transform.Find("Fill Area/Fill").GetComponent<Image>().color = fillColor;
        return slider;
    }

    static void CreateCenteredText(Transform parent, string value, Vector2 anchoredPos, int fontSize, Vector2 size, Color color)
    {
        CreateText(parent, value, anchoredPos, size, TextAnchor.MiddleCenter, fontSize, new Vector2(0.5f, 0.5f), color);
    }

    static Font GetBuiltinFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return font;
    }

    static void ApplyBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(SceneFolder + "/MainMenu.unity", true),
            new EditorBuildSettingsScene(SceneFolder + "/Floor1.unity", true),
            new EditorBuildSettingsScene(SceneFolder + "/Floor2.unity", true),
            new EditorBuildSettingsScene(SceneFolder + "/Floor3.unity", true),
            new EditorBuildSettingsScene(SceneFolder + "/Victory.unity", true),
        };
    }
}
