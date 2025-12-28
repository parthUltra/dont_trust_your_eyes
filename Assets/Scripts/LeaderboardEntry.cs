using UnityEngine;
using TMPro;

/// <summary>
/// Simple component for leaderboard entry display
/// Attach this to your leaderboard entry prefab
/// </summary>
public class LeaderboardEntry : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI scoreText;

    public void SetEntry(int rank, string username, int score, bool isPlayer = false)
    {
        if (rankText != null)
            rankText.text = "#" + rank.ToString();
        
        if (usernameText != null)
            usernameText.text = username;
        
        if (scoreText != null)
            scoreText.text = score.ToString();
    }
}

