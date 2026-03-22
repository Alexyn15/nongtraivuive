using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossAI : MonoBehaviour
{
    public enum BossState
    {
        Idle,
        Chase,
        Casting,
        Dead
    }

    [Header("References")]
    public Transform player;
    public GameObject warningPrefab;   // sẽ gán prefab WarningArea
    public Animator animator;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float chaseRange = 8f;

    [Header("Skill")]
    public float skillCooldown = 3f;
    public float minSkillDistance = 2f;

    [Header("Loot")]
    public GameObject[] lootPrefabs;   // các prefab item sẽ rơi
    public int minLoot = 1;
    public int maxLoot = 3;
    public float lootRadius = 1f;

    private Rigidbody2D rb;
    private BossState state = BossState.Idle;
    private float nextSkillTime;
    private Health health;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();

        if (health != null)
            health.OnDeath += OnDeath;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void Update()
    {
        if (state == BossState.Dead) return;
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        switch (state)
        {
            case BossState.Idle:
                if (dist <= chaseRange)
                    state = BossState.Chase;
                break;

            case BossState.Chase:
                if (Time.time >= nextSkillTime && dist >= minSkillDistance)
                    StartCastSkill();
                else if (dist > chaseRange)
                    state = BossState.Idle;
                break;

            case BossState.Casting:
                // chờ cast xong
                break;
        }
    }

    private void FixedUpdate()
    {
        if (state == BossState.Dead || player == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (state == BossState.Chase)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            rb.linearVelocity = dir * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void StartCastSkill()
    {
        state = BossState.Casting;
        rb.linearVelocity = Vector2.zero;

        if (animator != null)
            animator.SetTrigger("Skill"); // hoặc đúng tên trigger trong Animator

        // Gọi luôn CastSkill để chắc chắn skill được dùng
        CastSkill();
    }

    // Có thể gọi trực tiếp (như trên) hoặc từ Animation Event trong clip Cast
    public void CastSkill()
    {
        if (warningPrefab == null || player == null) return;

        // Dừng di chuyển khi cast
        rb.linearVelocity = Vector2.zero;

        Vector2 targetPos = player.position; // đánh dấu ngay chỗ player
        Instantiate(warningPrefab, targetPos, Quaternion.identity);

        nextSkillTime = Time.time + skillCooldown;
        state = BossState.Chase;
    }

    private void OnDeath()
    {
        state = BossState.Dead;
        rb.linearVelocity = Vector2.zero;

        if (animator != null)
            animator.SetTrigger("Die");

        SpawnLoot();

        Destroy(gameObject); // xóa sau 3 giây để có thời gian chơi hiệu ứng chết
    }

    private void SpawnLoot()
    {
        if (lootPrefabs == null || lootPrefabs.Length == 0) return;

        int count = Random.Range(minLoot, maxLoot + 1);

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = lootPrefabs[Random.Range(0, lootPrefabs.Length)];
            if (prefab == null) continue;

            // vị trí rơi ngẫu nhiên quanh boss
            Vector2 offset = Random.insideUnitCircle * lootRadius;
            Vector3 spawnPos = transform.position + new Vector3(offset.x, offset.y, 0f);

            Instantiate(prefab, spawnPos, Quaternion.identity);
        }
    }
}