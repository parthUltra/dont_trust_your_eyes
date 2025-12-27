using UnityEngine;
using UnityEngine.InputSystem;

public class Projectile : MonoBehaviour
{
    public enum ShapeType { Square, Circle }
    public ShapeType shapeType;
    public bool isReal;
    public float speed; // Speed is now set by the Spawner

    private AudioSource audioSource;
    private PlayerController playerController;
    private bool hasBeenClicked = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            // Only the real projectile makes directional sound
            audioSource.mute = !isReal;
        }

        playerController = FindObjectOfType<PlayerController>();

        // Ensure there's a collider on this projectile
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            Debug.LogWarning($"Projectile {gameObject.name} is missing a Collider2D component! Adding CircleCollider2D as default.");
            gameObject.AddComponent<CircleCollider2D>();
        }
    }

    void Update()
    {
        // Moves toward the center
        transform.position = Vector3.MoveTowards(transform.position, Vector3.zero, speed * Time.deltaTime);

        // Check for mouse clicks
        if (!hasBeenClicked && playerController != null && !playerController.IsDead())
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                CheckClick(ShapeType.Square);
            }
            else if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                CheckClick(ShapeType.Circle);
            }
        }

        // Auto-destruct and register miss if real projectile hits center
        if (Vector3.Distance(transform.position, Vector3.zero) < 0.1f)
        {
            if (isReal && !hasBeenClicked)
            {
                if (playerController != null) 
                    playerController.RegisterMiss();
            }
            Destroy(gameObject);
        }
    }

    void CheckClick(ShapeType clickedType)
    {
        if (hasBeenClicked) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePos.z = transform.position.z;

        // Check if click is within this projectile's collider
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null && collider.OverlapPoint(mousePos))
        {
            hasBeenClicked = true;

            // Determine which side this projectile is on
            bool isLeftSide = transform.position.x < 0;

            // Determine which side was clicked
            bool clickedLeft = mousePos.x < 0;

            // Check if this is the correct match
            bool correctMatch = (isReal && shapeType == clickedType && isLeftSide == clickedLeft);

            if (playerController != null)
            {
                playerController.ProcessProjectileClick(correctMatch);
            }

            // Destroy this and all other projectiles on the screen
            ClearAllProjectiles();
        }
    }

    void ClearAllProjectiles()
    {
        Projectile[] allProjectiles = FindObjectsOfType<Projectile>();
        foreach (Projectile p in allProjectiles)
        {
            Destroy(p.gameObject);
        }
    }

    public bool IsOnLeftSide()
    {
        return transform.position.x < 0;
    }
}