using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicVolumeBinder : MonoBehaviour
{
    private AudioSource audioSource;

    public static event System.Action OnVolumeChanged;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        ApplyVolume();
    }

    private void OnEnable()
    {
        OnVolumeChanged += ApplyVolume;
        ApplyVolume();
    }

    private void OnDisable()
    {
        OnVolumeChanged -= ApplyVolume;
    }

    public static void RefreshAll()
    {
        OnVolumeChanged?.Invoke();
    }

    public void ApplyVolume()
    {
        float volume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }
}
