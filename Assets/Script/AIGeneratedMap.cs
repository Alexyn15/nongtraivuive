using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Tilemaps;

public class AIGeneratedMap : MonoBehaviour
{
    [Header("AI Server")]
    [SerializeField] private string apiUrl = "http://localhost:5000/generate-map";

    [Header("Request Settings")]
    public int requestedWidth = 32;
    public int requestedHeight = 32;

    [Header("Ground Tiles (index = tile ID)")]
    public TileBase[] groundTiles; // 0 = grass, 1 = dirt, 2 = water, ...

    [Header("Decor Tiles (index = tile ID, 0 = none)")]
    public TileBase[] decorTiles;  // 0 = none, 1 = bush, 2 = rock, ...

    [Header("Tilemaps")]
    public Tilemap groundTilemap;
    public Tilemap decorTilemap;

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
        public int[] ground; // 1D, len = w*h
        public int[] decor;  // 1D, len = w*h
    }

    private void Awake()
    {
        // nếu quên gán, thử tự tìm trong con
        if (groundTilemap == null)
            groundTilemap = GetComponent<Tilemap>();
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
            Debug.Log("AIGeneratedMap: raw response = " + json);

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
                Debug.LogError($"AIGeneratedMap: decor invalid (len={data.decor?.Length}, expected={expectedLen})");
                // có thể cho phép decor thiếu và chỉ vẽ ground
                // yield break;
            }

            DrawMap(data);
        }
    }

    private void DrawMap(MapData data)
    {
        if (groundTilemap == null || decorTilemap == null)
        {
            Debug.LogError("AIGeneratedMap: Tilemaps not assigned");
            return;
        }

        groundTilemap.ClearAllTiles();
        decorTilemap.ClearAllTiles();

        int xOffset = -data.width / 2;
        int yOffset = -data.height / 2;

        for (int y = 0; y < data.height; y++)
        {
            for (int x = 0; x < data.width; x++)
            {
                int index = y * data.width + x;
                Vector3Int pos = new Vector3Int(x + xOffset, y + yOffset, 0);

                // ground
                int g = data.ground[index];
                if (g >= 0 && g < groundTiles.Length)
                {
                    groundTilemap.SetTile(pos, groundTiles[g]);
                }

                // decor (0 = none)
                int d = data.decor[index];
                if (d > 0 && d < decorTiles.Length)
                {
                    decorTilemap.SetTile(pos, decorTiles[d]);
                }
            }
        }

        Debug.Log($"AIGeneratedMap: drawn {data.width}x{data.height} map with ground+decor");
    }
}
