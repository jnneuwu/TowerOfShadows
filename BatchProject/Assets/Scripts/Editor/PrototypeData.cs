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
            Vector2[] crates)
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
        }
    }

    public static readonly LevelDef[] Levels =
    {
        new LevelDef(
            "Floor1",
            1,
            new Vector2(-28f, 0f),
            new Vector2(38f, -1f),
            new[]
            {
                new RoomDef("Spawn", new RectInt(-32, -4, 8, 8)),
                new RoomDef("RoomA", new RectInt(-20, 4, 10, 10)),
                new RoomDef("RoomB", new RectInt(-6, -12, 12, 12)),
                new RoomDef("RoomC", new RectInt(12, 4, 10, 10)),
                new RoomDef("Boss", new RectInt(26, -10, 18, 18), true),
            },
            new[]
            {
                new CorridorDef(new Vector2Int(-24, 0), new Vector2Int(-20, 9), 3),
                new CorridorDef(new Vector2Int(-10, 9), new Vector2Int(-6, -6), 3),
                new CorridorDef(new Vector2Int(6, -6), new Vector2Int(12, 9), 3),
                new CorridorDef(new Vector2Int(22, 9), new Vector2Int(26, -1), 3),
            },
            new[]
            {
                new DoorDef("RoomA", new Vector2(-8f, 9f)),
                new DoorDef("RoomB", new Vector2(8f, -6f)),
                new DoorDef("RoomC", new Vector2(24f, 9f)),
            },
            new[]
            {
                new EnemySpawn("RoomA", EnemyKind.Walker, new Vector2(-17f, 7f)),
                new EnemySpawn("RoomA", EnemyKind.Walker, new Vector2(-13f, 11f)),
                new EnemySpawn("RoomA", EnemyKind.Charger, new Vector2(-12f, 7.5f)),
                new EnemySpawn("RoomB", EnemyKind.Walker, new Vector2(-3f, -9f)),
                new EnemySpawn("RoomB", EnemyKind.Walker, new Vector2(2f, -3f)),
                new EnemySpawn("RoomB", EnemyKind.Charger, new Vector2(0f, -6f)),
                new EnemySpawn("RoomC", EnemyKind.Walker, new Vector2(15f, 7f)),
                new EnemySpawn("RoomC", EnemyKind.Charger, new Vector2(18f, 11f)),
                new EnemySpawn("RoomC", EnemyKind.Caster, new Vector2(20f, 8f)),
                new EnemySpawn("Boss", EnemyKind.Floor1Boss, new Vector2(35f, -1f)),
                new EnemySpawn("Boss", EnemyKind.Walker, new Vector2(31f, 3f)),
                new EnemySpawn("Boss", EnemyKind.Walker, new Vector2(31f, -4f)),
                new EnemySpawn("Boss", EnemyKind.Charger, new Vector2(39f, 2f)),
            },
            new[]
            {
                new RectInt(-17, 7, 2, 2),
                new RectInt(-14, 10, 2, 2),
                new RectInt(-2, -9, 2, 2),
                new RectInt(1, -5, 2, 2),
                new RectInt(15, 7, 2, 2),
                new RectInt(18, 10, 2, 2),
                new RectInt(31, -6, 2, 2),
                new RectInt(36, 2, 2, 2),
            },
            new[]
            {
                new Vector2(-1f, -10f),
                new Vector2(17f, 6f),
                new Vector2(39f, -6f),
            }),

        new LevelDef(
            "Floor2",
            2,
            new Vector2(-30f, 0f),
            new Vector2(60f, -2f),
            new[]
            {
                new RoomDef("Spawn", new RectInt(-34, -4, 8, 8)),
                new RoomDef("RoomA", new RectInt(-22, -12, 12, 12)),
                new RoomDef("RoomB", new RectInt(-6, 4, 12, 12)),
                new RoomDef("RoomC", new RectInt(12, -12, 14, 12)),
                new RoomDef("RoomD", new RectInt(31, 5, 12, 12)),
                new RoomDef("Boss", new RectInt(46, -12, 20, 20), true),
            },
            new[]
            {
                new CorridorDef(new Vector2Int(-26, 0), new Vector2Int(-22, -6), 3),
                new CorridorDef(new Vector2Int(-10, -6), new Vector2Int(-6, 10), 3),
                new CorridorDef(new Vector2Int(6, 10), new Vector2Int(12, -6), 3),
                new CorridorDef(new Vector2Int(26, -6), new Vector2Int(31, 11), 3),
                new CorridorDef(new Vector2Int(43, 11), new Vector2Int(46, -2), 3),
            },
            new[]
            {
                new DoorDef("RoomA", new Vector2(-8f, -6f)),
                new DoorDef("RoomB", new Vector2(8f, 10f)),
                new DoorDef("RoomC", new Vector2(28f, -6f)),
                new DoorDef("RoomD", new Vector2(44f, 11f)),
            },
            new[]
            {
                new EnemySpawn("RoomA", EnemyKind.Walker, new Vector2(-19f, -9f)),
                new EnemySpawn("RoomA", EnemyKind.Walker, new Vector2(-13f, -3f)),
                new EnemySpawn("RoomA", EnemyKind.Charger, new Vector2(-15f, -6f)),
                new EnemySpawn("RoomB", EnemyKind.Walker, new Vector2(-3f, 7f)),
                new EnemySpawn("RoomB", EnemyKind.Charger, new Vector2(1f, 12f)),
                new EnemySpawn("RoomB", EnemyKind.Caster, new Vector2(3f, 8f)),
                new EnemySpawn("RoomC", EnemyKind.Walker, new Vector2(15f, -9f)),
                new EnemySpawn("RoomC", EnemyKind.Charger, new Vector2(20f, -4f)),
                new EnemySpawn("RoomC", EnemyKind.Caster, new Vector2(23f, -8f)),
                new EnemySpawn("RoomD", EnemyKind.Walker, new Vector2(34f, 8f)),
                new EnemySpawn("RoomD", EnemyKind.Charger, new Vector2(38f, 14f)),
                new EnemySpawn("RoomD", EnemyKind.Caster, new Vector2(40f, 9f)),
                new EnemySpawn("Boss", EnemyKind.Floor2Boss, new Vector2(56f, -2f)),
                new EnemySpawn("Boss", EnemyKind.Charger, new Vector2(51f, 4f)),
                new EnemySpawn("Boss", EnemyKind.Charger, new Vector2(51f, -7f)),
                new EnemySpawn("Boss", EnemyKind.Caster, new Vector2(60f, 5f)),
            },
            new[]
            {
                new RectInt(-19, -9, 2, 2),
                new RectInt(-14, -4, 2, 2),
                new RectInt(-2, 7, 2, 2),
                new RectInt(2, 11, 2, 2),
                new RectInt(15, -9, 2, 2),
                new RectInt(20, -5, 2, 2),
                new RectInt(34, 8, 2, 2),
                new RectInt(39, 13, 2, 2),
                new RectInt(52, -8, 2, 2),
                new RectInt(59, 4, 2, 2),
            },
            new[]
            {
                new Vector2(-16f, -10f),
                new Vector2(0f, 6f),
                new Vector2(22f, -10f),
                new Vector2(58f, 6f),
            }),

        new LevelDef(
            "Floor3",
            3,
            new Vector2(-32f, 0f),
            new Vector2(66f, -2f),
            new[]
            {
                new RoomDef("Spawn", new RectInt(-36, -4, 8, 8)),
                new RoomDef("RoomA", new RectInt(-24, 6, 12, 12)),
                new RoomDef("RoomB", new RectInt(-8, -14, 14, 14)),
                new RoomDef("RoomC", new RectInt(12, 6, 14, 14)),
                new RoomDef("RoomD", new RectInt(31, -14, 14, 14)),
                new RoomDef("Boss", new RectInt(50, -14, 24, 24), true),
            },
            new[]
            {
                new CorridorDef(new Vector2Int(-28, 0), new Vector2Int(-24, 12), 3),
                new CorridorDef(new Vector2Int(-12, 12), new Vector2Int(-8, -7), 3),
                new CorridorDef(new Vector2Int(6, -7), new Vector2Int(12, 12), 3),
                new CorridorDef(new Vector2Int(26, 12), new Vector2Int(31, -7), 3),
                new CorridorDef(new Vector2Int(45, -7), new Vector2Int(50, -2), 3),
            },
            new[]
            {
                new DoorDef("RoomA", new Vector2(-10f, 12f)),
                new DoorDef("RoomB", new Vector2(8f, -7f)),
                new DoorDef("RoomC", new Vector2(28f, 12f)),
                new DoorDef("RoomD", new Vector2(47f, -7f)),
            },
            new[]
            {
                new EnemySpawn("RoomA", EnemyKind.Walker, new Vector2(-21f, 9f)),
                new EnemySpawn("RoomA", EnemyKind.Charger, new Vector2(-16f, 14f)),
                new EnemySpawn("RoomA", EnemyKind.Caster, new Vector2(-14f, 9f)),
                new EnemySpawn("RoomB", EnemyKind.Caster, new Vector2(-4f, -10f)),
                new EnemySpawn("RoomB", EnemyKind.Caster, new Vector2(2f, -4f)),
                new EnemySpawn("RoomB", EnemyKind.Charger, new Vector2(-1f, -7f)),
                new EnemySpawn("RoomC", EnemyKind.Walker, new Vector2(15f, 9f)),
                new EnemySpawn("RoomC", EnemyKind.Charger, new Vector2(20f, 15f)),
                new EnemySpawn("RoomC", EnemyKind.Caster, new Vector2(23f, 10f)),
                new EnemySpawn("RoomD", EnemyKind.Walker, new Vector2(34f, -10f)),
                new EnemySpawn("RoomD", EnemyKind.Charger, new Vector2(40f, -4f)),
                new EnemySpawn("RoomD", EnemyKind.Caster, new Vector2(42f, -11f)),
                new EnemySpawn("Boss", EnemyKind.FinalBoss, new Vector2(62f, -2f)),
                new EnemySpawn("Boss", EnemyKind.Charger, new Vector2(56f, 5f)),
                new EnemySpawn("Boss", EnemyKind.Charger, new Vector2(56f, -9f)),
                new EnemySpawn("Boss", EnemyKind.Caster, new Vector2(68f, 5f)),
                new EnemySpawn("Boss", EnemyKind.Caster, new Vector2(68f, -9f)),
            },
            new[]
            {
                new RectInt(-21, 9, 2, 2),
                new RectInt(-16, 14, 2, 2),
                new RectInt(-4, -10, 2, 2),
                new RectInt(1, -5, 2, 2),
                new RectInt(15, 9, 2, 2),
                new RectInt(21, 14, 2, 2),
                new RectInt(34, -10, 2, 2),
                new RectInt(40, -4, 2, 2),
                new RectInt(56, -9, 2, 2),
                new RectInt(67, 4, 2, 2),
            },
            new[]
            {
                new Vector2(-2f, -12f),
                new Vector2(18f, 8f),
                new Vector2(36f, -12f),
                new Vector2(70f, -11f),
            }),
    };
}
