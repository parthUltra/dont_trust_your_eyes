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

    void Start()
    {
        currentSpeed = initialSpeed;
        currentDelay = initialDelay;
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // Wait until no Projectiles are left on screen
            while (FindObjectsOfType<Projectile>().Length > 0)
            {
                yield return null;
            }

            yield return new WaitForSeconds(currentDelay);

            SpawnWave();

            // Increase difficulty
            currentSpeed = Mathf.Min(currentSpeed + speedIncrease, maxSpeed);
            currentDelay = Mathf.Max(currentDelay - 0.05f, minDelay);
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