using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject mainPanel;
    public GameObject creditsPanel;
    public GameObject instructionsPanel;
    
    [Header("Audio")]
    public AudioClip buttonClickSound;
    private AudioSource audioSource;

    void Start()
    {
        // Ensure time is running in menu
        Time.timeScale = 1;
        
        // Setup audio
        audioSource = gameObject.AddComponent<AudioSource>();
        
        // Show main panel by default
        ShowMainMenu();
    }

    public void PlayGame()
    {
        PlayButtonSound();
        SceneManager.LoadScene("SampleScene");
    }

    public void ShowInstructions()
    {
        PlayButtonSound();
        if (mainPanel != null) mainPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (instructionsPanel != null) instructionsPanel.SetActive(true);
    }

    public void ShowCredits()
    {
        PlayButtonSound();
        if (mainPanel != null) mainPanel.SetActive(false);
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }

    public void ShowMainMenu()
    {
        PlayButtonSound();
        if (mainPanel != null) mainPanel.SetActive(true);
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        PlayButtonSound();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }
}

