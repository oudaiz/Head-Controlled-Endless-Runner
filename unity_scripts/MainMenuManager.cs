using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject playOptionsPanel;
    public GameObject warningPanel;
    public GameObject loadingPanel;

    [Header("Texts")]
    public TextMeshProUGUI warningText;
    public TextMeshProUGUI loadingText;

    [Header("Runtime Manager")]
    public VisionRuntimeManager visionRuntimeManager;

    void Start()
    {
        if (playOptionsPanel != null)
            playOptionsPanel.SetActive(false);

        if (warningPanel != null)
            warningPanel.SetActive(false);

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        if (loadingText != null)
            loadingText.text = "";
    }
    public void OpenPlayOptions()
    {
        if (playOptionsPanel != null)
            playOptionsPanel.SetActive(true);

        if (warningPanel != null)
            warningPanel.SetActive(false);

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        if (loadingText != null)
            loadingText.text = "";
    }

    public void ClosePlayOptions()
    {
        if (playOptionsPanel != null)
            playOptionsPanel.SetActive(false);
    }

    public void PlayWithKeyboard()
    {
        GameLaunchSettings.SelectedInputMode = InputMode.Keyboard;

        if (visionRuntimeManager != null)
            visionRuntimeManager.StopCameraSystem();

        if (warningPanel != null)
            warningPanel.SetActive(false);

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
    }

    public void PlayWithCamera()
    {
        StartCoroutine(PlayWithCameraRoutine());
    }

    IEnumerator PlayWithCameraRoutine()
    {
        GameLaunchSettings.SelectedInputMode = InputMode.Camera;

        if (warningPanel != null)
            warningPanel.SetActive(false);

        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        if (loadingText != null)
            loadingText.text = "Starting camera system...";

        bool finished = false;
        bool success = false;
        string message = "";

        yield return StartCoroutine(
            visionRuntimeManager.StartCameraSystem((ok, msg) =>
            {
                success = ok;
                message = msg;
                finished = true;
            })
        );

        if (!finished || !success)
        {
            if (loadingPanel != null)
                loadingPanel.SetActive(false);

            if (playOptionsPanel != null)
                playOptionsPanel.SetActive(false);

            if (warningPanel != null)
                warningPanel.SetActive(true);

            if (warningText != null)
                warningText.text = message;

            yield break;
        }

        if (loadingText != null)
            loadingText.text = "Camera ready";

        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
    }

    public void BackFromWarning()
    {
        Debug.Log("BackFromWarning called");

        if (visionRuntimeManager != null)
            visionRuntimeManager.StopCameraSystem();

        if (loadingText != null)
            loadingText.text = "";

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        if (warningPanel != null)
            warningPanel.SetActive(false);

        if (playOptionsPanel != null)
            playOptionsPanel.SetActive(true);
    }

    public void QuitGame()
    {
        if (visionRuntimeManager != null)
            visionRuntimeManager.StopCameraSystem();

    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
}