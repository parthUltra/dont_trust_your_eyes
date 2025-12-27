using UnityEngine;

/// <summary>
/// Handles collision detection between the sword and projectiles
/// </summary>
public class SwordHitbox : MonoBehaviour
{
    [Header("Debug")]
    public bool showDebugGizmo = true;
    public Color debugColor = Color.red;

    private Projectile.ShapeType currentAttackType;
    private PlayerController playerController;
    private bool facingRight;

    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
    }

    public void SetAttackType(Projectile.ShapeType attackType, bool isFacingRight)
    {
        currentAttackType = attackType;
        facingRight = isFacingRight;
        
        Debug.Log($"SwordHitbox set: AttackType={attackType}, FacingRight={facingRight}");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"SwordHitbox collided with: {other.gameObject.name}");
        
        Projectile projectile = other.GetComponent<Projectile>();
        if (projectile != null && !projectile.HasBeenHit())
        {
            Debug.Log($"Hit projectile! IsReal: {projectile.isReal}, Type: {projectile.shapeType}, AttackType: {currentAttackType}");
            
            // Check if this is the correct match
            bool isLeftSide = projectile.transform.position.x < 0;
            bool correctSide = (facingRight && !isLeftSide) || (!facingRight && isLeftSide);
            
            Debug.Log($"Projectile side: {(isLeftSide ? "Left" : "Right")}, Sword facing: {(facingRight ? "Right" : "Left")}, CorrectSide: {correctSide}");
            
            bool correctMatch = projectile.isReal 
                && projectile.shapeType == currentAttackType 
                && correctSide;

            Debug.Log($"CorrectMatch: {correctMatch}");

            // Mark projectile as hit and notify player controller
            projectile.OnSwordHit(correctMatch);
            
            if (playerController != null)
            {
                playerController.ProcessProjectileClick(correctMatch);
            }

            // Clear all projectiles
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

    void OnDrawGizmos()
    {
        if (!showDebugGizmo) return;

        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            Gizmos.color = debugColor;
            
            // Draw the collider bounds
            Vector3 center = transform.position + (Vector3)boxCollider.offset;
            Vector3 size = boxCollider.size;
            Gizmos.DrawWireCube(center, size);
            
            // Draw a filled circle at the center
            Gizmos.DrawSphere(center, 0.1f);
            
            // Draw corner markers
            Gizmos.DrawSphere(center + new Vector3(size.x/2, size.y/2, 0), 0.05f);
            Gizmos.DrawSphere(center + new Vector3(-size.x/2, size.y/2, 0), 0.05f);
            Gizmos.DrawSphere(center + new Vector3(size.x/2, -size.y/2, 0), 0.05f);
            Gizmos.DrawSphere(center + new Vector3(-size.x/2, -size.y/2, 0), 0.05f);
        }
    }
}

