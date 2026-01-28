
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private CinemachineCamera vcam;

    private CinemachinePositionComposer positionComposer;
    private Vector3 originalDamping;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (!vcam)
        {
            Debug.LogError("ScreenFader: vcam chưa được gán");
            return;
        }

        positionComposer = vcam.GetComponent<CinemachinePositionComposer>();

        if (!positionComposer)
        {
            Debug.LogError("ScreenFader: Camera không có CinemachinePositionComposer");
            return;
        }

        originalDamping = positionComposer.Damping;
    }

    async Task Fade(float targetTransparency)
    {
        float start = canvasGroup.alpha;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, targetTransparency, t / fadeDuration);
            await Task.Yield();
        }

        canvasGroup.alpha = targetTransparency;
    }

    public async Task FadeOut()
    {
        await Fade(1f);
        SetDamping(Vector3.zero);
    }

    public async Task FadeIn()
    {
        await Fade(0f);
        SetDamping(originalDamping);
    }

    void SetDamping(Vector3 d)
    {
        if (!positionComposer) return;
        positionComposer.Damping = d;
    }
}
