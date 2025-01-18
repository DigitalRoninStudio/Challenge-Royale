using UnityEngine;

public static class NetworkLogger
{
    public static bool IsDebugEnabled { get; set; } = true;
    public static void Log(string message)
    {
        if (IsDebugEnabled)
            Debug.Log($"[Network] {message}");
    }

    public static void LogWarning(string message)
    {
        if (IsDebugEnabled)
            Debug.LogWarning($"[Network] {message}");
    }

    public static void LogError(string message)
    {
        if (IsDebugEnabled)
            Debug.LogError($"[Network] {message}");
    }
}
