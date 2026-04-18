using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Tilemaps;

public class AIGeneratedMap : MonoBehaviour
{
    [Header("AI Server")]
    [SerializeField] private string apiUrl = "http://127.0.0.1:5000/generate-map";

    [Header("Request Settings")]
    public int requestedWidth = 32;
    public int requestedHeight = 32;

    [Header("Ground Tiles (index = tile ID)")]
    public TileBase[] groundTiles;

    [Header("Decor Tiles (index = tile ID, 0 = none)")]
    public TileBase[] decorTiles;

    [Header("Wall Tiles (index = tile ID, 0 = none, 1 = wall)")]
    public TileBase[] wallTiles;

    [Header("Tilemaps")]
    public Tilemap groundTilemap;
    public Tilemap decorTilemap;
    public Tilemap wallTilemap;

    [Header("Post Process")]
    [SerializeField] private bool smoothGround = true;
    [SerializeField] [Range(0, 5)] private int smoothPasses = 2;
    [SerializeField] private bool forceBorderWalls = true;

    [System.Serializable]
    public class MapRequest
    {
        public int width;
        public int height;
    }

    [System.Serializable]
    public class MapData
    {
        public int width;
        public int height;
        public int[] ground;
        public int[] decor;
        public int[] walls;
    }

    private void Start()
    {
#if UNITY_EDITOR
        StartCoroutine(RequestMapFromAI());
#else
        Debug.Log("AIGeneratedMap: disabled in build.");
#endif
    }

    private IEnumerator RequestMapFromAI()
    {
        var reqBody = new MapRequest
        {
            width = requestedWidth,
            height = requestedHeight
        };

        string jsonBody = JsonUtility.ToJson(reqBody);

        using (var req = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            Debug.Log("AIGeneratedMap: requesting map from " + apiUrl);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("AIGeneratedMap: request failed: " + req.error);
                yield break;
            }

            string json = req.downloadHandler.text;
            MapData data = JsonUtility.FromJson<MapData>(json);
            if (data == null)
            {
                Debug.LogError("AIGeneratedMap: data == null");
                yield break;
            }

            int expectedLen = data.width * data.height;
            if (data.ground == null || data.ground.Length != expectedLen)
            {
                Debug.LogError($"AIGeneratedMap: ground invalid (len={data.ground?.Length}, expected={expectedLen})");
                yield break;
            }

            if (data.decor == null || data.decor.Length != expectedLen)
            {
                data.decor = new int[expectedLen];
            }

            if (data.walls == null || data.walls.Length != expectedLen)
            {
                data.walls = new int[expectedLen];
            }

            if (smoothGround)
            {
                ApplyGroundSmoothing(data);
            }

            if (forceBorderWalls)
            {
                ForceBorderWalls(data);
            }

            DrawMap(data);
        }
    }

    private void ApplyGroundSmoothing(MapData data)
    {
        int w = data.width;
        int h = data.height;

        for (int pass = 0; pass < smoothPasses; pass++)
        {
            int[] src = data.ground;
            int[] dst = new int[src.Length];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int index = y * w + x;

                    // giữ nguyên viền
                    if (x == 0 || y == 0 || x == w - 1 || y == h - 1)
                    {
                        dst[index] = src[index];
                        continue;
                    }

                    int[] counts = new int[groundTiles.Length > 0 ? groundTiles.Length : 3];

                    for (int oy = -1; oy <= 1; oy++)
                    {
                        for (int ox = -1; ox <= 1; ox++)
                        {
                            int nx = x + ox;
                            int ny = y + oy;
                            int ni = ny * w + nx;

                            int g = src[ni];
                            if (g >= 0 && g < counts.Length)
                            {
                                counts[g]++;
                            }
                        }
                    }

                    int bestType = src[index];
                    int bestCount = -1;
                    for (int i = 0; i < counts.Length; i++)
                    {
                        if (counts[i] > bestCount)
                        {
                            bestCount = counts[i];
                            bestType = i;
                        }
                    }

                    dst[index] = bestType;
                }
            }

            data.ground = dst;
        }
    }

    private void ForceBorderWalls(MapData data)
    {
        int w = data.width;
        int h = data.height;

        for (int x = 0; x < w; x++)
        {
            data.walls[x] = 1;
            data.walls[(h - 1) * w + x] = 1;
        }

        for (int y = 0; y < h; y++)
        {
            data.walls[y * w] = 1;
            data.walls[y * w + (w - 1)] = 1;
        }
    }

    private void DrawMap(MapData data)
    {
        if (groundTilemap == null || decorTilemap == null || wallTilemap == null)
        {
            Debug.LogError("AIGeneratedMap: Tilemaps not assigned");
            return;
        }

        groundTilemap.ClearAllTiles();
        decorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        int xOffset = -data.width / 2;
        int yOffset = -data.height / 2;

        for (int y = 0; y < data.height; y++)
        {
            for (int x = 0; x < data.width; x++)
            {
                int index = y * data.width + x;
                Vector3Int pos = new Vector3Int(x + xOffset, y + yOffset, 0);

                int g = data.ground[index];
                if (g >= 0 && g < groundTiles.Length)
                {
                    groundTilemap.SetTile(pos, groundTiles[g]);
                }

                int d = data.decor[index];
                if (d > 0 && d < decorTiles.Length)
                {
                    decorTilemap.SetTile(pos, decorTiles[d]);
                }

                int w = data.walls[index];
                if (w > 0 && w < wallTiles.Length)
                {
                    wallTilemap.SetTile(pos, wallTiles[w]);
                }
            }
        }

        Debug.Log($"AIGeneratedMap: drawn {data.width}x{data.height} map");
    }
}
