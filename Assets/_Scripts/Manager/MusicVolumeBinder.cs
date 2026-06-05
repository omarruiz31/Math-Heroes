using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicVolumeBinder : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        ApplyVolume();
    }

    public void ApplyVolume()
    {
        float volume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    private void OnEnable()
    {
        // Refresh when returning to scene or enabled
        ApplyVolume();
    }
}
