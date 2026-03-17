using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerFarmController : MonoBehaviour
{
    public Tilemap tm_Ground;
    public Tilemap tm_Grass;
    public Tilemap tm_Forest;

    public TileBase tb_Ground;
    public TileBase tb_Grass;
    public TileBase tb_Forest;

    public HotbarController hotbar;
    private Item selectedItem;
    public Item cropDropPrefab;
    private Dictionary<TileBase, Item> cropLookup = new Dictionary<TileBase, Item>();

    [System.Serializable]
    public class CropData
    {
        public TileBase cropTile;       // Tile cây lớn - dùng để thu hoạch (giống code gốc)
        public TileBase seedTile;       // Tile hạt mới trồng (MỚI - nếu để trống thì bỏ qua giai đoạn hạt)
        public Item harvestItem;
        public float growTime = 10f;    // Thời gian lớn (giây), chỉ dùng khi có seedTile
    }

    public List<CropData> crops;

    private HashSet<Vector3Int> readyToHarvest = new HashSet<Vector3Int>();

    void Update()
    {
        HandleFarmAction();
    }

    void Start()
    {
        RegisterAllSeeds();
    }

    void RegisterAllSeeds()
    {
        Item[] allItems = Resources.LoadAll<Item>("");
        foreach (Item item in allItems)
        {
            if (item.isSeed && item.cropTile != null && item.harvestItem != null)
            {
                cropLookup[item.cropTile] = item.harvestItem;
            }
        }
    }

    public void HandleFarmAction()
    {
        // G = xóa cỏ
        if (Input.GetKeyDown(KeyCode.G))
        {
            Vector3Int cellPos = tm_Ground.WorldToCell(transform.position);
            TileBase currentTile = tm_Grass.GetTile(cellPos);
            if (currentTile == tb_Grass)
                tm_Grass.SetTile(cellPos, null);
        }

        // H = trồng cây
        if (Input.GetKeyDown(KeyCode.H))
        {
            selectedItem = hotbar.GetSelectedItem();
            if (selectedItem == null) return;

            Vector3Int cellPos = tm_Ground.WorldToCell(transform.position);
            TileBase groundTile = tm_Ground.GetTile(cellPos);
            TileBase forestTile = tm_Forest.GetTile(cellPos);

            TileBase grassTile = tm_Grass.GetTile(cellPos);

            if (selectedItem.isSeed &&
                selectedItem.cropTile != null &&
                groundTile == tb_Ground &&
                forestTile == null &&
                grassTile == null)   // Phải dọn cỏ trước (G) mới trồng được
            {
                CropData matched = crops.Find(c => c.cropTile == selectedItem.cropTile);

                if (matched != null && matched.seedTile != null)
                {
                    // Có seedTile: hiện hạt trước, sau growTime đổi thành cropTile
                    tm_Forest.SetTile(cellPos, matched.seedTile);
                    StartCoroutine(GrowCrop(cellPos, matched));
                }
                else
                {
                    // Không có seedTile: hoạt động y hệt code gốc
                    tm_Forest.SetTile(cellPos, selectedItem.cropTile);
                    if (matched != null)
                        readyToHarvest.Add(cellPos);
                }

                selectedItem.RemoveFromStack(1);
                if (selectedItem.quantity <= 0)
                    Destroy(selectedItem.gameObject);
            }
        }

        // J = thu hoạch
        if (Input.GetKeyDown(KeyCode.J))
        {
            Vector3Int cellPos = tm_Ground.WorldToCell(transform.position);
            TileBase forestTile = tm_Forest.GetTile(cellPos);
            if (forestTile == null) return;

            foreach (var crop in crops)
            {
                if (crop.cropTile == forestTile && readyToHarvest.Contains(cellPos))
                {
                    hotbar.AddItem(crop.harvestItem, 1);
                    tm_Forest.SetTile(cellPos, null);
                    readyToHarvest.Remove(cellPos);
                    return;
                }
            }
        }
    }

    private IEnumerator GrowCrop(Vector3Int cellPos, CropData crop)
    {
        yield return new WaitForSeconds(crop.growTime);

        // Chỉ đổi nếu tile vẫn còn là seedTile (chưa bị xóa)
        if (tm_Forest.GetTile(cellPos) == crop.seedTile)
        {
            tm_Forest.SetTile(cellPos, crop.cropTile);
            readyToHarvest.Add(cellPos);
        }
    }
}