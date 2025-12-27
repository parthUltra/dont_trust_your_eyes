using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Gameplay Settings")]
    public float hitRange = 5.0f;
    public int score = 0;
    public int misses = 0;
    public int maxMisses = 3;

    [Header("Audio")]
    public AudioClip hitSound;
    public AudioClip missSound;
    private AudioSource audioSource;

    [Header("Legacy UI")]
    public TextMeshProUGUI scoreText;
    public GameObject gameOverPanel;

    [Header("New Individual Heart UI")]
    public Image[] heartImages;      // Size 3: Drag Heart1, Heart2, Heart3 here
    public Sprite fullHeartSprite;   // Your Full Heart PNG
    public Sprite emptyHeartSprite;  // Your Empty Heart PNG

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
            if (spriteAnimator != null)
            {
                spriteAnimator.SetFacingDirection(faceRight);
                spriteAnimator.PlayAnimation(SpriteAnimator.AnimationState.Attack, false);
            }
            ProcessInput(Projectile.ShapeType.Square, clickedLeft);
        }
        else if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            bool faceRight = !clickedLeft;
            lastFacingRight = faceRight;
            if (spriteAnimator != null)
            {
                spriteAnimator.SetFacingDirection(faceRight);
                spriteAnimator.PlayAnimation(SpriteAnimator.AnimationState.Attack2, false);
            }
            ProcessInput(Projectile.ShapeType.Circle, clickedLeft);
        }
    }

    void ProcessInput(Projectile.ShapeType type, bool isLeftSide)
    {
        Projectile[] all = FindObjectsOfType<Projectile>();
        bool hitFound = false;
        bool anyInRange = false;

        foreach (Projectile p in all)
        {
            if (Vector3.Distance(p.transform.position, Vector3.zero) <= hitRange)
            {
                anyInRange = true;
                bool pOnLeft = p.transform.position.x < 0;
                if (p.isReal && p.shapeType == type && pOnLeft == isLeftSide)
                    hitFound = true;
            }
        }

        if (hitFound)
        {
            score += 100;
            audioSource.PlayOneShot(hitSound);
            ClearScreen();
        }
        else if (anyInRange)
        {
            RegisterMiss();
            ClearScreen();
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

    void ClearScreen()
    {
        foreach (Projectile p in FindObjectsOfType<Projectile>()) Destroy(p.gameObject);
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "Score: " + score;

        // --- UPDATED HEART LOGIC ---
        // Loops through your 3 heart images and turns them empty based on miss count
        for (int i = 0; i < heartImages.Length; i++)
        {
            // If the current heart index is less than (Total - Misses), it stays full
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

            if (scoreText != null)
            {
                scoreText.gameObject.SetActive(false);
            }

            // Stop the spawner
            Spawner spawner = FindObjectOfType<Spawner>();
            if (spawner != null)
            {
                spawner.StopSpawning();
            }
            GameOverManager gom = gameOverPanel.GetComponent<GameOverManager>();
            if (gom != null)
            {
                gom.UpdateScoreDisplay();
            }
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
