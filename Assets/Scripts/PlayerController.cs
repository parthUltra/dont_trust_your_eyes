using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float hitRange = 5.0f;
    public int score = 0;
    public int misses = 0;
    public int maxMisses = 3;

    public AudioClip hitSound;
    public AudioClip missSound;
    private AudioSource audioSource;

    public Text scoreText;
    public Text missText;
    public GameObject gameOverPanel;

    void Start()
    {
        Time.timeScale = 1;
        audioSource = gameObject.AddComponent<AudioSource>();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        UpdateUI();
    }

    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        bool clickedLeft = mousePos.x < 0;

        if (Mouse.current.leftButton.wasPressedThisFrame)
            ProcessInput(Projectile.ShapeType.Square, clickedLeft);
        else if (Mouse.current.rightButton.wasPressedThisFrame)
            ProcessInput(Projectile.ShapeType.Circle, clickedLeft);
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
        if (misses >= maxMisses) GameOver();
    }

    void ClearScreen()
    {
        foreach (Projectile p in FindObjectsOfType<Projectile>()) Destroy(p.gameObject);
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "Score: " + score;
        if (missText != null) missText.text = "Misses: " + misses + "/" + maxMisses;
    }

    void GameOver()
    {
        Time.timeScale = 0;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}