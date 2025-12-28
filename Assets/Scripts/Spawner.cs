using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    public GameObject squarePrefab;
    public GameObject circlePrefab;
    
    [Header("Scaling Difficulty")]
    public float initialSpeed = 5f;
    public float maxSpeed = 15f;
    public float speedIncrease = 0.3f;
    public float initialDelay = 1.5f;
    public float minDelay = 0.5f;

    public float spawnDistance = 10f;

    private float currentSpeed;
    private float currentDelay;
    private Coroutine spawnCoroutine;
    private bool isStopped = false;

    // --- TRUE SIGHT STATE ---
    private bool isTrueSightActive = false;
    private float trueSightTimer = 0f;

    void Start()
    {
        currentSpeed = initialSpeed;
        currentDelay = initialDelay;
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    // Activates the skipping logic
    public void ActivateTrueSight(float duration)
    {
        isTrueSightActive = true;
        trueSightTimer = duration;
    }

    public bool IsTrueSightActive() => isTrueSightActive;

    void Update()
    {
        // Countdown for True Sight effect duration
        if (isTrueSightActive)
        {
            trueSightTimer -= Time.deltaTime;
            if (trueSightTimer <= 0) isTrueSightActive = false;
        }
    }

    IEnumerator SpawnRoutine()
    {
        while (!isStopped)
        {
            while (FindObjectsOfType<Projectile>().Length > 0 && !isStopped)
            {
                yield return null;
            }

            if (isStopped) yield break;
            yield return new WaitForSeconds(currentDelay);
            if (isStopped) yield break;

            SpawnWave();

            currentSpeed = Mathf.Min(currentSpeed + speedIncrease, maxSpeed);
            currentDelay = Mathf.Max(currentDelay - 0.05f, minDelay);
        }
    }

    public void StopSpawning()
    {
        isStopped = true;
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
    }

    void SpawnWave()
    {
        bool isSquare = Random.value > 0.5f;
        GameObject prefab = isSquare ? squarePrefab : circlePrefab;

        if (isTrueSightActive)
        {
            // TRUE SIGHT: Only spawn ONE real projectile at a random side
            float side = Random.value > 0.5f ? -spawnDistance : spawnDistance;
            GameObject obj = Instantiate(prefab, new Vector3(side, 0, 0), Quaternion.identity);
            
            if (side > 0)
            {
                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null) sr.flipX = true;
            }

            SetupProjectile(obj, true); // Force to Real
        }
        else
        {
            // STANDARD: Spawn two projectiles (one real, one fake)
            GameObject leftObj = Instantiate(prefab, new Vector3(-spawnDistance, 0, 0), Quaternion.identity);
            GameObject rightObj = Instantiate(prefab, new Vector3(spawnDistance, 0, 0), Quaternion.identity);

            SpriteRenderer rightRenderer = rightObj.GetComponent<SpriteRenderer>();
            if (rightRenderer != null) rightRenderer.flipX = true;

            bool leftIsReal = Random.value > 0.5f;
            SetupProjectile(leftObj, leftIsReal);
            SetupProjectile(rightObj, !leftIsReal);
        }
    }

    void SetupProjectile(GameObject obj, bool real)
    {
        Projectile p = obj.GetComponent<Projectile>();
        if (p != null)
        {
            p.isReal = real;
            p.speed = currentSpeed;
        }
    }
}