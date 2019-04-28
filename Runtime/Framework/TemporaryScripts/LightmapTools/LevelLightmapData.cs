using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class LevelLightmapData : MonoBehaviour
{
    [System.Serializable]
    public class SphericalHarmonics
    {
        public float[] coefficients = new float[27];
    }

    [System.Serializable]
    public class RendererInfo
    {
        public Renderer renderer;
        public int lightmapIndex;
        public Vector4 lightmapOffsetScale;
    }

    [System.Serializable]
    public class LightingScenarioData
    {
        public RendererInfo[] rendererInfos;
        public Texture2D[] lightmaps;
        public Texture2D[] lightmapsDir;
        public LightmapsMode lightmapsMode;
        public SphericalHarmonics[] lightProbes;
    }

    [SerializeField]
    private LightingScenarioData lightingScenariosData;

#if UNITY_EDITOR
    [SerializeField]
    public SceneAsset lightingScenariosScenes;
#endif
    [SerializeField]
    public List<String> lightingScenesNames;
    public int currentLightingScenario = -1;
    public int previousLightingScenario = -1;

    [SerializeField]
    public int lightingScenariosCount;

    public bool verbose = false;

    private List<SphericalHarmonicsL2[]> lightProbesRuntime = new List<SphericalHarmonicsL2[]>();

    public void LoadLightingScenario()
    {
        LightmapSettings.lightmapsMode = lightingScenariosData.lightmapsMode;

        var newLightmaps = LoadLightmaps();

        ApplyRendererInfo(lightingScenariosData.rendererInfos);//每种天气只会保存一个lightingScenariosData，所以索引取0即可

        LightmapSettings.lightmaps = newLightmaps;
    }

    private void Start()
    {
        //PrepareLightProbeArrays();
        LoadLightingScenario();
    }

    private void OnDestroy()
    {
        LightmapSettings.lightmapsMode = LightmapsMode.NonDirectional;
        LightmapSettings.lightmaps = null;
    }

    private void PrepareLightProbeArrays()
    {
        for (int x = 0; x < lightingScenariosCount; x++)
        {
            lightProbesRuntime.Add(DeserializeLightProbes(x));
        }
    }

    private SphericalHarmonicsL2[] DeserializeLightProbes(int index)
    {
        var sphericalHarmonicsArray = new SphericalHarmonicsL2[lightingScenariosData.lightProbes.Length];

        for (int i = 0; i < lightingScenariosData.lightProbes.Length; i++)
        {
            var sphericalHarmonics = new SphericalHarmonicsL2();

            // j is coefficient
            for (int j = 0; j < 3; j++)
            {
                //k is channel ( r g b )
                for (int k = 0; k < 9; k++)
                {
                    sphericalHarmonics[j, k] = lightingScenariosData.lightProbes[i].coefficients[j * 9 + k];
                }
            }

            sphericalHarmonicsArray[i] = sphericalHarmonics;
        }
        return sphericalHarmonicsArray;
    }

    LightmapData[] LoadLightmaps()
    {
        if (lightingScenariosData.lightmaps == null
                || lightingScenariosData.lightmaps.Length == 0)
        {
            if (GLog.IsLogWarningEnabled) GLog.LogWarning("No lightmaps stored in scenario ");
            return null;
        }

        var newLightmaps = new LightmapData[lightingScenariosData.lightmaps.Length];

        for (int i = 0; i < newLightmaps.Length; i++)
        {
            newLightmaps[i] = new LightmapData();
            newLightmaps[i].lightmapColor = lightingScenariosData.lightmaps[i];

            if (lightingScenariosData.lightmapsMode != LightmapsMode.NonDirectional)
            {
                newLightmaps[i].lightmapDir = lightingScenariosData.lightmapsDir[i];
            }
        }
        return newLightmaps;
    }

    public void ApplyRendererInfo(RendererInfo[] infos)
    {
        try
        {
            for (int i = 0; i < infos.Length; i++)
            {
                var info = infos[i];
                if (info != null && info.renderer != null)
                {
                    info.renderer.lightmapIndex = infos[i].lightmapIndex;
                    if (!info.renderer.isPartOfStaticBatch)
                    {
                        info.renderer.lightmapScaleOffset = infos[i].lightmapOffsetScale;
                    }
                    if (info.renderer.isPartOfStaticBatch && verbose == true)
                    {
                        if (GLog.IsLogInfoEnabled) GLog.LogInfo("Object " + info.renderer.gameObject.name + " is part of static batch, skipping lightmap offset and scale.");
                    }
                }
            }
        }
        catch (Exception e)
        {
            if(GLog.IsLogErrorEnabled) GLog.LogError("Error in ApplyRendererInfo:" + e.GetType().ToString());
        }
    }

    public void LoadLightProbes(int index)
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            PrepareLightProbeArrays();
        }
        try
        {
            LightmapSettings.lightProbes.bakedProbes = lightProbesRuntime[index];
        }
        catch (Exception e)
        {
            if(GLog.IsLogErrorEnabled) GLog.LogException("Warning, error when trying to load lightprobes for scenario " + index + "\n" + e);
        }
    }

#if UNITY_EDITOR

    public void StoreLightmapInfos(int index)
    {
        if (GLog.IsLogInfoEnabled) GLog.LogInfo("Storing data for lighting scenario " + index);
        if (UnityEditor.Lightmapping.giWorkflowMode != UnityEditor.Lightmapping.GIWorkflowMode.OnDemand)
        {
            if(GLog.IsLogErrorEnabled) GLog.LogError("ExtractLightmapData requires that you have baked you lightmaps and Auto mode is disabled.");
            return;
        }

        var newLightingScenarioData = new LightingScenarioData();
        var newRendererInfos = new List<RendererInfo>();
        var newLightmapsTextures = new List<Texture2D>();
        var newLightmapsTexturesDir = new List<Texture2D>();
        var newLightmapsMode = new LightmapsMode();
        var newSphericalHarmonicsList = new List<SphericalHarmonics>();

        newLightmapsMode = LightmapSettings.lightmapsMode;

        GenerateLightmapInfo(gameObject, newRendererInfos, newLightmapsTextures, newLightmapsTexturesDir, newLightmapsMode);

        newLightingScenarioData.lightmapsMode = newLightmapsMode;

        newLightingScenarioData.lightmaps = newLightmapsTextures.ToArray();

        if (newLightmapsMode != LightmapsMode.NonDirectional)
        {
            newLightingScenarioData.lightmapsDir = newLightmapsTexturesDir.ToArray();
        }

        newLightingScenarioData.rendererInfos = newRendererInfos.ToArray();

        var scene_LightProbes = new SphericalHarmonicsL2[LightmapSettings.lightProbes.bakedProbes.Length];
        scene_LightProbes = LightmapSettings.lightProbes.bakedProbes;

        for (int i = 0; i < scene_LightProbes.Length; i++)
        {
            var SHCoeff = new SphericalHarmonics();

            // j is coefficient
            for (int j = 0; j < 3; j++)
            {
                //k is channel ( r g b )
                for (int k = 0; k < 9; k++)
                {
                    SHCoeff.coefficients[j * 9 + k] = scene_LightProbes[i][j, k];
                }
            }

            newSphericalHarmonicsList.Add(SHCoeff);
        }

        newLightingScenarioData.lightProbes = newSphericalHarmonicsList.ToArray();

        lightingScenariosData = newLightingScenarioData;

        if (lightingScenesNames == null || lightingScenesNames.Count < lightingScenariosCount)
        {
            lightingScenesNames = new List<string>();
            while (lightingScenesNames.Count < lightingScenariosCount)
            {
                lightingScenesNames.Add(null);
            }
        }
    }

    static void GenerateLightmapInfo(GameObject root, List<RendererInfo> newRendererInfos, List<Texture2D> newLightmapsLight, List<Texture2D> newLightmapsDir, LightmapsMode newLightmapsMode)
    {
        //BuglyAgent.PrintLog(LogSeverity.LogDebug, "GenerateLightmapInfo");
        var renderers = FindObjectsOfType(typeof(MeshRenderer));
        if (GLog.IsLogInfoEnabled) GLog.LogInfo("stored info for " + renderers.Length + " meshrenderers");
        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer.lightmapIndex != -1)
            {
                RendererInfo info = new RendererInfo();
                info.renderer = renderer;
                info.lightmapOffsetScale = renderer.lightmapScaleOffset;

                if (renderer.lightmapIndex >= LightmapSettings.lightmaps.Length)
                {
                    if(GLog.IsLogErrorEnabled) GLog.LogError("Render Errror");
                    continue;
                }

                Texture2D lightmaplight = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapColor;
                info.lightmapIndex = newLightmapsLight.IndexOf(lightmaplight);
                if (info.lightmapIndex == -1)
                {
                    info.lightmapIndex = newLightmapsLight.Count;
                    newLightmapsLight.Add(lightmaplight);
                }

                if (newLightmapsMode != LightmapsMode.NonDirectional)
                {
                    Texture2D lightmapdir = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapDir;
                    info.lightmapIndex = newLightmapsDir.IndexOf(lightmapdir);
                    if (info.lightmapIndex == -1)
                    {
                        info.lightmapIndex = newLightmapsDir.Count;
                        newLightmapsDir.Add(lightmapdir);
                    }
                }
                newRendererInfos.Add(info);
            }
        }

    }

    public void BuildLightingScenario(string ScenarioName)
    {
        //Remove reference to LightingDataAsset so that Unity doesn't delete the previous bake
        Lightmapping.lightingDataAsset = null;

        if (GLog.IsLogInfoEnabled) GLog.LogInfo("Baking" + ScenarioName);

        EditorSceneManager.OpenScene("Assets/CapstonesRes/Game/Models/Scene/LightingMapData/" + ScenarioName + ".unity", OpenSceneMode.Additive);
        EditorSceneManager.SetActiveScene(EditorSceneManager.GetSceneByPath("Assets/CapstonesRes/Game/Models/Scene/LightingMapData/" + ScenarioName + ".unity"));

        StartCoroutine(BuildLightingAsync(ScenarioName));
    }

    private IEnumerator BuildLightingAsync(string ScenarioName)
    {
        var newLightmapMode = new LightmapsMode();
        newLightmapMode = LightmapSettings.lightmapsMode;
        Lightmapping.BakeAsync();
        while (Lightmapping.isRunning) { yield return null; }
        //EditorSceneManager.SaveScene(EditorSceneManager.GetSceneByPath("Assets/Scenes/Develop/LightMapScenes/LightingMapData/" + ScenarioName + ".unity"));
        //Lightmapping.lightingDataAsset = null;
        EditorSceneManager.SaveScene(EditorSceneManager.GetSceneByPath("Assets/CapstonesRes/Game/Models/Scene/LightingMapData/" + ScenarioName + ".unity"));
        EditorSceneManager.CloseScene(EditorSceneManager.GetSceneByPath("Assets/CapstonesRes/Game/Models/Scene/LightingMapData/" + ScenarioName + ".unity"), true);
        LightmapSettings.lightmapsMode = newLightmapMode;
    }
#endif
}