using UnityEngine;

[CreateAssetMenu(fileName = "AudioSettings", menuName = "Audio/Settings", order = 1)]
public class AudioSettingsSO : ScriptableObject
{
    [Range(0f, 1f)] public float volumeBacksound = 1f;
    [Range(0f, 1f)] public float volumeSFX = 1f;
}
