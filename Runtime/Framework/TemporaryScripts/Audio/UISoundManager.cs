using XLua;
using UnityEngine;

[LuaCallCSharp]
public static class UISoundManager
{
    private static float UISoundVolume;
    private static bool IsSet = false;

    public static void Set()
    {
        IsSet = true;
        LuaTable CommonConstants = LuaTableUtils.GetXLuaTable("ui.common.CommonConstants");
        UISoundVolume = CommonConstants.Get<float>("UISoundVolume"); ;
    }

    public static void Play(string file, float volume = -1f, bool loop = false)
    {
        if (!IsSet)
        {
            Set();
        }
        if (volume == -1f)
        {
            volume = UISoundVolume;
        }

        if (AudioManager.GetPlayer("ui") == null)
        {
            AudioManager.CreatePlayer("ui", true);
        }
        AudioManager.GetPlayer("ui")
            .PlayAudioInstantly("Assets/CapstonesRes/Game/Audio/UI/" + file, volume, loop);
    }

    public static void Stop()
    {
        if (AudioManager.GetPlayer("ui") != null)
        {
            AudioManager.GetPlayer("ui").Stop();
        }
    }
}
