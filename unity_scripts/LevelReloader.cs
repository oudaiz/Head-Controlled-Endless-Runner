using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelReloader : MonoBehaviour
{
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;

        if (VisionRuntimeManager.Instance != null)
            VisionRuntimeManager.Instance.StopCameraSystem();

        SceneManager.LoadScene("MainMenu");
    }
}