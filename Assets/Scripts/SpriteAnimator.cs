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

    private AnimationState currentState = AnimationState.Idle;
    private AnimationState previousState = AnimationState.Idle;
    private bool facingRight = true;

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
        PlayAnimation(AnimationState.Idle, true);
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
        }
    }

    public bool IsFacingRight()
    {
        return facingRight;
    }
}

