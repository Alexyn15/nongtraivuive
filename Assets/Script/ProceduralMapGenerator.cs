using UnityEngine;
using UnityEngine.Tilemaps;

public class ProceduralMapGenerator : MonoBehaviour
{
    [Header("Map Size")]
    public int width = 50;   // số ô theo trục X
    public int height = 50;  // số ô theo trục Y

    [Header("Tiles")]
    public TileBase defaultTile;      // tile mặc định (vd. cỏ)
    public TileBase[] randomTiles;    // các tile khác để random (vd. đất, đá...)

    [Header("Random Settings")]
    public int seed = 0;
    public bool useRandomSeed = true;

    private Tilemap tilemap;

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }

    private void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        if (tilemap == null)
        {
            tilemap = GetComponent<Tilemap>();
        }

        // random seed mỗi lần load scene nếu muốn
        if (useRandomSeed)
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
        }

        Random.InitState(seed);

        // Xóa tile cũ trong vùng
        ClearArea();

        // Tính offset để map nằm quanh (0,0) – tùy bạn có thể đổi
        int xOffset = -width / 2;
        int yOffset = -height / 2;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int tilePos = new Vector3Int(x + xOffset, y + yOffset, 0);

                TileBase tileToPlace = GetRandomTile();
                tilemap.SetTile(tilePos, tileToPlace);
            }
        }
    }

    private void ClearArea()
    {
        int xOffset = -width / 2;
        int yOffset = -height / 2;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var pos = new Vector3Int(x + xOffset, y + yOffset, 0);
                tilemap.SetTile(pos, null);
            }
        }
    }

    private TileBase GetRandomTile()
    {
        // Nếu có mảng randomTiles, random trong đó, nếu không thì dùng defaultTile
        if (randomTiles != null && randomTiles.Length > 0)
        {
            // Ví dụ: 80% default, 20% tile random
            float r = Random.value;
            if (r < 0.8f && defaultTile != null)
                return defaultTile;

            int index = Random.Range(0, randomTiles.Length);
            return randomTiles[index] != null ? randomTiles[index] : defaultTile;
        }

        return defaultTile;
    }
}