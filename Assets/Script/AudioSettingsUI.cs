using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("Mixer")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private string volumeParameter = "BGMVolume";

    [Header("UI")]
    [SerializeField] private Slider volumeSlider;

    private const float MinVolumeDb = -40f;
    private const float MaxVolumeDb = 0f;

    private void Awake()
    {
        if (volumeSlider == null)
            volumeSlider = GetComponentInChildren<Slider>();
    }

    private void Start()
    {
        if (mainMixer != null && mainMixer.GetFloat(volumeParameter, out float currentDb))
        {
            float t = Mathf.InverseLerp(MinVolumeDb, MaxVolumeDb, currentDb);
            volumeSlider.SetValueWithoutNotify(t);
        }

        volumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);
    }

    private void OnDestroy()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnVolumeSliderChanged);
    }

    private void OnVolumeSliderChanged(float value)
    {
        float db = Mathf.Lerp(MinVolumeDb, MaxVolumeDb, value);
        if (mainMixer != null)
            mainMixer.SetFloat(volumeParameter, db);
    }
}
