using UnityEngine;

public class PerformanceManager : MonoBehaviour
{
    [Tooltip("Target frame rate (e.g. 180)")]
    public int targetFrameRate = 180;

    void Awake()
    {
        // Disable VSync so targetFrameRate is respected
        QualitySettings.vSyncCount = 0;

        // Set target FPS
        Application.targetFrameRate = targetFrameRate;

        // Optional: Log GPU support status
        Debug.Log("GPU Support: " + SystemInfo.graphicsDeviceType);
        Debug.Log("GPU Name: " + SystemInfo.graphicsDeviceName);
        Debug.Log("GPU Memory: " + SystemInfo.graphicsMemorySize + " MB");
        Debug.Log("GPU Shader Level: " + SystemInfo.graphicsShaderLevel);
        Debug.Log("GPU Supports GPU Instancing: " + SystemInfo.supportsInstancing);
    }
}
