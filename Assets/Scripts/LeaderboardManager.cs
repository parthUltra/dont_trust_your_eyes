using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Dan.Main;
using Dan.Models;

public class LeaderboardManager : MonoBehaviour
{
    [Header("Leaderboard Settings")]
    [Tooltip("Enter your public leaderboard key here")]
    public string publicLeaderboardKey = "";

    [Header("UI References")]
    public TMP_InputField usernameInput;
    public Button submitButton;
    public TextMeshProUGUI errorText;
    public TextMeshProUGUI statusText;
    public Transform leaderboardContent;
    public GameObject leaderboardEntryPrefab;
    public ScrollRect leaderboardScrollRect;

    [Header("Audio")]
    public AudioClip submitSound;
    private AudioSource audioSource;

    private int currentScore;
    private bool hasSubmittedThisRound = false;
    private bool isLoadingLeaderboard = false;
    private List<GameObject> leaderboardEntries = new List<GameObject>();

    void Start()
    {
        Debug.Log("LeaderboardManager Start() called");
        audioSource = gameObject.AddComponent<AudioSource>();

        if (submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmitScore);
        }

        if (usernameInput != null)
        {
            usernameInput.onValueChanged.AddListener(OnUsernameChanged);
        }

        if (errorText != null)
        {
            errorText.text = "";
        }

        if (statusText != null)
        {
            statusText.text = "";
        }

        LoadLeaderboard();
    }

    public void SetScore(int score)
    {
        currentScore = score;
        // Reset submission state for new game round
        hasSubmittedThisRound = false;

        // Re-enable submit button
        if (submitButton != null)
        {
            submitButton.interactable = true;
        }
    }

    void OnUsernameChanged(string value)
    {
        if (errorText != null)
        {
            errorText.text = "";
        }
    }

    public void OnSubmitScore()
    {
        // Check if already submitted this round
        if (hasSubmittedThisRound)
        {
            ShowError("You already submitted your score for this round!");
            return;
        }

        string username = usernameInput != null ? usernameInput.text : "";

        // Validate username
        if (!ProfanityFilter.IsValidUsername(username, out string errorMessage))
        {
            ShowError(errorMessage);
            return;
        }

        // Disable submit button
        if (submitButton != null)
        {
            submitButton.interactable = false;
        }

        // Show status
        if (statusText != null)
        {
            statusText.text = "Submitting...";
        }

        // Submit to leaderboard
        UploadScore(username, currentScore);
    }

    void UploadScore(string username, int score)
    {
        LeaderboardCreator.UploadNewEntry(publicLeaderboardKey, username, score, (isSuccess) =>
        {
            if (isSuccess)
            {
                hasSubmittedThisRound = true;

                if (statusText != null)
                {
                    statusText.text = "Score submitted successfully!";
                    statusText.color = Color.green;
                }

                if (errorText != null)
                {
                    errorText.text = "";
                }

                if (submitSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(submitSound);
                }

                // Keep button disabled - only one submission per round
                if (submitButton != null)
                {
                    submitButton.interactable = false;
                }

                // Clear the input field
                if (usernameInput != null)
                {
                    usernameInput.text = "";
                }

                // Reload leaderboard with a small delay to allow server to process
                StartCoroutine(ReloadLeaderboardAfterDelay(1.5f));
            }
            else
            {
                ShowError("Failed to submit score. Try again.");
                if (submitButton != null)
                {
                    submitButton.interactable = true;
                }
                if (statusText != null)
                {
                    statusText.text = "";
                }
            }
        }, (errorMsg) =>
        {
            ShowError("Error: " + errorMsg);
            if (submitButton != null)
            {
                submitButton.interactable = true;
            }
            if (statusText != null)
            {
                statusText.text = "";
            }
        });
    }

    IEnumerator ReloadLeaderboardAfterDelay(float delay)
    {
        Debug.Log($"ReloadLeaderboardAfterDelay started - waiting {delay} seconds");

        if (statusText != null)
        {
            statusText.text = "Score submitted! Updating leaderboard...";
        }

        yield return new WaitForSecondsRealtime(delay);

        Debug.Log("ReloadLeaderboardAfterDelay calling LoadLeaderboard now");
        LoadLeaderboard();
    }

    public void LoadLeaderboard()
    {
        Debug.Log("LoadLeaderboard called");

        // Prevent multiple simultaneous loads
        if (isLoadingLeaderboard)
        {
            Debug.LogWarning("Leaderboard is already loading, skipping duplicate request");
            return;
        }

        isLoadingLeaderboard = true;

        // Clear existing entries
        ClearLeaderboard();

        if (string.IsNullOrEmpty(publicLeaderboardKey))
        {
            ShowError("Leaderboard key not set in Inspector!");
            isLoadingLeaderboard = false;
            return;
        }

        // Show loading message
        if (statusText != null)
        {
            if (hasSubmittedThisRound)
            {
                statusText.text = "Updating leaderboard...";
            }
            else
            {
                statusText.text = "Loading leaderboard...";
            }
            statusText.color = Color.white;
        }

        LeaderboardCreator.GetLeaderboard(publicLeaderboardKey, (entries) =>
        {
            isLoadingLeaderboard = false;

            if (entries != null && entries.Length > 0)
            {
                DisplayLeaderboard(entries);

                // Update status message after loading
                if (statusText != null)
                {
                    if (hasSubmittedThisRound)
                    {
                        statusText.text = "Score submitted! Play again to submit another.";
                        statusText.color = Color.green;
                    }
                    else
                    {
                        statusText.text = "";
                    }
                }
            }
            else
            {
                if (statusText != null)
                {
                    statusText.text = "No entries yet. Be the first!";
                    statusText.color = Color.white;
                }
            }
        }, (errorMsg) =>
        {
            isLoadingLeaderboard = false;
            ShowError("Failed to load leaderboard: " + errorMsg);
        });
    }

    void DisplayLeaderboard(Entry[] entries)
    {
        if (leaderboardContent == null)
        {
            Debug.LogError("Leaderboard Content is not assigned! Please assign it in the Inspector.");
            return;
        }

        if (leaderboardEntryPrefab == null)
        {
            Debug.LogError("Leaderboard Entry Prefab is not assigned! Please assign it in the Inspector.");
            return;
        }

        // Take top 10 entries
        int count = Mathf.Min(10, entries.Length);
        Debug.Log($"DisplayLeaderboard called - Creating {count} entries. Content currently has {leaderboardContent.childCount} children");

        for (int i = 0; i < count; i++)
        {
            GameObject entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContent);
            leaderboardEntries.Add(entryObj);

            // Find text components
            TextMeshProUGUI[] texts = entryObj.GetComponentsInChildren<TextMeshProUGUI>();

            if (texts.Length >= 3)
            {
                texts[0].text = "#" + entries[i].Rank.ToString();
                texts[1].text = entries[i].Username;
                texts[2].text = entries[i].Score.ToString();

                Debug.Log($"Entry {i + 1}: Rank {entries[i].Rank}, {entries[i].Username}, Score: {entries[i].Score}");
            }
            else
            {
                Debug.LogWarning($"Leaderboard entry prefab should have at least 3 TextMeshProUGUI components, found {texts.Length}");
            }
        }

        Debug.Log($"DisplayLeaderboard finished - Content now has {leaderboardContent.childCount} children");
    }

    void ClearLeaderboard()
    {
        if (leaderboardContent == null) return;

        Debug.Log($"Clearing leaderboard - tracked entries: {leaderboardEntries.Count}, actual children: {leaderboardContent.childCount}");

        // Clear tracked list
        leaderboardEntries.Clear();

        // Destroy ALL children of the content (more robust)
        while (leaderboardContent.childCount > 0)
        {
            Transform child = leaderboardContent.GetChild(0);
            DestroyImmediate(child.gameObject);
        }

        Debug.Log("Leaderboard cleared completely");
    }

    void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.color = Color.red;
        }

        if (statusText != null)
        {
            statusText.text = "";
        }
    }
}

