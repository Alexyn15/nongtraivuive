    using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float lifeTime = 1f; // chỉnh = thời lượng clip effect

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
