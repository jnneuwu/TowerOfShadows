using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteAlways]
[DisallowMultipleComponent]
public class RuntimeLevelGeometry : MonoBehaviour
{
    [Header("Tiles")]
    public Tile backgroundTile;
    public Tile floorTile;
    public Tile wallTopTile;
    public Tile wallFrontTile;
    public Tile wallShadowTile;

    [Header("Cells")]
    public Vector3Int[] backgroundCells;
    public Vector3Int[] floorCells;
    public Vector3Int[] wallCells;
    public Vector3Int[] frontFaceCells;
    public Vector3Int[] shadowCells;

    bool built;

    void Awake()
    {
        Build();
    }

    void OnEnable()
    {
        Build();
    }

    public void Build()
    {
        if (built && transform.Find("Grid") != null) return;
        built = true;

        Transform existingGrid = transform.Find("Grid");
        if (existingGrid != null)
        {
            if (Application.isPlaying) Destroy(existingGrid.gameObject);
            else DestroyImmediate(existingGrid.gameObject);
        }

        GameObject gridObject = new GameObject("Grid");
        gridObject.transform.SetParent(transform, false);
        Grid grid = gridObject.AddComponent<Grid>();
        grid.cellSize = Vector3.one;

        Tilemap backgroundTilemap = CreateTilemap(gridObject.transform, "Background");
        Tilemap floorTilemap = CreateTilemap(gridObject.transform, "Floor");
        Tilemap wallShadowTilemap = CreateTilemap(gridObject.transform, "WallShadow");
        Tilemap wallTopTilemap = CreateTilemap(gridObject.transform, "WallTop");
        Tilemap wallFrontTilemap = CreateTilemap(gridObject.transform, "WallFront");

        FillTiles(backgroundTilemap, backgroundCells, backgroundTile);
        FillTiles(floorTilemap, floorCells, floorTile);
        FillTiles(wallShadowTilemap, shadowCells, wallShadowTile);
        FillTiles(wallTopTilemap, wallCells, wallTopTile);
        FillTiles(wallFrontTilemap, frontFaceCells, wallFrontTile);

        backgroundTilemap.CompressBounds();
        floorTilemap.CompressBounds();
        wallShadowTilemap.CompressBounds();
        wallTopTilemap.CompressBounds();
        wallFrontTilemap.CompressBounds();

        backgroundTilemap.RefreshAllTiles();
        floorTilemap.RefreshAllTiles();
        wallShadowTilemap.RefreshAllTiles();
        wallTopTilemap.RefreshAllTiles();
        wallFrontTilemap.RefreshAllTiles();

        backgroundTilemap.GetComponent<TilemapRenderer>().sortingOrder = -6;
        floorTilemap.GetComponent<TilemapRenderer>().sortingOrder = 0;
        wallShadowTilemap.GetComponent<TilemapRenderer>().sortingOrder = 2;
        wallTopTilemap.GetComponent<TilemapRenderer>().sortingOrder = 8;
        wallFrontTilemap.GetComponent<TilemapRenderer>().sortingOrder = 14;

        TilemapCollider2D collider = wallTopTilemap.gameObject.AddComponent<TilemapCollider2D>();
        collider.usedByComposite = true;
        Rigidbody2D rb = wallTopTilemap.gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        wallTopTilemap.gameObject.AddComponent<CompositeCollider2D>();

        int wallLayer = LayerMask.NameToLayer("Wall");
        if (wallLayer >= 0) wallTopTilemap.gameObject.layer = wallLayer;
    }

    static void FillTiles(Tilemap tilemap, Vector3Int[] cells, Tile tile)
    {
        if (tilemap == null || tile == null || cells == null) return;

        for (int i = 0; i < cells.Length; i++)
        {
            tilemap.SetTile(cells[i], tile);
        }
    }

    static Tilemap CreateTilemap(Transform parent, string name)
    {
        GameObject tilemapObject = new GameObject(name);
        tilemapObject.transform.SetParent(parent, false);
        Tilemap tilemap = tilemapObject.AddComponent<Tilemap>();
        TilemapRenderer renderer = tilemapObject.AddComponent<TilemapRenderer>();
        renderer.sortOrder = TilemapRenderer.SortOrder.TopRight;
        return tilemap;
    }
}
