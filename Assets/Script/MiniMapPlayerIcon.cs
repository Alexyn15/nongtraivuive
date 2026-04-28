using UnityEngine;

public class MiniMapPlayerIcon : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private RectTransform iconRect;
    [SerializeField] private RectTransform miniMapRect;

    [Header("World Bounds -> MiniMap Bounds")]
    [SerializeField] private Vector2 worldMin = new Vector2(-50f, -50f);
    [SerializeField] private Vector2 worldMax = new Vector2(50f, 50f);

    [Header("Rotation")]
    [SerializeField] private bool rotateIconByMovement = true;
    [SerializeField] private float rotationSmooth = 15f;

    private Vector2 lastMoveDir = Vector2.up;
    private Vector3 lastPos;

    private void Start()
    {
        if (player != null)
            lastPos = player.position;
    }

    private void LateUpdate()
    {
        if (player == null || iconRect == null || miniMapRect == null) return;

        // 1) Position: world -> minimap anchoredPosition
        float tx = Mathf.InverseLerp(worldMin.x, worldMax.x, player.position.x);
        float ty = Mathf.InverseLerp(worldMin.y, worldMax.y, player.position.z != 0 ? player.position.z : player.position.y);

        float px = Mathf.Lerp(-miniMapRect.rect.width * 0.5f, miniMapRect.rect.width * 0.5f, tx);
        float py = Mathf.Lerp(-miniMapRect.rect.height * 0.5f, miniMapRect.rect.height * 0.5f, ty);

        iconRect.anchoredPosition = new Vector2(px, py);

        // 2) Rotation theo hướng di chuyển
        if (rotateIconByMovement)
        {
            Vector2 move = new Vector2(player.position.x - lastPos.x, player.position.y - lastPos.y);
            if (move.sqrMagnitude > 0.0001f)
            {
                lastMoveDir = move.normalized;
            }

            float targetAngle = Mathf.Atan2(lastMoveDir.y, lastMoveDir.x) * Mathf.Rad2Deg - 90f;
            float currentZ = iconRect.localEulerAngles.z;
            float z = Mathf.LerpAngle(currentZ, targetAngle, Time.deltaTime * rotationSmooth);
            iconRect.localEulerAngles = new Vector3(0f, 0f, z);
        }

        lastPos = player.position;
    }
}