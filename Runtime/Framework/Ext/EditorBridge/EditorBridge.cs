using System;

namespace Capstones.UnityFramework
{
    public static class EditorBridge
    {
        private static event Action _OnPlayModeChanged = () => { };
        public static event Action OnPlayModeChanged
        {
            add
            {
#if UNITY_EDITOR
                _OnPlayModeChanged += value;
                UnityEditor.EditorApplication.playmodeStateChanged = DoPlayModeChanged;
#endif
            }
            remove
            {
#if UNITY_EDITOR
                _OnPlayModeChanged -= value;
#endif
            }
        }
        private static event Action _PrePlayModeChange = () => { };
        public static event Action PrePlayModeChange
        {
            add
            {
#if UNITY_EDITOR
                _PrePlayModeChange += value;
                UnityEditor.EditorApplication.playmodeStateChanged = DoPlayModeChanged;
#endif
            }
            remove
            {
#if UNITY_EDITOR
                _PrePlayModeChange -= value;
#endif
            }
        }
        private static event Action _AfterPlayModeChange = () => { };
        public static event Action AfterPlayModeChange
        {
            add
            {
#if UNITY_EDITOR
                _AfterPlayModeChange += value;
                UnityEditor.EditorApplication.playmodeStateChanged = DoPlayModeChanged;
#endif
            }
            remove
            {
#if UNITY_EDITOR
                _AfterPlayModeChange -= value;
#endif
            }
        }
        private static void DoPlayModeChanged()
        {
#if UNITY_EDITOR
            var isPlaying = UnityEngine.Application.isPlaying;
            var isToPlay = UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode;
            if (!isPlaying && isToPlay || isPlaying && !isToPlay)
            {
                _PrePlayModeChange();
                _OnPlayModeChanged();
            }
            else
            { 
                _OnPlayModeChanged();
                _AfterPlayModeChange();
            }
#endif
        }

        private static event Action _OnDelayedCallOnce = () => { };
        public static event Action OnDelayedCallOnce
        {
            add
            {
#if UNITY_EDITOR
                _OnDelayedCallOnce += value;
                UnityEditor.EditorApplication.delayCall = DoDelayedCallOnce;
#endif
            }
            remove
            {
#if UNITY_EDITOR
                _OnDelayedCallOnce -= value;
#endif
            }
        }
        private static void DoDelayedCallOnce()
        {
            _OnDelayedCallOnce();
            _OnDelayedCallOnce = () => { };
        }
    }
}