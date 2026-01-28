using UnityEngine;

public class PlayerItemCollecter : MonoBehaviour
{
    private InventoryController inventoryController;
    void Start()
    {
        inventoryController = FindObjectOfType<InventoryController>();
    }

    // Update is called once per frame
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Item"))
        {
            Item item = collision.GetComponent<Item>();
            if (item != null) {
                //add item to inventory
                bool itemAdded = inventoryController.AddItem(collision.gameObject);

                if (itemAdded)
                {   
                    item.ShowPopUp();
                    //destroy item from scene
                    Destroy(collision.gameObject);
                }
            }
        }
    }
}
