using UnityEngine;
using UnityEngine.AI;

public class RequirementChecklist : MonoBehaviour
{
    string[] lines;

    void OnGUI()
    {
        if (lines == null || Time.frameCount % 30 == 0) UpdateLines();
        var style = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft };
        GUILayout.BeginArea(new Rect(10, 10, 350, 260), "Checklist", style);
        if (lines != null)
        {
            foreach (var l in lines) GUILayout.Label(l);
        }
        GUILayout.EndArea();
    }

    void UpdateLines()
    {
        int lights = FindObjectsOfType<Light>().Length;
        int particles = FindObjectsOfType<ParticleSystem>().Length;
        int agents = FindObjectsOfType<NavMeshAgent>().Length;
        bool hasWeather = FindObjectOfType<WeatherManager>() != null;
        bool hasPathMgr = FindObjectOfType<PathManager>() != null;
        bool hasCapture = FindObjectOfType<CameraCapture>() != null;
        bool hasOffMesh = FindObjectsOfType<OffMeshLink>().Length > 0;

        lines = new[]
        {
            Check(lights >= 2, ">=2 lights (Directional + Point/Spot)"),
            Check(particles >= 1, ">=1 Particle (fountain/fire/rain/snow)"),
            Check(hasWeather, "Weather Manager (Sunny/Night/Foggy/Snow)"),
            Check(agents >= 5, ">=5 agents moving"),
            Check(hasOffMesh, "Off-Mesh Link present (for jump)"),
            Check(hasPathMgr, "PathManager active (global path & visualization)"),
            Check(hasCapture, "CameraCapture for PNG output"),
            "Tip: Bake NavMesh (Window->AI->Navigation->Bake)."
        };
    }

    string Check(bool ok, string name) => $"{(ok ? "[OK] " : "[..] ")}{name}";
}