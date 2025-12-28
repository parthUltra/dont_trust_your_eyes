using UnityEngine;

/// <summary>
/// True Sight powerup that forces the spawner to skip fake projectiles.
/// </summary>
public class TrueSightPowerup : PowerupBase
{
    [Header("True Sight Settings")]
    [SerializeField] private float duration = 3f;

    protected override void ApplyEffect()
    {
        Spawner spawner = FindObjectOfType<Spawner>();
        if (spawner != null)
        {
            spawner.ActivateTrueSight(duration);
            Debug.Log("True Sight activated! Fake projectiles hidden.");
        }
    }

    public override bool CanSpawn(PlayerController player)
    {
        Spawner spawner = FindObjectOfType<Spawner>();
        
        // Allowed to spawn at full health, but not if True Sight is already active
        return player != null && !player.IsDead() && 
               spawner != null && !spawner.IsTrueSightActive();
    }
}