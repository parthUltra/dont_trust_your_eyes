using UnityEngine;

/// <summary>
/// Heart powerup that heals the player by restoring one life
/// </summary>
public class HeartPowerup : PowerupBase
{
    protected override void ApplyEffect()
    {
        if (playerController != null)
        {
            playerController.Heal();
            Debug.Log("Heart collected! Life restored.");
        }
    }
    
    public override bool CanSpawn(PlayerController player)
    {
        // Only spawn hearts if player has less than 3 lives (i.e., has taken damage)
        return player != null && player.misses > 0;
    }
}

