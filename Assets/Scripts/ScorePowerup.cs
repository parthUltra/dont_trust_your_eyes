using UnityEngine;

/// <summary>
/// Powerup that increases the player's score multiplier by +1.
/// </summary>
public class ScorePowerup : PowerupBase
{
    protected override void ApplyEffect()
    {
        if (playerController != null)
        {
            playerController.AddMultiplier();
            Debug.Log("Multiplier Increased! Current: x" + playerController.scoreMultiplier);
        }
    }
    
    public override bool CanSpawn(PlayerController player)
    {
        // Can always spawn if player is alive
        return player != null && !player.IsDead();
    }
}