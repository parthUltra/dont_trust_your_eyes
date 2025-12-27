using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;
    public AudioClip buttonClickSound;
    
    private AudioSource audioSource;
    private const string HIGH_SCORE_KEY = "HighScore";

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        UpdateScoreDisplay();
    }

    public void UpdateScoreDisplay()
    {
        // Get the current score from PlayerController
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            int currentScore = player.score;
            int highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);

            // Update high score if current score is higher
            if (currentScore > highScore)
            {
                highScore = currentScore;
                PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
                PlayerPrefs.Save();
            }

            // Update UI
            if (finalScoreText != null)
            {
                finalScoreText.text = "Final Score: " + currentScore;
            }

            if (highScoreText != null)
            {
                highScoreText.text = "High Score: " + highScore;
            }
        }
    }

    public void RestartGame()
    {
        PlayButtonSound();
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        PlayButtonSound();
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }

    private void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }
}

