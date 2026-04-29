using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class VisionRuntimeManager : MonoBehaviour
{
    public static VisionRuntimeManager Instance;

    [Header("Python Settings")]
    public string pythonExecutable = "/usr/bin/python3";
    public string workingDirectory = "/full/path/to/python/project";
    public string arguments = "main.py";

    [Header("API Settings")]
    public string healthUrl = "http://127.0.0.1:5000/health";
    public float startupTimeout = 20f;
    public float retryDelay = 1f;

    private Process pythonProcess;

    public bool IsRunning
    {
        get
        {
            return pythonProcess != null && !pythonProcess.HasExited;
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator StartCameraSystem(Action<bool, string> onComplete)
    {
        if (pythonProcess != null && !pythonProcess.HasExited)
        {
            Debug.Log("Python already running. PID = " + pythonProcess.Id);
            yield return StartCoroutine(WaitForHealth(onComplete));
            yield break;
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = pythonExecutable,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        try
        {
            pythonProcess = new Process();
            pythonProcess.StartInfo = startInfo;
            pythonProcess.EnableRaisingEvents = true;

            pythonProcess.Exited += (sender, e) =>
            {
                Debug.Log("Python process exited.");
            };

            pythonProcess.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    Debug.Log("[PYTHON] " + e.Data);
            };

            pythonProcess.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    Debug.LogError("[PYTHON ERROR] " + e.Data);
            };

            pythonProcess.Start();
            Debug.Log("Python started. PID = " + pythonProcess.Id);

            pythonProcess.BeginOutputReadLine();
            pythonProcess.BeginErrorReadLine();
        }
        catch (Exception ex)
        {
            onComplete?.Invoke(false, "Failed to start Python: " + ex.Message);
            yield break;
        }

        yield return StartCoroutine(WaitForHealth(onComplete));
    }

    IEnumerator WaitForHealth(Action<bool, string> onComplete)
    {
        float elapsed = 0f;

        while (elapsed < startupTimeout)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(healthUrl))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    onComplete?.Invoke(true, "Camera system started successfully.");
                    yield break;
                }
            }

            elapsed += retryDelay;
            yield return new WaitForSeconds(retryDelay);
        }

        onComplete?.Invoke(false, "Camera system did not respond on /health.");
    }

    public void StopCameraSystem()
    {
        try
        {
            if (pythonProcess != null)
            {
                int pid = pythonProcess.Id;
                Debug.Log("Stopping Python process. PID = " + pid);

                if (!pythonProcess.HasExited)
                {
                    pythonProcess.Kill();
                    bool exited = pythonProcess.WaitForExit(3000);
                    Debug.Log("Python exited = " + exited);
                }

                pythonProcess.Dispose();
                pythonProcess = null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("StopCameraSystem warning: " + ex.Message);
        }
        finally
        {
            pythonProcess = null;
        }
    }

    void OnApplicationQuit()
    {
        StopCameraSystem();
    }
}