using UnityEngine;
using System.Collections; // Required for IEnumerator

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

    void Start()
    {
        currentSpeed = initialSpeed;
        currentDelay = initialDelay;
        spawnCoroutine = StartCoroutine(SpawnRoutine());
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

            // Increase difficulty
            currentSpeed = Mathf.Min(currentSpeed + speedIncrease, maxSpeed);
            currentDelay = Mathf.Max(currentDelay - 0.05f, minDelay);
        }
    }

    public void StopSpawning()
    {
        isStopped = true;
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
    }

    void SpawnWave()
    {
        bool isSquare = Random.value > 0.5f;
        GameObject prefab = isSquare ? squarePrefab : circlePrefab;

        // Instantiate Left and Right
        GameObject leftObj = Instantiate(prefab, new Vector3(-spawnDistance, 0, 0), Quaternion.identity);
        GameObject rightObj = Instantiate(prefab, new Vector3(spawnDistance, 0, 0), Quaternion.identity);

        // --- NEW LOGIC TO FLIP THE RIGHT FIREBALL ---
        // The right fireball needs to face Left, so we flip it.
        SpriteRenderer rightRenderer = rightObj.GetComponent<SpriteRenderer>();
        if (rightRenderer != null)
        {
            rightRenderer.flipX = true;
        }
        // --------------------------------------------

        bool leftIsReal = Random.value > 0.5f;
        SetupProjectile(leftObj, leftIsReal);
        SetupProjectile(rightObj, !leftIsReal);
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
