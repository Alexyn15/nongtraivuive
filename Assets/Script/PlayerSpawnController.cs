using System.Collections;
using UnityEngine;

public class PlayerSpawnController : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool forceSpawnAfterLoad = true;

    [Tooltip("Delay nhỏ để đảm bảo SaveController đã LoadGame xong.")]
    [SerializeField] private float delayAfterLoad = 0.05f;

    private IEnumerator Start()
    {
        if (!forceSpawnAfterLoad) yield break;

        // chờ qua frame Start của các script khác (đặc biệt SaveController)
        yield return null;
        yield return new WaitForEndOfFrame();

        if (delayAfterLoad > 0f)
            yield return new WaitForSeconds(delayAfterLoad);

        SpawnNow();
    }

    [ContextMenu("Spawn Now")]
    public void SpawnNow()
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning("PlayerSpawnController: spawnPoint chưa được gán.");
            return;
        }

        transform.position = spawnPoint.position;
    }
}