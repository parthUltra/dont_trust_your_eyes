using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Gameplay Settings")]
    public int score = 0;
    public int misses = 0;
    public int maxMisses = 3;

    [Header("Audio")]
    public AudioClip hitSound;
    public AudioClip missSound;
    public AudioClip attack1Sound; // NEW: Sound for Left Click / Attack1
    public AudioClip attack2Sound; // NEW: Sound for Right Click / Attack2
    private AudioSource audioSource;

    [Header("Legacy UI")]
    public TextMeshProUGUI scoreText;
    public GameObject gameOverPanel;

    [Header("New Individual Heart UI")]
    public Image[] heartImages;      
    public Sprite fullHeartSprite;   
    public Sprite emptyHeartSprite;  

    private SpriteAnimator spriteAnimator;
    private bool isDead = false;
    private bool lastFacingRight = true;

    void Start()
    {
        Time.timeScale = 1;
        audioSource = gameObject.AddComponent<AudioSource>();
        spriteAnimator = GetComponent<SpriteAnimator>();
        
        if (spriteAnimator == null)
        {
            Debug.LogError("SpriteAnimator component not found on PlayerController!");
        }
        else
        {
            spriteAnimator.SetFacingDirection(true);
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(true);
        }
        UpdateUI();
    }

    void Update()
    {
        if (isDead) { return; }

        if (spriteAnimator != null && spriteAnimator.IsAnimationPlaying())
            return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        bool clickedLeft = mousePos.x < 0;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            bool faceRight = !clickedLeft;
            lastFacingRight = faceRight;
            
            // --- PLAY SWING SOUND 1 ---
            if (attack1Sound != null) audioSource.PlayOneShot(attack1Sound);

            if (spriteAnimator != null)
            {
                spriteAnimator.SetFacingDirection(faceRight);
                spriteAnimator.PlayAnimation(SpriteAnimator.AnimationState.Attack, false);
            }
        }
        else if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            bool faceRight = !clickedLeft;
            lastFacingRight = faceRight;

            // --- PLAY SWING SOUND 2 ---
            if (attack2Sound != null) audioSource.PlayOneShot(attack2Sound);

            if (spriteAnimator != null)
            {
                spriteAnimator.SetFacingDirection(faceRight);
                spriteAnimator.PlayAnimation(SpriteAnimator.AnimationState.Attack2, false);
            }
        }
    }

    public void ProcessProjectileClick(bool wasCorrect)
    {
        if (wasCorrect)
        {
            score += 100;
            audioSource.PlayOneShot(hitSound);
        }
        else
        {
            RegisterMiss();
        }
        UpdateUI();
    }

    public void RegisterMiss()
    {
        misses++;
        audioSource.PlayOneShot(missSound);

        if (spriteAnimator != null)
        {
            spriteAnimator.PlayAnimation(SpriteAnimator.AnimationState.Hit, false);
        }

        UpdateUI();

        if (misses >= maxMisses)
        {
            GameOver();
        }
    }

    public bool IsDead() => isDead;

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "Score: " + score;

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < (maxMisses - misses))
            {
                heartImages[i].sprite = fullHeartSprite;
            }
            else
            {
                heartImages[i].sprite = emptyHeartSprite;
            }
        }
    }

    public void Heal()
    {
        if (misses > 0)
        {
            misses--;
            UpdateUI();
        }
    }

    void ClearAllPowerups()
    {
        foreach (PowerupBase powerup in FindObjectsOfType<PowerupBase>()) Destroy(powerup.gameObject);
    }

    void ClearAllProjectiles()
    {
        foreach (Projectile projectile in FindObjectsOfType<Projectile>()) Destroy(projectile.gameObject);
    }

    void GameOver()
    {
        isDead = true;
        if (spriteAnimator != null)
        {
            spriteAnimator.PlayAnimation(SpriteAnimator.AnimationState.Death, false, () =>
            {
                Time.timeScale = 0;
                TriggerGameOverUI();
            });
        }
        else
        {
            Time.timeScale = 0;
            TriggerGameOverUI();
        }
    }

    void TriggerGameOverUI()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (scoreText != null) scoreText.gameObject.SetActive(false);

            Spawner spawner = FindObjectOfType<Spawner>();
            if (spawner != null) spawner.StopSpawning();

            PowerupSpawner pSpawner = FindObjectOfType<PowerupSpawner>();
            if (pSpawner != null) pSpawner.StopSpawning();

            ClearAllPowerups();
            ClearAllProjectiles();

            GameOverManager gom = gameOverPanel.GetComponent<GameOverManager>();
            if (gom != null) gom.UpdateScoreDisplay();
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}