using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Transform attackPoint;
    public float attackRange = 1f;
    public int attackDamage = 20;
    public float attackCooldown = 0.4f;
    public LayerMask enemyLayer;
    public Animator animator;

    private float nextAttackTime;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>(); // bạn đã gắn Animator trên Player
    }

    private void Update()
    {
        if (Time.time >= nextAttackTime && MouseLeftPressed())
        {
            Attack();
        }

        UpdateAttackPointDirection();
    }

    private bool MouseLeftPressed()
    {
        // nếu dùng Input System mới nhưng chưa map Fire1, tạm dùng chuột trái
        return Input.GetMouseButtonDown(0);
        // hoặc nếu bạn đã map Fire1 trong Input Manager cũ:
        // return Input.GetButtonDown("Fire1");
    }

    private void UpdateAttackPointDirection()
    {
        if (attackPoint == null || animator == null) return;

        // Lấy hướng cuối cùng từ animator (PlayerMovement đã set)
        float lx = animator.GetFloat("LastInputX");
        float ly = animator.GetFloat("LastInputY");
        Vector2 dir = new Vector2(lx, ly);

        // Nếu chưa có hướng (player chưa di chuyển), có thể mặc định xuống dưới
        if (dir.sqrMagnitude < 0.01f)
            dir = Vector2.down;

        attackPoint.localPosition = dir.normalized * attackRange * 0.6f;
    }

    private void Attack()
    {
        nextAttackTime = Time.time + attackCooldown;

        if (animator != null)
            animator.SetTrigger("Attack");

        if (attackPoint == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayer
        );

        foreach (var hit in hits)
        {
            Health h = hit.GetComponent<Health>();
            if (h != null)
                h.TakeDamage(attackDamage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}