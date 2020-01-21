using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Audio manager class
 * Idea taken from Brackeys on YouTube
 * https://www.youtube.com/watch?v=6OT43pvUyfY
 */
public class AudioManager : MonoBehaviour
{
    public Sound[] GameSounds;
    public Dictionary<string, Sound> Sounds = new Dictionary<string, Sound>();

    void Awake()
    {
        foreach (var sound in GameSounds)
        {
            sound.Source = gameObject.AddComponent<AudioSource>();
            sound.Source.clip = sound.Clip;
            sound.Source.volume = sound.Volume;
            sound.Source.pitch = sound.Pitch;

            Sounds[sound.Name] = sound;
        }
    }

    public void Play(string name, bool loop = false)
    {
        if (Sounds.ContainsKey(name))
        {
            Sounds[name].Source.Play();
            Sounds[name].Source.loop = loop;
        }
        else
        {
            Debug.LogWarning("Cannot find sound " + name);
        }
    }

    /* Allows the same sound to be played ontop of itself */
    public void PlayLayered(string name)
    {
        if (Sounds.ContainsKey(name))
        {
            Sounds[name].Source.PlayOneShot(Sounds[name].Clip);
        }
        else
        {
            Debug.LogWarning("Cannot find sound " + name);
        }
    }

    /* Helper to play an error sound */
    public void PlayError()
    {
        Sounds["ButtonClick"].Source.Stop();
        Sounds["Error"].Source.Play();
    }

    /* Helper to fade out sounds, useful for music */
    public void FadeOutSound(string name, float time)
    {
        if (Sounds.ContainsKey(name))
        {
            StartCoroutine(FadeOut(Sounds[name], time));
        }
        else
        {
            Debug.LogWarning("Cannot find sound " + name);
        }
    }

    /* Coroutine to fade out a sound */
    private IEnumerator FadeOut(Sound s, float time)
    {
        float timer = time;

        while (timer > 0.0f)
        {
            timer -= Time.deltaTime;
            s.Source.volume = Mathf.Lerp(0.0f, s.Volume, timer / time);
            yield return null;
        }

        s.Source.Stop();
        s.Source.volume = s.Volume;
    }
}
