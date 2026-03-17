using UnityEngine;

public class PlayerFarmAction : MonoBehaviour
{
    private FarmTile currentTile;
    public PlayerAnimation playerAnim; // GÁN TRONG INSPECTOR

    void Update()
    {
        // C = tưới (KHÔNG cần đứng trên đất cũng được)
        if (Input.GetKeyDown(KeyCode.C))
        {
            playerAnim.PlayWater();
        }

        if (currentTile == null) return;

        // F = cuốc đất
        if (Input.GetKeyDown(KeyCode.F))
        {
            currentTile.Hoe();
        }

        // E = thu hoạch
        if (Input.GetKeyDown(KeyCode.E))
        {
            currentTile.Harvest();
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        currentTile = col.GetComponent<FarmTile>();
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.GetComponent<FarmTile>() == currentTile)
            currentTile = null;
    }
}
