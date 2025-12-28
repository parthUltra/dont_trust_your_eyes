using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI finalScoreText;
    public AudioClip buttonClickSound;
    
    [Header("Leaderboard")]
    public LeaderboardManager leaderboardManager;
    
    private AudioSource audioSource;

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

            // Update UI
            if (finalScoreText != null)
            {
                finalScoreText.text = "Final Score: " + currentScore;
            }
            
            // Set score for leaderboard manager
            if (leaderboardManager != null)
            {
                leaderboardManager.SetScore(currentScore);
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

