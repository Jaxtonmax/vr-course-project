using UnityEngine;
using System;
using System.IO;

[RequireComponent(typeof(Camera))]
public class CameraCapture : MonoBehaviour
{
    public int width = 1920, height = 1080;
    public float fov = 60f;
    public string outputDir = "";

    Camera cam;
    RenderTexture rt;
    Texture2D tex;

    void Awake()
    {
        cam = GetComponent<Camera>();
        rt = new RenderTexture(width, height, 24);
        tex = new Texture2D(width, height, TextureFormat.RGB24, false);

        if (string.IsNullOrWhiteSpace(outputDir))
        {
            string pics = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            if (string.IsNullOrWhiteSpace(pics)) pics = Application.persistentDataPath;
            outputDir = Path.Combine(pics, "VRProjectCaptures");
        }
    }

    public void CaptureOnce(string fileName = null)
    {
        Directory.CreateDirectory(outputDir);
        cam.fieldOfView = fov;
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();
        cam.targetTexture = null;
        RenderTexture.active = null;
        var bytes = tex.EncodeToPNG();
        var path = Path.Combine(outputDir, fileName ?? $"cap_{DateTime.Now:yyyyMMdd_HHmmss}.png");
        File.WriteAllBytes(path, bytes);
        Debug.Log($"Saved: {path}");
    }

    void OnDestroy()
    {
        if (rt) rt.Release();
    }
}