using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string Name;
    public AudioClip Clip;

    [HideInInspector]
    public AudioSource Source;

    public float Volume = 1.0f;
    public float Pitch = 1.0f;
}
