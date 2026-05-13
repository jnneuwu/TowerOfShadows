using UnityEngine;

public static class PrototypeData
{
    public enum EnemyKind
    {
        Walker,
        Charger,
        Caster,
        Floor1Boss,
        Floor2Boss,
        FinalBoss
    }

    public readonly struct RoomDef
    {
        public readonly string Key;
        public readonly RectInt Area;
        public readonly bool IsBossRoom;

        public RoomDef(string key, RectInt area, bool isBossRoom = false)
        {
            Key = key;
            Area = area;
            IsBossRoom = isBossRoom;
        }
    }

    public readonly struct CorridorDef
    {
        public readonly Vector2Int From;
        public readonly Vector2Int To;
        public readonly int Width;

        public CorridorDef(Vector2Int from, Vector2Int to, int width = 3)
        {
            From = from;
            To = to;
            Width = width;
        }
    }

    public readonly struct DoorDef
    {
        public readonly string RoomKey;
        public readonly Vector2 Position;
        public readonly bool Horizontal;

        public DoorDef(string roomKey, Vector2 position, bool horizontal = false)
        {
            RoomKey = roomKey;
            Position = position;
            Horizontal = horizontal;
        }
    }

    public readonly struct EnemySpawn
    {
        public readonly string RoomKey;
        public readonly EnemyKind Kind;
        public readonly Vector2 Position;

        public EnemySpawn(string roomKey, EnemyKind kind, Vector2 position)
        {
            RoomKey = roomKey;
            Kind = kind;
            Position = position;
        }
    }

    public readonly struct ChestSpawn
    {
        public readonly Vector2 Position;
        public readonly int WeaponIndex;

        public ChestSpawn(Vector2 position, int weaponIndex)
        {
            Position = position;
            WeaponIndex = weaponIndex;
        }
    }

    public readonly struct LevelDef
    {
        public readonly string SceneName;
        public readonly int FloorIndex;
        public readonly Vector2 PlayerSpawn;
        public readonly Vector2 PortalSpawn;
        public readonly RoomDef[] Rooms;
        public readonly CorridorDef[] Corridors;
        public readonly DoorDef[] Doors;
        public readonly EnemySpawn[] Enemies;
        public readonly RectInt[] Blockers;
        public readonly Vector2[] Crates;
        public readonly ChestSpawn[] Chests;

        public LevelDef(
            string sceneName,
            int floorIndex,
            Vector2 playerSpawn,
            Vector2 portalSpawn,
            RoomDef[] rooms,
            CorridorDef[] corridors,
            DoorDef[] doors,
            EnemySpawn[] enemies,
            RectInt[] blockers,
            Vector2[] crates,
            ChestSpawn[] chests)
        {
            SceneName = sceneName;
            FloorIndex = floorIndex;
            PlayerSpawn = playerSpawn;
            PortalSpawn = portalSpawn;
            Rooms = rooms;
            Corridors = corridors;
            Doors = doors;
            Enemies = enemies;
            Blockers = blockers;
            Crates = crates;
            Chests = chests;
        }
    }

    // Floor layouts:
    //   Floor 1 "The Descent"    - diagonal path, small rooms, easy
    //   Floor 2 "The Crossroads" - vertical hub-and-spoke layout
    //   Floor 3 "The Abyss"      - wide ring/loop, many rooms, hard

    public static readonly LevelDef[] Levels =
    {
        // Floor 1 - diagonal path: Spawn(top-left) -> RoomA -> RoomB -> Boss(far right)
        new LevelDef(
            "Floor1",
            1,
            new Vector2(-8f, 8f),       // player in spawn
            new Vector2(38f, -14f),      // portal in boss
            new[]
            {
                new RoomDef("Spawn",  new RectInt(-12, 5, 8, 8)),
                new RoomDef("RoomA",  new RectInt(-1, -3, 12, 12)),
                new RoomDef("RoomB",  new RectInt(18, -12, 10, 10)),
                new RoomDef("Boss",   new RectInt(32, -20, 14, 14), true),
            },
            new[]
            {
                new CorridorDef(new Vector2Int(-4, 8),  new Vector2Int(-1, 3), 3),
                new CorridorDef(new Vector2Int(11, 3),  new Vector2Int(18, -6), 3),
                new CorridorDef(new Vector2Int(28, -6), new Vector2Int(32, -13), 3),
            },
            new[]
            {
                new DoorDef("RoomA", new Vector2(12f, 3f)),
                new DoorDef("RoomB", new Vector2(29f, -6f)),
            },
            new[]
            {
                new EnemySpawn("RoomA", EnemyKind.Walker,  new Vector2(3f, 1f)),
                new EnemySpawn("RoomA", EnemyKind.Walker,  new Vector2(7f, 5f)),
                new EnemySpawn("RoomA", EnemyKind.Charger, new Vector2(5f, 3f)),
                new EnemySpawn("RoomB", EnemyKind.Walker,  new Vector2(21f, -5f)),
                new EnemySpawn("RoomB", EnemyKind.Charger, new Vector2(25f, -8f)),
                new EnemySpawn("RoomB", EnemyKind.Caster,  new Vector2(23f, -4f)),
                new EnemySpawn("Boss",  EnemyKind.Floor1Boss, new Vector2(39f, -13f)),
                new EnemySpawn("Boss",  EnemyKind.Walker,  new Vector2(35f, -10f)),
                new EnemySpawn("Boss",  EnemyKind.Walker,  new Vector2(35f, -16f)),
            },
            new[]
            {
                new RectInt(4, 5, 2, 2),
                new RectInt(8, 0, 2, 2),
                new RectInt(21, -9, 2, 2),
                new RectInt(37, -18, 2, 2),
            },
            new[]
            {
                new Vector2(2f, 6f),
                new Vector2(24f, -3f),
                new Vector2(42f, -9f),
            },
            new[]
            {
                new ChestSpawn(new Vector2(9f, 7f), 1),     // Machine Gun
                new ChestSpawn(new Vector2(26f, -7f), 5),   // Frost Gun
            }),

        // FLOOR 2 - Vertical hub with branches (cross shape)
        // Spawn(bottom) -> Hub(center) -> Left/Right branches -> Top -> Boss(far top)
        new LevelDef(
            "Floor2",
            2,
            new Vector2(0f, -22f),
            new Vector2(0f, 38f),
            new[]
            {
                new RoomDef("Spawn",  new RectInt(-4, -26, 8, 8)),
                new RoomDef("Hub",    new RectInt(-6, -8, 12, 12)),
                new RoomDef("RoomA",  new RectInt(-24, -6, 12, 12)),   // left
                new RoomDef("RoomB",  new RectInt(12, -6, 12, 12)),    // right
                new RoomDef("RoomC",  new RectInt(-6, 12, 12, 12)),    // top
                new RoomDef("Boss",   new RectInt(-10, 30, 20, 18), true),
            },
            new[]
            {
                new CorridorDef(new Vector2Int(0, -18), new Vector2Int(0, -8), 3),
                new CorridorDef(new Vector2Int(-6, -2), new Vector2Int(-24, -2), 3),
                new CorridorDef(new Vector2Int(6, -2),  new Vector2Int(12, -2), 3),
                new CorridorDef(new Vector2Int(0, 4),   new Vector2Int(0, 12), 3),
                new CorridorDef(new Vector2Int(0, 24),  new Vector2Int(0, 30), 3),
            },
            new[]
            {
                new DoorDef("Hub", new Vector2(-12f, -2f)),
                new DoorDef("Hub", new Vector2(12f, -2f)),
                new DoorDef("Hub", new Vector2(0f, 4f), true),
                new DoorDef("RoomC", new Vector2(0f, 24f), true),
            },
            new[]
            {
                // Hub (light enemies)
                new EnemySpawn("Hub",   EnemyKind.Walker,  new Vector2(-1f, -5f)),
                new EnemySpawn("Hub",   EnemyKind.Walker,  new Vector2(4f, 1f)),
                // Left room
                new EnemySpawn("RoomA", EnemyKind.Walker,  new Vector2(-20f, -2f)),
                new EnemySpawn("RoomA", EnemyKind.Charger, new Vector2(-16f, 2f)),
                new EnemySpawn("RoomA", EnemyKind.Caster,  new Vector2(-18f, 3f)),
                // Right room
                new EnemySpawn("RoomB", EnemyKind.Walker,  new Vector2(16f, -2f)),
                new EnemySpawn("RoomB", EnemyKind.Charger, new Vector2(21f, 0f)),
                new EnemySpawn("RoomB", EnemyKind.Caster,  new Vector2(18f, -4f)),
                // Top room
                new EnemySpawn("RoomC", EnemyKind.Charger, new Vector2(-2f, 16f)),
                new EnemySpawn("RoomC", EnemyKind.Caster,  new Vector2(3f, 20f)),
                new EnemySpawn("RoomC", EnemyKind.Walker,  new Vector2(-3f, 21f)),
                // Boss
                new EnemySpawn("Boss",  EnemyKind.Floor2Boss, new Vector2(0f, 39f)),
                new EnemySpawn("Boss",  EnemyKind.Charger, new Vector2(-5f, 35f)),
                new EnemySpawn("Boss",  EnemyKind.Charger, new Vector2(5f, 35f)),
                new EnemySpawn("Boss",  EnemyKind.Caster,  new Vector2(-4f, 45f)),
                new EnemySpawn("Boss",  EnemyKind.Caster,  new Vector2(8f, 44f)),
            },
            new[]
            {
                new RectInt(-3, -4, 2, 2),
                new RectInt(2, 0, 2, 2),
                new RectInt(-20, 2, 2, 2),
                new RectInt(19, 2, 2, 2),
                new RectInt(-3, 19, 2, 2),
                new RectInt(3, 15, 2, 2),
                new RectInt(-7, 42, 2, 2),
                new RectInt(5, 34, 2, 2),
            },
            new[]
            {
                new Vector2(-15f, -4f),
                new Vector2(15f, -4f),
                new Vector2(2f, 22f),
                new Vector2(7f, 44f),
            },
            new[]
            {
                new ChestSpawn(new Vector2(-21f, 4f), 2),   // Shotgun
                new ChestSpawn(new Vector2(21f, 4f), 4),    // Flame Gun
                new ChestSpawn(new Vector2(2f, 14f), 3),    // Sniper
            }),

        // FLOOR 3 - Wide open layout with loop path
        // Spawn(left) -> split top/bottom paths -> converge -> Boss(far right, huge)
        new LevelDef(
            "Floor3",
            3,
            new Vector2(-30f, 0f),
            new Vector2(52f, 0f),
            new[]
            {
                new RoomDef("Spawn",  new RectInt(-34, -4, 8, 8)),
                new RoomDef("RoomA",  new RectInt(-20, 10, 14, 12)),    // upper-left
                new RoomDef("RoomB",  new RectInt(-20, -22, 14, 12)),   // lower-left
                new RoomDef("RoomC",  new RectInt(2, 10, 14, 14)),      // upper-right
                new RoomDef("RoomD",  new RectInt(2, -24, 14, 14)),     // lower-right
                new RoomDef("RoomE",  new RectInt(22, -6, 12, 12)),     // center-right convergence
                new RoomDef("Boss",   new RectInt(40, -14, 24, 24), true),
            },
            new[]
            {
                // Spawn splits into top and bottom
                new CorridorDef(new Vector2Int(-26, 0),  new Vector2Int(-20, 16), 3),
                new CorridorDef(new Vector2Int(-26, 0),  new Vector2Int(-20, -16), 3),
                // Top path
                new CorridorDef(new Vector2Int(-6, 16),  new Vector2Int(2, 16), 3),
                // Bottom path
                new CorridorDef(new Vector2Int(-6, -16), new Vector2Int(2, -16), 3),
                // Converge to center-right
                new CorridorDef(new Vector2Int(16, 16),  new Vector2Int(22, 0), 3),
                new CorridorDef(new Vector2Int(16, -16), new Vector2Int(22, 0), 3),
                // To boss
                new CorridorDef(new Vector2Int(34, 0),   new Vector2Int(40, 0), 3),
            },
            new[]
            {
                new DoorDef("RoomA", new Vector2(-5f, 16f)),
                new DoorDef("RoomB", new Vector2(-5f, -16f)),
                new DoorDef("RoomC", new Vector2(17f, 16f)),
                new DoorDef("RoomD", new Vector2(17f, -16f)),
                new DoorDef("RoomE", new Vector2(35f, 0f)),
            },
            new[]
            {
                // Upper-left
                new EnemySpawn("RoomA", EnemyKind.Walker,  new Vector2(-16f, 14f)),
                new EnemySpawn("RoomA", EnemyKind.Charger, new Vector2(-10f, 18f)),
                new EnemySpawn("RoomA", EnemyKind.Caster,  new Vector2(-12f, 14f)),
                // Lower-left
                new EnemySpawn("RoomB", EnemyKind.Walker,  new Vector2(-16f, -18f)),
                new EnemySpawn("RoomB", EnemyKind.Charger, new Vector2(-10f, -14f)),
                new EnemySpawn("RoomB", EnemyKind.Caster,  new Vector2(-14f, -14f)),
                // Upper-right
                new EnemySpawn("RoomC", EnemyKind.Charger, new Vector2(6f, 14f)),
                new EnemySpawn("RoomC", EnemyKind.Caster,  new Vector2(12f, 18f)),
                new EnemySpawn("RoomC", EnemyKind.Caster,  new Vector2(8f, 20f)),
                // Lower-right
                new EnemySpawn("RoomD", EnemyKind.Charger, new Vector2(6f, -18f)),
                new EnemySpawn("RoomD", EnemyKind.Caster,  new Vector2(10f, -16f)),
                new EnemySpawn("RoomD", EnemyKind.Caster,  new Vector2(8f, -20f)),
                // Center convergence
                new EnemySpawn("RoomE", EnemyKind.Walker,  new Vector2(25f, -2f)),
                new EnemySpawn("RoomE", EnemyKind.Charger, new Vector2(30f, 2f)),
                new EnemySpawn("RoomE", EnemyKind.Caster,  new Vector2(28f, 4f)),
                // Boss
                new EnemySpawn("Boss",  EnemyKind.FinalBoss, new Vector2(52f, 0f)),
                new EnemySpawn("Boss",  EnemyKind.Charger, new Vector2(47f, 6f)),
                new EnemySpawn("Boss",  EnemyKind.Charger, new Vector2(47f, -6f)),
                new EnemySpawn("Boss",  EnemyKind.Caster,  new Vector2(57f, 6f)),
                new EnemySpawn("Boss",  EnemyKind.Caster,  new Vector2(57f, -6f)),
            },
            new[]
            {
                new RectInt(-15, 17, 2, 2),
                new RectInt(-9, 12, 2, 2),
                new RectInt(-15, -19, 2, 2),
                new RectInt(-9, -14, 2, 2),
                new RectInt(5, 19, 2, 2),
                new RectInt(12, 13, 2, 2),
                new RectInt(5, -21, 2, 2),
                new RectInt(12, -15, 2, 2),
                new RectInt(48, -8, 2, 2),
                new RectInt(56, 4, 2, 2),
            },
            new[]
            {
                new Vector2(-8f, 18f),
                new Vector2(-8f, -18f),
                new Vector2(14f, 12f),
                new Vector2(14f, -12f),
                new Vector2(58f, -8f),
            },
            new[]
            {
                new ChestSpawn(new Vector2(-18f, 20f), 6),   // Laser Gun
                new ChestSpawn(new Vector2(-18f, -20f), 2),  // Shotgun
                new ChestSpawn(new Vector2(30f, -2f), 4),    // Flame Gun
            }),
    };
}
