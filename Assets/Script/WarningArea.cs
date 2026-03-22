using UnityEngine;

public class WarningArea : MonoBehaviour
{
    public float telegraphDuration = 1.0f;
    public int damage = 20;
    public LayerMask playerLayer;

    [Header("Visual Effect")]
    public GameObject impactEffectPrefab; // prefab effect nổ skill

    private float triggerTime;
    private bool activated;

    private void OnEnable()
    {
        triggerTime = Time.time + telegraphDuration;
        activated = false;
    }

    private void Update()
    {
        if (!activated && Time.time >= triggerTime)
        {
            activated = true;
            ApplyDamage();
            SpawnEffect();
            Destroy(gameObject, 0.05f);
        }
    }

    private void ApplyDamage()
    {
        float radius = 0.5f;
        CircleCollider2D circle = GetComponent<CircleCollider2D>();
        if (circle != null)
            radius = circle.radius * Mathf.Max(transform.localScale.x, transform.localScale.y);

        Collider2D hit = Physics2D.OverlapCircle(transform.position, radius, playerLayer);
        if (hit != null)
        {
            Health h = hit.GetComponent<Health>();
            if (h != null)
                h.TakeDamage(damage);
        }
    }

    private void SpawnEffect()
    {
        if (impactEffectPrefab == null) return;
        Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float radius = 0.5f;
        CircleCollider2D circle = GetComponent<CircleCollider2D>();
        if (circle != null)
            radius = circle.radius * Mathf.Max(transform.localScale.x, transform.localScale.y);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}