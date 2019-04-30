/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using System.Collections.Generic;
using System;
using UnityEngine;
using XLua;

//配置的详细介绍请看Doc下《XLua的配置.doc》
public static class GenConfig
{
    //lua中要使用到C#库的配置，比如C#标准库，或者Unity API，第三方库等。
    [LuaCallCSharp]
    public static List<Type> LuaCallCSharp = new List<Type>()
    {
        #region unity + system 系统级别的类
        typeof(System.Object),
        typeof(Vector2),
        typeof(Vector3),
        typeof(Vector4),
        typeof(Quaternion),
        typeof(Color),
        typeof(Ray),
        typeof(Bounds),
        typeof(Ray2D),
        typeof(Time),
        typeof(GameObject),
        typeof(Component),
        typeof(Behaviour),
        typeof(Transform),
        typeof(RectTransform),
        typeof(Resources),
        typeof(TextAsset),
        typeof(Keyframe),
        typeof(AnimationCurve),
        typeof(AnimationClip),
        typeof(MonoBehaviour),
        typeof(ParticleSystem),
        typeof(SkinnedMeshRenderer),
        typeof(Renderer),
        typeof(WWW),
        typeof(CanvasGroup),
        typeof(System.GC),
        typeof(System.Type),
        typeof(System.Single),
        typeof(System.Reflection.Missing),
        typeof(System.Reflection.BindingFlags),
        typeof(UnityEngine.Object),
        typeof(UnityEngine.UI.Image),
        typeof(UnityEngine.Debug),
        typeof(UnityEngine.UI.Toggle),
        typeof(UnityEngine.UI.Toggle.ToggleEvent),
        typeof(UnityEngine.UI.Toggle.ToggleTransition),
        typeof(UnityEngine.AsyncOperation),
        typeof(UnityEngine.Input),
        typeof(UnityEngine.RectTransformUtility),
        typeof(UnityEngine.WaitForEndOfFrame),
        typeof(UnityEngine.WaitForSecondsRealtime),
        typeof(UnityEngine.SystemInfo),
        //ios不支持 typeof(UnityEngine.TextureFormat),
        typeof(UnityEngine.Application),
        typeof(UnityEngine.RuntimePlatform),
        typeof(UnityEngine.Shader),
        typeof(UnityEngine.QualitySettings),
        typeof(UnityEngine.Screen),
        typeof(UnityEngine.Rendering.GraphicsDeviceType),
        typeof(UnityEngine.LightmapData),
        typeof(UnityEngine.LightmapSettings),
        typeof(UnityEngine.Canvas),
        typeof(UnityEngine.UI.Graphic),
        typeof(UnityEngine.EventSystems.EventHandle),
        typeof(UnityEngine.EventSystems.UIBehaviour),
        typeof(UnityEngine.UI.Image.Origin180),
        typeof(UnityEngine.UI.Image.Origin360),
        typeof(UnityEngine.UI.Image.Origin90),
        typeof(UnityEngine.UI.Image.OriginVertical),
        typeof(UnityEngine.UI.Image.OriginHorizontal),
        typeof(UnityEngine.UI.Image.FillMethod),
        typeof(UnityEngine.UI.Image.Type),
        typeof(UnityEngine.UI.CanvasScaler),
        typeof(UnityEngine.UI.CanvasScaler.Unit),
        typeof(UnityEngine.UI.CanvasScaler.ScreenMatchMode),
        typeof(UnityEngine.UI.CanvasScaler.ScaleMode),
        typeof(UnityEngine.UI.GraphicRaycaster),
        typeof(UnityEngine.UI.GraphicRaycaster.BlockingObjects),
        typeof(UnityEngine.RectTransform.Axis),
        typeof(UnityEngine.RectTransform.Edge),
        typeof(UnityEngine.Camera),
        typeof(UnityEngine.Camera.MonoOrStereoscopicEye),
        typeof(UnityEngine.Camera.StereoscopicEye),
        typeof(UnityEngine.RenderMode),
        typeof(UnityEngine.UI.Slider),
        typeof(UnityEngine.UI.Slider.SliderEvent),
        typeof(UnityEngine.UI.Slider.Direction),
        typeof(UnityEngine.EventSystems.EventSystem),
        typeof(UnityEngine.EventSystems.StandaloneInputModule),
        typeof(UnityEngine.EventSystems.StandaloneInputModule.InputMode),
        typeof(UnityEngine.UI.Selectable),
        typeof(UnityEngine.UI.Button),
        typeof(UnityEngine.UI.Button.ButtonClickedEvent),
        typeof(UnityEngine.Events.UnityEvent),
        typeof(UnityEngine.UI.InputField),
        typeof(UnityEngine.UI.InputField.OnChangeEvent),
        typeof(UnityEngine.UI.InputField.SubmitEvent),
        typeof(UnityEngine.UI.InputField.LineType),
        typeof(UnityEngine.UI.InputField.CharacterValidation),
        typeof(UnityEngine.UI.InputField.InputType),
        typeof(UnityEngine.UI.InputField.ContentType),
        typeof(UnityEngine.UI.ScrollRect),
        typeof(UnityEngine.Events.UnityEventBase),
        typeof(UnityEngine.UI.MaskableGraphic.CullStateChangedEvent),
        typeof(UnityEngine.UI.ScrollRect.ScrollRectEvent),
        typeof(UnityEngine.UI.ScrollRect.ScrollbarVisibility),
        typeof(UnityEngine.UI.ScrollRect.MovementType),
        typeof(UnityEngine.AnimatorClipInfo),
        typeof(UnityEngine.EventSystems.PointerEventData),
        typeof(UnityEngine.EventSystems.BaseEventData),
        typeof(UnityEngine.EventSystems.AbstractEventData),
        typeof(UnityEngine.EventSystems.PointerEventData.FramePressState),
        typeof(UnityEngine.EventSystems.PointerEventData.InputButton),
        typeof(UnityEngine.Sprite),
        typeof(UnityEngine.UI.Selectable.Transition),
        typeof(UnityEngine.Coroutine),
        typeof(UnityEngine.YieldInstruction),
        typeof(UnityEngine.Animator),
        typeof(UnityEngine.UI.Image),
        typeof(UnityEngine.UI.MaskableGraphic),
        typeof(UnityEngine.PlayerPrefs),
        typeof(UnityEngine.EventSystems.UIBehaviour),
        typeof(UnityEngine.Rect),
        typeof(UnityEngine.UI.EmptyGraphic),
        typeof(UnityEngine.RenderTexture),
        typeof(UnityEngine.Texture),
        typeof(UnityEngine.Texture2D),
        typeof(UnityEngine.RenderTextureFormat),
        typeof(UnityEngine.RenderTextureReadWrite),
        typeof(UnityEngine.UI.RawImage),
        typeof(UnityEngine.UI.VerticalLayoutGroup),
        typeof(UnityEngine.UI.Text),
        typeof(UnityEngine.UI.Dropdown),
        typeof(UnityEngine.ScreenOrientation),
        typeof(UnityEngine.UI.Dropdown.DropdownEvent),
        typeof(UnityEngine.UI.Dropdown.OptionDataList),
        typeof(UnityEngine.UI.Dropdown.OptionData),
        typeof(UnityEngine.Renderer),
        typeof(UnityEngine.UI.ContentSizeFitter),
        typeof(UnityEngine.UI.ContentSizeFitter.FitMode),
        typeof(UnityEngine.UI.HorizontalLayoutGroup),
        typeof(UnityEngine.UI.GridLayoutGroup.Corner),
        typeof(UnityEngine.UI.GridLayoutGroup.Axis),
        typeof(UnityEngine.UI.GridLayoutGroup),
        typeof(UnityEngine.UI.GridLayoutGroup.Constraint),
        typeof(UnityEngine.UI.LayoutGroup),
        typeof(UnityEngine.Video.VideoPlayer), 
        #endregion
        #region 代理
        typeof(UnityEngine.Application.LogCallback),
        typeof(UnityEngine.Application.LowMemoryCallback),
        typeof(UnityEngine.Application.AdvertisingIdentifierCallback),
        typeof(UnityEngine.Canvas.WillRenderCanvases),
        typeof(UnityEngine.UI.InputField.OnValidateInput),
        typeof(UnityEngine.EventSystems.StandaloneInputModule.InputMode),
        typeof(UnityEngine.UI.InputField.OnValidateInput),
        

        #endregion
        #region list
        //typeof(UnityEngine.Events.UnityEvent<bool>),
        typeof(System.Collections.Generic.List<int>),
        //typeof(UnityEngine.Events.UnityEvent<bool>),
        //typeof(UnityEngine.Events.UnityEvent<string>),
        //typeof(UnityEngine.Events.UnityEvent<Vector2>),
        //typeof(UnityEngine.Events.UnityEvent<System.Int32>),
        //typeof(DG.Tweening.Core.TweenerCore<Single,Single,DG.Tweening.Plugins.Options.FloatOptions>),
        //typeof(DG.Tweening.Core.TweenerCore<Color,Color,DG.Tweening.Plugins.Options.ColorOptions>),
        //typeof(DG.Tweening.Core.TweenerCore<Vector2,Vector2,DG.Tweening.Plugins.Options.VectorOptions>),
        //typeof(DG.Tweening.Core.TweenerCore<Vector3,Vector3,DG.Tweening.Plugins.Options.VectorOptions>),
        //typeof(DG.Tweening.Core.TweenerCore<Vector2,Vector2,DG.Tweening.Plugins.Options.VectorOptions>),
        #endregion
        #region struct + enum
        typeof(DG.Tweening.Ease),
        typeof(Capstones.UnityFramework.HttpRequest.RequestStatus),
        typeof(XLua.UI.ScrollRectEx.MovementType),
        typeof(XLua.UI.ScrollRectExSameSize.Direction),
        typeof(XLua.UI.ScrollRectEx.Direction),
        #endregion
        typeof(GLog),
        //以下类不能进行 wrap 会出现bug
        //typeof(DG.Tweening.TweenExtensions),
        //typeof(DropdownExt),
        //typeof(System.String),
        //typeof(Capstones.PlatExt.PlatDependant),
        //typeof(Capstones.PlatExt.PlatDependant.TaskProgress),
        //typeof(System.Collections.Generic.List<UnityEngine.GameObject>),
        //typeof(UnityEngine.iOS.Device),
        //typeof(UnityEngine.iOS.DeviceGeneration),
       // typeof(CoreGame.UI.Models.MatchInfoModel),

    };
    //C#静态调用Lua的配置（包括事件的原型），仅可以配delegate，interface
    [CSharpCallLua]
    public static List<Type> CSharpCallLua = new List<Type>()
    {
        #region fun
        typeof(Func<uint>),
        typeof(Func<int>),
        typeof(Func<float>),
        typeof(Func<long>),
        typeof(Func<ulong>),
        typeof(Func<double>),
        typeof(Func<bool>),
        typeof(Func<double, double, double>),
        typeof(Func<string>),
        typeof(Func<Quaternion>),
        typeof(Func<Rect>),
        typeof(Func<RectOffset>),
        typeof(Func<Color>),
        typeof(Func<DG.Tweening.Color2>),
        typeof(Func<Vector2>),
        typeof(Func<Vector3>),
        typeof(Func<Vector4>),
        typeof(Func<LuaTable,LuaTable>),
        typeof(Func<LuaTable,int,string>),
        typeof(Func<LuaTable,GameObject>),
        typeof(Func<LuaTable,int,GameObject>),
        typeof(Func<LuaTable,string>),
        #endregion
        #region Action
        typeof(Action),
        typeof(Action<uint>),
        typeof(Action<int>),
        typeof(Action<float>),
        typeof(Action<long>),
        typeof(Action<ulong>),
        typeof(Action<double>),
        typeof(Action<bool>),
        typeof(Action<double, double, double>),
        typeof(Action<string>),
        typeof(Action<Quaternion>),
        typeof(Action<Rect>),
        typeof(Action<RectOffset>),
        typeof(Action<Color>),
        typeof(Action<DG.Tweening.Color2>),
        typeof(Action<Vector2>),
        typeof(Action<Vector3>),
        typeof(Action<Vector4>),
        typeof(Action<Sprite>),
        typeof(Action<int, String>),
        typeof(Action<LuaTable>),
        typeof(Action<LuaTable, int>),
        typeof(Action<LuaTable, LuaTable, int>),
        typeof(Action<string, string, LogType>),
        typeof(Action<string, bool, string>),
        typeof(Action<string, int, char>),
        typeof(Action<string, int, char>),
        typeof(Action<LuaTable,float>),
        typeof(Action<UnityEngine.Video.VideoPlayer>),
        typeof(Action<UnityEngine.Video.VideoPlayer,string>),
        #endregion
        //接口类
        typeof(System.Collections.IEnumerator),
        typeof(LuaBehaviour.ILuaCoroutine),
    };
    //黑名单
    [BlackList]
    public static List<List<string>> BlackList = new List<List<string>>()  {
                new List<string>(){"UnityEngine.WWW", "movie"},
    #if UNITY_WEBGL
                new List<string>(){"UnityEngine.WWW", "threadPriority"},
    #endif
                new List<string>(){"UnityEngine.Texture2D", "alphaIsTransparency"},
                new List<string>(){"UnityEngine.Security", "GetChainOfTrustValue"},
                new List<string>(){"UnityEngine.CanvasRenderer", "onRequestRebuild"},
                new List<string>(){"UnityEngine.Light", "areaSize"},
                new List<string>(){"UnityEngine.AnimatorOverrideController", "PerformOverrideClipListCleanup"},
    #if !UNITY_WEBPLAYER
                new List<string>(){"UnityEngine.Application", "ExternalEval"},
    #endif
                new List<string>(){"UnityEngine.GameObject", "networkView"}, //4.6.2 not support
                new List<string>(){"UnityEngine.Component", "networkView"},  //4.6.2 not support
                new List<string>(){"UnityEngine.UI.Text", "OnRebuildRequested"},
                new List<string>(){"System.IO.FileInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.FileInfo", "SetAccessControl", "System.Security.AccessControl.FileSecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.DirectoryInfo", "SetAccessControl", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "CreateSubdirectory", "System.String", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "Create", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"UnityEngine.MonoBehaviour", "runInEditMode"},
                new List<string>(){"Capstones.UnityFramework.ResManager", "LoadMainAsset", "System.String"},
                new List<string>(){"Capstones.UnityFramework.ResManager", "GetDistributeAssetName", "System.String"},
                new List<string>(){ "UnityEngine.Input", "IsJoystickPreconfigured", "System.String"},
                new List<string>(){ "UnityEngine.UI.Graphic", "OnRebuildRequested"},
            };
}