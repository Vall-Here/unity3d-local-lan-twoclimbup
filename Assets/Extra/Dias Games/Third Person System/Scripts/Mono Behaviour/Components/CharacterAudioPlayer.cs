using UnityEngine;
using Unity.Netcode;

namespace DiasGames.Components
{
public class CharacterAudioPlayer : NetworkBehaviour
{
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private AudioSource effectsSource;
    [SerializeField] private AudioSettingsSO audioSettings;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(IsOwner) {
            voiceSource.volume = audioSettings.volumeSFX;
        }

          if(MainSceneAudioManager.Instance != null) {
            MainSceneAudioManager.Instance.AudioSettingsChanged += UpdateAudioSettings; }
    }

    private void OnDisable()
    {
        if(MainSceneAudioManager.Instance != null) {
            MainSceneAudioManager.Instance.AudioSettingsChanged -= UpdateAudioSettings; }
    }
    private void UpdateAudioSettings(float volumeBacksound, float volumeSFX)
    {
        if(voiceSource != null) {
            voiceSource.volume = volumeSFX;
        }
        // if(effectsSource != null) {
        //     effectsSource.volume = volumeSFX;
        // }
    }

    public void PlayVoice(AudioClip clip)
    {
        if (voiceSource == null) return;
        if(IsOwner) {
            voiceSource.clip = clip;
            voiceSource.Play();
        }
    }
    public void PlayVoice(AudioClip[] clips)
    {
        if (voiceSource == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (IsOwner) {
            voiceSource.clip = clip;
            voiceSource.Play();
        }
    }

    public void PlayEffect(AudioClip clip)
    {
        if (effectsSource == null) return;
        if(IsOwner) {
            effectsSource.clip = clip;
            effectsSource.Play();
        }
    }

    public void PlayEffect(AudioClip[] clips)
    {
        if (effectsSource == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];

        if(IsOwner) {
            effectsSource.clip = clip;
            effectsSource.Play();
        } 
    }
}
}