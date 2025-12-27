using UnityEngine;

public class Projectile : MonoBehaviour
{
    public enum ShapeType { Square, Circle }
    public ShapeType shapeType;
    public bool isReal;
    public float speed; // Speed is now set by the Spawner

    private AudioSource audioSource;
    private PlayerController playerController;
    private bool hasBeenHit = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            // Only the real projectile makes directional sound
            audioSource.mute = !isReal;
        }

        playerController = FindObjectOfType<PlayerController>();

        // Ensure there's a Rigidbody2D (required for trigger detection)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.isKinematic = true;
            rb.gravityScale = 0f;
        }

        // Ensure there's a collider on this projectile
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            Debug.LogWarning($"Projectile {gameObject.name} is missing a Collider2D component! Adding CircleCollider2D as default.");
            CircleCollider2D newCollider = gameObject.AddComponent<CircleCollider2D>();
            newCollider.isTrigger = true;
        }
        else
        {
            collider.isTrigger = true;
        }
    }

    void Update()
    {
        // Moves toward the center
        transform.position = Vector3.MoveTowards(transform.position, Vector3.zero, speed * Time.deltaTime);

        // Auto-destruct and register miss if real projectile hits center
        if (Vector3.Distance(transform.position, Vector3.zero) < 0.1f)
        {
            if (isReal && !hasBeenHit)
            {
                if (playerController != null) 
                    playerController.RegisterMiss();
            }
            Destroy(gameObject);
        }
    }

    public bool HasBeenHit()
    {
        return hasBeenHit;
    }

    public void OnSwordHit(bool wasCorrect)
    {
        hasBeenHit = true;
    }

    public bool IsOnLeftSide()
    {
        return transform.position.x < 0;
    }
}