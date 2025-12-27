using UnityEngine;

/// <summary>
/// Base class for all powerups. Extend this to create new powerup types.
/// </summary>
public abstract class PowerupBase : MonoBehaviour
{
    [Header("Powerup Settings")]
    [SerializeField] protected float lifetime = 5f; // How long before the powerup despawns
    [SerializeField] protected float despawnAnimDuration = 0.5f;
    
    [Header("Audio")]
    [SerializeField] protected AudioClip collectSound;
    
    protected PlayerController playerController;
    protected bool isCollected = false;
    private float spawnTime;
    
    protected virtual void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerController not found in scene!");
        }
        
        spawnTime = Time.time;
    }
    
    protected virtual void Update()
    {
        // Check for lifetime expiration
        if (!isCollected && Time.time - spawnTime >= lifetime)
        {
            Despawn();
        }
        
        // Check for mouse click
        if (UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame || 
            UnityEngine.InputSystem.Mouse.current.rightButton.wasPressedThisFrame)
        {
            CheckClick();
        }
    }
    
    protected virtual void CheckClick()
    {
        if (isCollected) return;
        
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
        mousePos.z = transform.position.z;
        
        // Check if click is within the powerup's collider
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null && collider.OverlapPoint(mousePos))
        {
            Collect();
        }
    }
    
    protected virtual void Collect()
    {
        if (isCollected) return;
        
        isCollected = true;
        
        // Play collection sound
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
        
        // Apply the powerup effect
        ApplyEffect();
        
        // Destroy the powerup
        Destroy(gameObject);
    }
    
    protected virtual void Despawn()
    {
        // Can be overridden for custom despawn animation
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Override this method to define what the powerup does when collected
    /// </summary>
    protected abstract void ApplyEffect();
    
    /// <summary>
    /// Override this to check if the powerup should spawn (e.g., only spawn hearts when player is damaged)
    /// </summary>
    /// <param name="player">Reference to the player controller</param>
    /// <returns>True if this powerup type can spawn now</returns>
    public abstract bool CanSpawn(PlayerController player);
}

