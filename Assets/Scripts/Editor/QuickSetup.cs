#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public static class QuickSetup
{
    [MenuItem("Tools/VRProject/Quick Setup")]
    public static void DoSetup()
    {
        var sys = GameObject.Find("GameSystem") ?? new GameObject("GameSystem");
        var pathMgr = Require<PathManager>(sys);
        var weather = Require<WeatherManager>(sys);
        var spawner = Require<CrowdSpawner>(sys);
        var simpleUI = Require<SimpleUI>(sys);
        var selfTest = Require<SelfTest>(sys);
        var checklist = Require<RequirementChecklist>(sys);

        var mainCam = Camera.main;
        if (mainCam == null)
        {
            var camGO = new GameObject("MainCamera");
            mainCam = camGO.AddComponent<Camera>();
            camGO.tag = "MainCamera";
            camGO.transform.position = new Vector3(0, 5, -10);
            camGO.transform.rotation = Quaternion.Euler(15, 0, 0);
        }
        pathMgr.cam = mainCam;

        var sun = Object.FindObjectOfType<Light>();
        if (sun == null || sun.type != LightType.Directional)
        {
            var sunGO = new GameObject("Sun");
            sun = sunGO.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.1f;
            sun.transform.rotation = Quaternion.Euler(50, -30, 0);
        }
        weather.sun = sun;

        var cap = GameObject.Find("CaptureCamera");
        CameraCapture capture;
        if (cap == null)
        {
            cap = new GameObject("CaptureCamera");
            var c = cap.AddComponent<Camera>();
            c.enabled = false;
            capture = cap.AddComponent<CameraCapture>();
        }
        else capture = cap.GetComponent<CameraCapture>() ?? cap.AddComponent<CameraCapture>();

        EnsureFolder("Assets/Prefabs");
        var lrGO = new GameObject("PathLinePrefab");
        var lr = lrGO.AddComponent<LineRenderer>();
        lr.alignment = LineAlignment.View;
        lr.widthMultiplier = 0.06f;
        lr.material = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Line.mat");
        lr.numCapVertices = 4;
        var prefabPath = "Assets/Prefabs/PathLine.prefab";
        PrefabUtility.SaveAsPrefabAsset(lrGO, prefabPath);
        Object.DestroyImmediate(lrGO);
        var lrComp = AssetDatabase.LoadAssetAtPath<LineRenderer>(prefabPath);
        pathMgr.pathLinePrefab = lrComp;

        if (spawner.spawnPoints == null || spawner.spawnPoints.Length == 0)
        {
            var parent = GameObject.Find("SpawnPoints") ?? new GameObject("SpawnPoints");
            var pts = new System.Collections.Generic.List<Transform>();
            for (int i = 0; i < 5; i++)
            {
                var p = new GameObject("SpawnPoint_" + i);
                p.transform.parent = parent.transform;
                p.transform.position = new Vector3(i * 2f, 0f, 0f);
                pts.Add(p.transform);
            }
            spawner.spawnPoints = pts.ToArray();
        }

        var canvasGO = GameObject.Find("VRProjectCanvas");
        Canvas canvas;
        if (canvasGO == null)
        {
            canvasGO = new GameObject("VRProjectCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        else canvas = canvasGO.GetComponent<Canvas>();

        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        float top = -40f;
        float left = 110f;
        CreateButton(canvas.transform, "Select All", left, top + 0, () => UnityEventTools.AddPersistentListener(GetButton("Select All").onClick, simpleUI.SelectAllAgents));
        CreateButton(canvas.transform, "Set Start", left, top + 40, () => UnityEventTools.AddPersistentListener(GetButton("Set Start").onClick, simpleUI.SetStartFromClick));
        CreateButton(canvas.transform, "Set Goal+Apply", left, top + 80, () => UnityEventTools.AddPersistentListener(GetButton("Set Goal+Apply").onClick, simpleUI.SetGoalFromClickAndApply));
        CreateButton(canvas.transform, "Sunny", left, top + 140, () => UnityEventTools.AddPersistentListener(GetButton("Sunny").onClick, simpleUI.SetWeatherSunny));
        CreateButton(canvas.transform, "Night", left, top + 180, () => UnityEventTools.AddPersistentListener(GetButton("Night").onClick, simpleUI.SetWeatherNight));
        CreateButton(canvas.transform, "Foggy", left, top + 220, () => UnityEventTools.AddPersistentListener(GetButton("Foggy").onClick, simpleUI.SetWeatherFoggy));
        CreateButton(canvas.transform, "Snow", left, top + 260, () => UnityEventTools.AddPersistentListener(GetButton("Snow").onClick, simpleUI.SetWeatherSnow));
        CreateButton(canvas.transform, "Capture PNG", left, top + 320, () => UnityEventTools.AddPersistentListener(GetButton("Capture PNG").onClick, simpleUI.Capture));

        simpleUI.pathMgr = pathMgr;
        simpleUI.weather = weather;
        simpleUI.capture = capture;
        simpleUI.spawner = spawner;

        selfTest.spawner = spawner;
        selfTest.pathMgr = pathMgr;

        EditorUtility.DisplayDialog("Quick Setup", "Setup complete.\n1) Import your environment & characters\n2) Bake NavMesh (Window->AI->Navigation)\n3) Press Play", "OK");
    }

    static T Require<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        return c ? c : go.AddComponent<T>();
    }

    static void EnsureFolder(string path)
    {
        var parts = path.Split('/');
        string current = "";
        for (int i = 0; i < parts.Length; i++)
        {
            current = i == 0 ? parts[0] : current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(current))
            {
                var parent = System.IO.Path.GetDirectoryName(current).Replace("\\", "/");
                var leaf = System.IO.Path.GetFileName(current);
                AssetDatabase.CreateFolder(parent, leaf);
            }
        }
    }

    static void CreateButton(Transform parent, string text, float centerX, float topOffset, System.Action bind)
    {
        var go = new GameObject(text);
        go.transform.SetParent(parent);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200, 32);
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(centerX, topOffset);

        var img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        var btn = go.AddComponent<Button>();

        var labelGO = new GameObject("Text");
        labelGO.transform.SetParent(go.transform);
        var tr = labelGO.AddComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;
        var txt = labelGO.AddComponent<Text>();
        txt.text = text;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.fontSize = 14;
        txt.color = Color.white;
        // Unity 2022+ 不再提供 Arial.ttf 作为内置字体，改用 LegacyRuntime.ttf
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        bind?.Invoke();
    }

    static Button GetButton(string name)
    {
        var go = GameObject.Find(name);
        return go != null ? go.GetComponent<Button>() : null;
    }
}
#endif
