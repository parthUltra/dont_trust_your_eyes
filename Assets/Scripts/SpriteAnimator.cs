using UnityEngine;
using System.Collections;

public class SpriteAnimator : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Sprite[] currentAnimation;
    private int currentFrame = 0;
    private float frameTimer = 0f;
    private bool isPlaying = false;
    private bool loop = true;
    private System.Action onAnimationComplete;

    [Header("Animation Settings")]
    public float framesPerSecond = 10f;

    [Header("Animation Sprites")]
    public Sprite[] idleSprites;
    public Sprite[] attackSprites;
    public Sprite[] attack2Sprites;
    public Sprite[] hitSprites;
    public Sprite[] deathSprites;

    [Header("Sword Hitbox Settings")]
    [Tooltip("The GameObject with collider that acts as the sword hitbox")]
    public GameObject swordHitbox;
    [Tooltip("Show debug visualization of sword hitbox in game")]
    public bool showDebugHitbox = true;
    [Tooltip("Which frames of Attack animation should the hitbox be active")]
    public Vector2Int attackHitboxFrames = new Vector2Int(2, 4);
    [Tooltip("Which frames of Attack2 animation should the hitbox be active")]
    public Vector2Int attack2HitboxFrames = new Vector2Int(2, 4);
    [Tooltip("Position offset of hitbox when facing right")]
    public Vector2 hitboxOffsetRight = new Vector2(0.3f, 0f);
    [Tooltip("Position offset of hitbox when facing left")]
    public Vector2 hitboxOffsetLeft = new Vector2(-0.3f, 0f);

    private AnimationState currentState = AnimationState.Idle;
    private AnimationState previousState = AnimationState.Idle;
    private bool facingRight = true;
    private SwordHitbox swordHitboxScript;

    public enum AnimationState
    {
        Idle,
        Attack,
        Attack2,
        Hit,
        Death
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
    }

    void Start()
    {
        spriteRenderer.flipX = !facingRight;
        
        // Setup sword hitbox if not assigned
        if (swordHitbox == null)
        {
            CreateSwordHitbox();
        }
        else
        {
            swordHitboxScript = swordHitbox.GetComponent<SwordHitbox>();
            if (swordHitboxScript == null)
            {
                swordHitboxScript = swordHitbox.AddComponent<SwordHitbox>();
            }
            
            // Update debug visualization for pre-existing hitbox
            UpdateDebugVisualization();
        }
        
        if (swordHitbox != null)
        {
            swordHitbox.SetActive(false);
        }
        
        PlayAnimation(AnimationState.Idle, true);
    }

    void UpdateDebugVisualization()
    {
        if (swordHitbox == null) return;
        
        SpriteRenderer debugSprite = swordHitbox.GetComponent<SpriteRenderer>();
        
        if (showDebugHitbox && debugSprite == null)
        {
            // Add debug visualization
            debugSprite = swordHitbox.AddComponent<SpriteRenderer>();
            debugSprite.sortingOrder = 100;
            
            Texture2D texture = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color(1f, 0f, 0f, 0.5f);
            }
            texture.SetPixels(pixels);
            texture.Apply();
            
            Sprite debugSpriteAsset = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 16f);
            debugSprite.sprite = debugSpriteAsset;
            
            BoxCollider2D collider = swordHitbox.GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                debugSprite.transform.localScale = new Vector3(collider.size.x / 2f, collider.size.y / 2f, 1f);
            }
        }
        else if (!showDebugHitbox && debugSprite != null)
        {
            // Remove debug visualization
            Destroy(debugSprite);
        }
    }

    void CreateSwordHitbox()
    {
        swordHitbox = new GameObject("SwordHitbox");
        swordHitbox.transform.SetParent(transform);
        swordHitbox.transform.localPosition = hitboxOffsetRight;
        
        // Add a Rigidbody2D (required for trigger detection)
        Rigidbody2D rb = swordHitbox.AddComponent<Rigidbody2D>();
        rb.isKinematic = true;
        rb.gravityScale = 0f;
        
        // Add a box collider for the sword
        BoxCollider2D collider = swordHitbox.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.5f, 0.5f);
        collider.isTrigger = true;
        
        // Add visual debug sprite if enabled
        if (showDebugHitbox)
        {
            SpriteRenderer debugSprite = swordHitbox.AddComponent<SpriteRenderer>();
            debugSprite.sortingOrder = 100; // Render on top
            
            // Create a simple square texture
            Texture2D texture = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color(1f, 0f, 0f, 0.5f); // Semi-transparent red
            }
            texture.SetPixels(pixels);
            texture.Apply();
            
            Sprite debugSpriteAsset = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 16f);
            debugSprite.sprite = debugSpriteAsset;
            debugSprite.transform.localScale = new Vector3(collider.size.x / 2f, collider.size.y / 2f, 1f);
        }
        
        // Add the SwordHitbox script
        swordHitboxScript = swordHitbox.AddComponent<SwordHitbox>();
        
        swordHitbox.SetActive(false);
    }

    void Update()
    {
        if (!isPlaying || currentAnimation == null || currentAnimation.Length == 0)
        {
            return;
        }

        frameTimer += Time.deltaTime;
        float frameLength = 1f / framesPerSecond;

        if (frameTimer >= frameLength)
        {
            frameTimer -= frameLength;
            currentFrame++;

            if (currentFrame >= currentAnimation.Length)
            {
                if (loop)
                {
                    currentFrame = 0;
                }
                else
                {
                    currentFrame = currentAnimation.Length - 1;
                    isPlaying = false;
                    onAnimationComplete?.Invoke();
                    onAnimationComplete = null;

                    if (currentState != AnimationState.Idle && currentState != AnimationState.Death)
                    {
                        PlayAnimation(AnimationState.Idle, true);
                    }
                    return;
                }
            }

            UpdateSprite();
            UpdateSwordHitbox();
        }
    }

    void UpdateSwordHitbox()
    {
        if (swordHitbox == null) return;

        bool shouldBeActive = false;
        
        // Check if we're in an attack animation and on the right frame
        if (currentState == AnimationState.Attack)
        {
            shouldBeActive = currentFrame >= attackHitboxFrames.x && currentFrame <= attackHitboxFrames.y;
        }
        else if (currentState == AnimationState.Attack2)
        {
            shouldBeActive = currentFrame >= attack2HitboxFrames.x && currentFrame <= attack2HitboxFrames.y;
        }

        // Update hitbox state and position
        if (shouldBeActive != swordHitbox.activeSelf)
        {
            swordHitbox.SetActive(shouldBeActive);
            
            // Update attack type when activating
            if (shouldBeActive && swordHitboxScript != null)
            {
                Projectile.ShapeType attackType = currentState == AnimationState.Attack 
                    ? Projectile.ShapeType.Square 
                    : Projectile.ShapeType.Circle;
                swordHitboxScript.SetAttackType(attackType, facingRight);
            }
        }

        // Update position based on facing direction
        if (swordHitbox.activeSelf)
        {
            swordHitbox.transform.localPosition = facingRight ? hitboxOffsetRight : hitboxOffsetLeft;
        }
    }

    void UpdateSprite()
    {
        if (currentAnimation != null && currentFrame < currentAnimation.Length)
        {
            spriteRenderer.sprite = currentAnimation[currentFrame];
        }
    }

    public void PlayAnimation(AnimationState state, bool shouldLoop = false, System.Action callback = null)
    {
        if (currentState == AnimationState.Death && state != AnimationState.Death)
            return;

        if (currentState == state && isPlaying)
            return;

        previousState = currentState;
        currentState = state;
        loop = shouldLoop;
        onAnimationComplete = callback;

        switch (state)
        {
            case AnimationState.Idle:
                currentAnimation = idleSprites;
                break;
            case AnimationState.Attack:
                currentAnimation = attackSprites;
                break;
            case AnimationState.Attack2:
                currentAnimation = attack2Sprites;
                break;
            case AnimationState.Hit:
                currentAnimation = hitSprites;
                break;
            case AnimationState.Death:
                currentAnimation = deathSprites;
                break;
        }

        if (currentAnimation != null && currentAnimation.Length > 0)
        {
            currentFrame = 0;
            isPlaying = true;
            UpdateSprite();
        }
        else
        {
            Debug.LogWarning($"No sprites assigned for animation: {state}");
        }
    }

    public bool IsAnimationPlaying()
    {
        return isPlaying && !loop;
    }

    public AnimationState GetCurrentState()
    {
        return currentState;
    }

    public void SetFacingDirection(bool faceRight)
    {
        if (facingRight != faceRight)
        {
            facingRight = faceRight;
            spriteRenderer.flipX = !facingRight;
            
            // Update hitbox position if active
            if (swordHitbox != null && swordHitbox.activeSelf)
            {
                swordHitbox.transform.localPosition = facingRight ? hitboxOffsetRight : hitboxOffsetLeft;
            }
        }
    }

    public bool IsFacingRight()
    {
        return facingRight;
    }
}

