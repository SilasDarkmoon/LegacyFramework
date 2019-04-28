using System.Collections;
using Capstones.UnityFramework;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPlayer : MonoBehaviour
{
    private float clipVolume = 1f;
    private float? globalVolume = null;
    private string category;
    private string playerPrefsKey;
    private AudioSource audioSource = null;

    public bool isPlaying
    {
        get { return audioSource.isPlaying; }
    }

    public float ClipVolume
    {
        get { return clipVolume; }
        private set
        {
            clipVolume = value;
            ApplyVolume();
        }
    }

    public string Category
    {
        get { return category; }
        set
        {
            category = value;
            playerPrefsKey = "keyAudioGlobalVolume_" + value;
        }
    }

    public float GlobalVolume
    {
        get
        {
            if (!globalVolume.HasValue)
            {
                globalVolume = 1f;
            }

            return globalVolume.Value;
        }
        set
        {
            globalVolume = value;
            ApplyVolume();
        }
    }

    public float ClipLength
    {
        get
        {
            if (audioSource.clip != null)
                return audioSource.clip.length;
            return 0;
        }
    }

    public float RemainingClipLength
    {
        get
        {
            if (audioSource.loop)
                return -1;
            if (audioSource.clip != null)
                return audioSource.clip.length - audioSource.time;
            return 0;
        }
    }

    public IEnumerator PlayAudio(string path, float volume, bool loop = false)
    {
        ClipVolume = volume;
        var clip = ResManager.LoadRes(path, typeof(AudioClip)) as AudioClip;
        if (clip)
        {
            audioSource.clip = clip;
            ApplyVolume();
            audioSource.Play();
            audioSource.loop = loop;
            yield return new AudioPlayEndYieldInstruction(audioSource);
        }
        else
        {
            if (GLog.IsLogErrorEnabled) GLog.LogError("Audio clip not found, path :" + path);
        }
    }

    public void PlayAudioInstantly(string path, float volume, bool loop = false)
    {
        PlayAudio(path, volume, loop).MoveNext();
    }

    public void Stop()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    public void ApplyVolume()
    {
        if (category.Equals("music"))
        {
            audioSource.volume = clipVolume * GlobalVolume * AudioManager.GlobalMusicVolume;
        }
        else
        {
            audioSource.volume = clipVolume * GlobalVolume * AudioManager.GlobalVolume;
        }
    }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
}
