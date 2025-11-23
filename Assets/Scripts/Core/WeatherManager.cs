using UnityEngine;

public enum WeatherType { Sunny, Night, Foggy, Snow }

public class WeatherManager : MonoBehaviour
{
    public Light sun;
    public Material skySunny, skyNight, skyFog;
    public ParticleSystem rain, snow;
    public Color fogColor = new Color(0.7f, 0.7f, 0.8f);
    public float fogDensity = 0.01f;
    public Color sunnyAmbient = new Color(0.9f, 0.9f, 0.9f);
    public Color nightAmbient = new Color(0.1f, 0.1f, 0.2f);
    public Color foggyAmbient = new Color(0.6f, 0.6f, 0.65f);
    public Color snowAmbient = new Color(0.85f, 0.85f, 0.9f);

    Material defaultSkybox;

    void Awake()
    {
        if (sun == null)
        {
            sun = RenderSettings.sun;
        }
        defaultSkybox = RenderSettings.skybox;
    }

    public void SetWeather(WeatherType t)
    {
        ToggleParticles(false, false);
        RenderSettings.fog = false;

        switch (t)
        {
            case WeatherType.Sunny:
                ApplyWeather(skySunny, 1.1f, sunnyAmbient, false, Color.clear, 0f);
                break;
            case WeatherType.Night:
                ApplyWeather(skyNight, 0.15f, nightAmbient, false, Color.clear, 0f);
                break;
            case WeatherType.Foggy:
                ApplyWeather(skyFog, 0.6f, foggyAmbient, true, fogColor, fogDensity);
                break;
            case WeatherType.Snow:
                ApplyWeather(skySunny, 0.9f, snowAmbient, true, new Color(0.85f, 0.9f, 0.95f), fogDensity * 0.7f);
                ToggleParticles(false, true);
                break;
        }
        DynamicGI.UpdateEnvironment();
    }

    void ApplyWeather(Material sky, float sunIntensity, Color ambient, bool fogEnabled, Color fogClr, float fogDen)
    {
        RenderSettings.skybox = sky ?? defaultSkybox;
        RenderSettings.ambientLight = ambient;

        if (sun != null)
        {
            sun.intensity = sunIntensity;
            sun.color = Color.white;
        }

        RenderSettings.fog = fogEnabled;
        if (fogEnabled)
        {
            RenderSettings.fogColor = fogClr;
            RenderSettings.fogDensity = fogDen;
        }
    }

    void ToggleParticles(bool rainOn, bool snowOn)
    {
        if (rain) rain.gameObject.SetActive(rainOn);
        if (snow) snow.gameObject.SetActive(snowOn);
    }
}
