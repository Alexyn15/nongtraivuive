using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class AnimalMovement : MonoBehaviour
{
    [Header("Movement Area")]
    public Vector2 center;           // tâm vùng di chuyển (nếu để (0,0) sẽ dùng vị trí spawn)
    public float moveRadius = 3f;    // bán kính vùng động vật được phép đi

    [Header("Movement Settings")]
    public float moveSpeed = 1.5f;
    public float changeDirIntervalMin = 2f;
    public float changeDirIntervalMax = 5f;

    [Header("Collision Settings")]
    public LayerMask obstacleLayers; // layer vật cản (tường, cây, đá...)

    private Rigidbody2D rb;
    private Vector2 moveDir;
    private float nextChangeDirTime;
    private Vector2 spawnPos;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Lưu vị trí spawn làm tâm nếu center chưa set
        spawnPos = transform.position;
        if (center == Vector2.zero)
            center = spawnPos;

        PickRandomDirection();
    }

    private void Update()
    {
        if (Time.time >= nextChangeDirTime)
        {
            PickRandomDirection();
        }

        // Giữ trong bán kính cho phép
        KeepWithinRadius();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveDir * moveSpeed;
    }

    private void PickRandomDirection()
    {
        // Hướng random trên mặt phẳng
        moveDir = Random.insideUnitCircle.normalized;
        if (moveDir == Vector2.zero)
            moveDir = Vector2.right;

        nextChangeDirTime = Time.time + Random.Range(changeDirIntervalMin, changeDirIntervalMax);
    }

    private void KeepWithinRadius()
    {
        Vector2 currentPos = transform.position;
        Vector2 toCenter = (center - currentPos);
        float distFromCenter = toCenter.magnitude;

        if (distFromCenter > moveRadius)
        {
            // khi ra khỏi vùng, quay đầu về tâm
            moveDir = toCenter.normalized;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Nếu va vào vật cản trong obstacleLayers thì quay đầu
        if (((1 << collision.gameObject.layer) & obstacleLayers) != 0)
        {
            // Đơn giản: đảo hướng
            moveDir = -moveDir;

            // Hoặc phản xạ theo normal va chạm:
            // moveDir = Vector2.Reflect(moveDir, collision.contacts[0].normal).normalized;
        }
    }
}
