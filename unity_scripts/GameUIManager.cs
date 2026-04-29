using UnityEngine;
using TMPro;
using System.Collections;

public class GameUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject gameOverPanel;

    [Header("Texts")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI bestScoreText;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI titleText;

    [Header("Live HUD")]
    public TextMeshProUGUI scoreLiveText;
    public TextMeshProUGUI coinsLiveText;

    [Header("Countdown")]
    public TextMeshProUGUI countdownText;

    [Header("Animation")]
    public float panelAnimSpeed = 4f;

    private RectTransform panelRect;

    void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            panelRect = gameOverPanel.GetComponent<RectTransform>();
        }
    }

    public void UpdateHUD(int score, int coins)
    {
        if (scoreLiveText != null)
            scoreLiveText.text = "Score: " + score;

        if (coinsLiveText != null)
            coinsLiveText.text = "Coins: " + coins;
    }

    public IEnumerator ShowCountdown(System.Action onFinish)
    {
        int count = 3;

        if (countdownText != null)
            countdownText.gameObject.SetActive(true);

        while (count > 0)
        {
            if (countdownText != null)
                countdownText.text = count.ToString();

            yield return new WaitForSeconds(1f);
            count--;
        }

        if (countdownText != null)
            countdownText.text = "GO!";

        yield return new WaitForSeconds(0.5f);

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        onFinish?.Invoke();
    }

    public void ShowGameOver(int score, int coins)
    {
        int bestScore = PlayerPrefs.GetInt("HighScore", 0);

        if (score > bestScore)
        {
            bestScore = score;
            PlayerPrefs.SetInt("HighScore", bestScore);
            PlayerPrefs.Save();
        }

        if (titleText != null)
            titleText.text = "GAME OVER";

        if (scoreText != null)
            scoreText.text = "Score: " + score;

        if (bestScoreText != null)
            bestScoreText.text = "Best Score: " + bestScore;

        if (coinsText != null)
            coinsText.text = "Coins: " + coins;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            if (panelRect != null)
            {
                panelRect.localScale = Vector3.zero;
                StartCoroutine(AnimatePanel());
            }
        }

        Time.timeScale = 0f;
    }

    IEnumerator AnimatePanel()
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * panelAnimSpeed;

            if (panelRect != null)
                panelRect.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);

            yield return null;
        }

        if (panelRect != null)
            panelRect.localScale = Vector3.one;
    }
}