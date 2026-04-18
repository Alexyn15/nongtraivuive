using UnityEngine;

public class PlayerMiningController : MonoBehaviour
{
    [Header("Input")]
    public KeyCode mineKey = KeyCode.Q;

    [Header("Detect")]
    public Transform mineOrigin;
    public float mineRange = 1.25f;
    public LayerMask oreLayerMask;

    private InventoryController inventoryController;
    private ItemDictionary itemDictionary;

    private void Start()
    {
        inventoryController = FindObjectOfType<InventoryController>();
        itemDictionary = FindObjectOfType<ItemDictionary>();

        if (mineOrigin == null) mineOrigin = transform;
    }

    private void Update()
    {
        if (Input.GetKeyDown(mineKey))
        {
            TryMineNearestOre();
        }
    }

    private void TryMineNearestOre()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(mineOrigin.position, mineRange, oreLayerMask);
        if (hits == null || hits.Length == 0) return;

        OreNode nearest = null;
        float nearestDist = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            OreNode ore = hit.GetComponent<OreNode>();
            if (ore == null || !ore.CanMine()) continue;

            float d = (hit.transform.position - mineOrigin.position).sqrMagnitude;
            if (d < nearestDist)
            {
                nearestDist = d;
                nearest = ore;
            }
        }

        if (nearest != null)
        {
            nearest.TryMine(inventoryController, itemDictionary);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (mineOrigin == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(mineOrigin.position, mineRange);
    }
}