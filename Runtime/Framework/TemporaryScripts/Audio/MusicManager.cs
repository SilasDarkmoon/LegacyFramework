using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
[XLua.LuaCallCSharp]
public static class MusicManager
{
    private static List<string> PlayList;
    private static float Volume;
    private static bool isPlaying;
    private static bool IsSet = false;

    public static void Reset()
    {
        IsSet = false;
    }
    public static void Set()
    {
        IsSet = true;
        PlayList = null;
        Volume = 0.4f;
        isPlaying = false;
    }

    public static void SetPlayList(List<string> playList)
    {
        if (AudioManager.GetPlayer("music") != null && AudioManager.GetPlayer("music").isPlaying)
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(FadeOut());
            seq.AppendCallback(() =>
            {
                PlayList = playList;
            });
        }
        else
        {
            PlayList = playList;
        }
    }

    public static void SetVolume(float volume)
    {
        Volume = volume;
    }

    public static void Play(string[] playList = null)
    {
        if (!IsSet)
        {
            Set();
        }
        if (playList != null)
        {
            SetPlayList(new List<string>(playList));
        }
        if (AudioManager.GetPlayer("music") == null)
        {
            AudioManager.CreatePlayer("music", true);
        }
        if (!isPlaying)
        {
            isPlaying = true;
            AudioManager.GetPlayer("music").StartCoroutine(PlayMusic());
        }
    }

    public static void Stop()
    {
        isPlaying = false;
        AudioManager.GetPlayer("music").Stop();
    }

    public static void DestroyPlayer()
    {
        Stop();
        AudioManager.DestroyPlayer("music");
    }

    public static Sequence FadeOut()
    {
        Sequence seq = DOTween.Sequence();
        var target = AudioManager.GetPlayer("music");
        seq.Append(DOTween.To(() => target.GlobalVolume, x => target.GlobalVolume = x, 0, 2f).SetTarget(target));
        seq.AppendCallback(Stop);
        return seq;
    }

    private static IEnumerator PlayMusic()
    {
        while (isPlaying)
        {
            if (PlayList != null && PlayList.Count > 0)
            {
                int playIndex = MathUtils.Random.RandomInt(0, PlayList.Count);
                yield return AudioManager.GetPlayer("music").PlayAudio(PlayList[playIndex], Volume);
            }
            yield return null;
        }
    }

}
