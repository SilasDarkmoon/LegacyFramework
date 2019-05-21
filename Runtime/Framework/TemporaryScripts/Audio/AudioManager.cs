using System.Collections.Generic;
using System.Linq;
using Capstones.UnityFramework;
using UnityEngine;

public class AudioManager
{
    private static float? globalVolume = null;
    private static float? globalMusicVolume = null;
    private static string playerPrefsKey = "keyAudioGlobalVolume";
    private static string playerPrefsKeyMusic = "keyMusicGlobalVolume";
    private static Dictionary<string, AudioPlayer> playerMap = new Dictionary<string, AudioPlayer>();

    public static float GlobalVolume
    {
        get
        {
            if (!globalVolume.HasValue)
            {
                if (PlayerPrefs.HasKey(playerPrefsKey))
                {
                    globalVolume = PlayerPrefs.GetFloat(playerPrefsKey);
                }
                else
                {
                    SetGlobalVolume(1f);
                }
            }

            return globalVolume.Value;
        }
        set { SetGlobalVolume(value); }
    }

    public static float GlobalMusicVolume
    {
        get
        {
            if (!globalMusicVolume.HasValue)
            {
                if (PlayerPrefs.HasKey(playerPrefsKeyMusic))
                {
                    globalMusicVolume = PlayerPrefs.GetFloat(playerPrefsKeyMusic);
                }
                else
                {
                    SetGlobalMusicVolume(1f);
                }
            }

            return globalMusicVolume.Value;
        }
        set { SetGlobalMusicVolume(value); }
    }

    public static AudioPlayer GetPlayer(string category)
    {
        if (playerMap.ContainsKey(category))
        {
            return playerMap[category];
        }
        return null;
    }

    public static bool CreatePlayer(string category, bool ignoreClear = false)
    {
        if (playerMap.ContainsKey(category))
        {
            if (GLog.IsLogWarningEnabled) GLog.LogWarning("Audio Player '"+ category + "' already created! Returning original player!");
            return false;
        }

        // Create new Audio Player
        var go = new GameObject("AudioPlayer " + category, typeof(AudioSource), typeof(AudioPlayer));
        GameObject.DontDestroyOnLoad(go);
        //if (!ignoreClear)
        //{
        //    ResManager.CanDestroyAll(go);
        //}

        var audioPlayer = go.GetComponent<AudioPlayer>();
        audioPlayer.Category = category;
        audioPlayer.GlobalVolume = 1f;
        playerMap.Add(category, audioPlayer);

        return true;
    }

    public static void DestroyPlayer(string category)
    {
        if (!playerMap.ContainsKey(category))
        {
            if (GLog.IsLogWarningEnabled) GLog.LogWarning("Audio Player '"+ category + "' not exist!");
            return;
        }
        if (playerMap[category])
        {
            playerMap[category].Stop();
            Object.Destroy(playerMap[category]);
        }

        playerMap.Remove(category);
    }

    public static void RemoveUnusedKeys()
    {
        var keys = playerMap.Keys.ToArray();

        foreach (var key in keys)
        {
            if (playerMap[key] == null)
            {
                playerMap.Remove(key);
            }
        }
    }

    public static void DestroyAllPlayers()
    {
        var keys = playerMap.Keys.ToArray();

        foreach (var key in keys)
        {
            Object.Destroy(playerMap[key]);
            playerMap.Remove(key);
        }
    }

    private static void SetGlobalVolume(float value)
    {
        globalVolume = value;
        PlayerPrefs.SetFloat(playerPrefsKey, value);
        foreach (var player in playerMap.Values)
        {
            player.ApplyVolume();
        }
    }

    private static void SetGlobalMusicVolume(float value)
    {
        globalMusicVolume = value;
        PlayerPrefs.SetFloat(playerPrefsKeyMusic, value);
        foreach (var player in playerMap.Values)
        {
            player.ApplyVolume();
        }
    }
}
