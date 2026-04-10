using UnityEngine;

[CreateAssetMenu(fileName = "TempoMusicSet", menuName = "Brush With Death/Audio/Tempo Music Set")]
public class TempoMusicSet : ScriptableObject
{
    [Header("Main Loop")]
    [SerializeField] private AudioClip mainLoop;
    [SerializeField, Range(0f, 1f)] private float mainVolume = 1f;

    [Header("Tempo Layers")]
    [SerializeField] private AudioClip slowLayer;
    [SerializeField, Range(0f, 1f)] private float slowVolume = 1f;
    [SerializeField] private AudioClip midLayer;
    [SerializeField, Range(0f, 1f)] private float midVolume = 1f;
    [SerializeField] private AudioClip fastLayer;
    [SerializeField, Range(0f, 1f)] private float fastVolume = 1f;
    [SerializeField] private AudioClip intenseLayer;
    [SerializeField, Range(0f, 1f)] private float intenseVolume = 1f;

    public AudioClip MainLoop => mainLoop;
    public float MainVolume => mainVolume;

    public AudioClip GetTempoLayer(TempoBand tempoBand)
    {
        return tempoBand switch
        {
            TempoBand.Slow => slowLayer,
            TempoBand.Fast => fastLayer,
            TempoBand.Intense => intenseLayer,
            _ => midLayer
        };
    }

    public float GetTempoVolume(TempoBand tempoBand)
    {
        return tempoBand switch
        {
            TempoBand.Slow => slowVolume,
            TempoBand.Fast => fastVolume,
            TempoBand.Intense => intenseVolume,
            _ => midVolume
        };
    }
}
