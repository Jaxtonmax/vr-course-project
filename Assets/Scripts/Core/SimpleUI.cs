using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class SimpleUI : MonoBehaviour
{
    public PathManager pathMgr;
    public WeatherManager weather;
    public CameraCapture capture;
    public CrowdSpawner spawner;

    public void SelectAllAgents()
    {
        if (pathMgr == null)
        {
            Debug.LogWarning("SimpleUI.SelectAllAgents: pathMgr is null.");
            return;
        }

        List<NavMeshAgent> agents;
        // 优先使用 CrowdSpawner 生成的列表；如果没有，就自动查找场景中的所有 NavMeshAgent
        if (spawner != null && spawner.SpawnedAgents != null && spawner.SpawnedAgents.Count > 0)
        {
            agents = spawner.SpawnedAgents;
        }
        else
        {
            agents = new List<NavMeshAgent>(FindObjectsOfType<NavMeshAgent>());
        }

        pathMgr.SelectAgents(agents);
        Debug.Log($"SelectAllAgents: current agents = {agents.Count}");
    }

    public void SetStartFromClick()
    {
        if (pathMgr == null)
        {
            Debug.LogWarning("SimpleUI.SetStartFromClick: pathMgr is null.");
            return;
        }
        Debug.Log("SetStartFromClick: waiting for ground click.");
        pathMgr.SetStartFromClick();
    }

    public void SetGoalFromClickAndApply()
    {
        if (pathMgr == null)
        {
            Debug.LogWarning("SimpleUI.SetGoalFromClickAndApply: pathMgr is null.");
            return;
        }
        Debug.Log("SetGoalFromClickAndApply: waiting for ground click.");
        pathMgr.SetGoalFromClickAndApply(true);
    }

    public void SetWeatherSunny() => weather.SetWeather(WeatherType.Sunny);
    public void SetWeatherNight() => weather.SetWeather(WeatherType.Night);
    public void SetWeatherFoggy() => weather.SetWeather(WeatherType.Foggy);
    public void SetWeatherSnow() => weather.SetWeather(WeatherType.Snow);

    public void Capture() => capture.CaptureOnce();
}
