using UnityEngine;

public class TrueSightPowerup : PowerupBase
{
    [Header("True Sight Settings")]
    [SerializeField] private float duration = 3f;

    protected override void ApplyEffect()
    {
        Spawner spawner = FindObjectOfType<Spawner>();
        if (spawner != null)
        {
            // Trigger the special spawning mode
            spawner.ActivateTrueSight(duration);
            Debug.Log("True Sight activated! Fake projectiles hidden.");
        }
    }

    public override bool CanSpawn(PlayerController player)
    {
        Spawner spawner = FindObjectOfType<Spawner>();
        
        // Spawn at full health, but don't overlap if active
        return player != null && !player.IsDead() && 
               spawner != null && !spawner.IsTrueSightActive();
    }
}