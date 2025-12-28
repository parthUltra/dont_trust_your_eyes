using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages spawning of powerups at random intervals
/// </summary>
public class PowerupSpawner : MonoBehaviour
{
    [System.Serializable]
    public class PowerupSpawnData
    {
        public GameObject powerupPrefab;
        public float spawnWeight = 1f; // Higher weight = more likely to spawn
        [Tooltip("Min/Max time in seconds between spawn attempts")]
        public Vector2 spawnInterval = new Vector2(5f, 15f);
    }
    
    [Header("Powerup Prefabs")]
    [SerializeField] private List<PowerupSpawnData> powerupTypes = new List<PowerupSpawnData>();
    
    [Header("Spawn Area")]
    [SerializeField] private Vector2 spawnAreaMin = new Vector2(-8f, -4f);
    [SerializeField] private Vector2 spawnAreaMax = new Vector2(8f, 4f);
    
    [Header("Settings")]
    [SerializeField] private int maxActivePowerups = 2; // Limit how many powerups can exist at once
    
    private PlayerController playerController;
    private bool isSpawning = false;
    private Coroutine spawnCoroutine;
    private List<GameObject> activePowerups = new List<GameObject>();
    
    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerController not found! PowerupSpawner disabled.");
            enabled = false;
            return;
        }
        
        StartSpawning();
    }
    
    void Update()
    {
        // Clean up null references from the active powerups list
        activePowerups.RemoveAll(item => item == null);
    }
    
    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            spawnCoroutine = StartCoroutine(SpawnRoutine());
        }
    }
    
    public void StopSpawning()
    {
        isSpawning = false;
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }
    
    private IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            // Wait for a random interval before attempting to spawn
            float waitTime = Random.Range(5f, 15f);
            yield return new WaitForSeconds(waitTime);
            
            // Only spawn if we haven't reached the max active powerups
            if (activePowerups.Count < maxActivePowerups)
            {
                TrySpawnPowerup();
            }
        }
    }
    
    private void TrySpawnPowerup()
    {
        if (powerupTypes == null || powerupTypes.Count == 0)
        {
            Debug.LogWarning("No powerup types configured in PowerupSpawner!");
            return;
        }
        
        // Filter powerups that can spawn based on their conditions
        List<PowerupSpawnData> eligiblePowerups = new List<PowerupSpawnData>();
        List<float> weights = new List<float>();
        
        foreach (var powerupData in powerupTypes)
        {
            if (powerupData.powerupPrefab == null) continue;
            
            PowerupBase powerupScript = powerupData.powerupPrefab.GetComponent<PowerupBase>();
            if (powerupScript != null && powerupScript.CanSpawn(playerController))
            {
                eligiblePowerups.Add(powerupData);
                weights.Add(powerupData.spawnWeight);
            }
        }
        
        // If no eligible powerups, don't spawn anything
        if (eligiblePowerups.Count == 0)
        {
            return;
        }
        
        // Select a powerup based on weighted random selection
        PowerupSpawnData selectedPowerup = SelectWeightedRandom(eligiblePowerups, weights);
        
        if (selectedPowerup != null)
        {
            SpawnPowerup(selectedPowerup.powerupPrefab);
        }
    }
    
    private PowerupSpawnData SelectWeightedRandom(List<PowerupSpawnData> powerups, List<float> weights)
    {
        if (powerups.Count == 0) return null;
        
        float totalWeight = 0f;
        foreach (float weight in weights)
        {
            totalWeight += weight;
        }
        
        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;
        
        for (int i = 0; i < powerups.Count; i++)
        {
            cumulativeWeight += weights[i];
            if (randomValue <= cumulativeWeight)
            {
                return powerups[i];
            }
        }
        
        return powerups[powerups.Count - 1];
    }
    
    private void SpawnPowerup(GameObject powerupPrefab)
    {
        // Generate random position within spawn area
        Vector3 spawnPosition = new Vector3(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            Random.Range(spawnAreaMin.y, spawnAreaMax.y),
            0f
        );
        
        GameObject powerup = Instantiate(powerupPrefab, spawnPosition, Quaternion.identity);
        activePowerups.Add(powerup);
        
        Debug.Log($"Spawned {powerupPrefab.name} at {spawnPosition}");
    }
    
    // Visualize spawn area in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3(
            (spawnAreaMin.x + spawnAreaMax.x) / 2f,
            (spawnAreaMin.y + spawnAreaMax.y) / 2f,
            0f
        );
        Vector3 size = new Vector3(
            spawnAreaMax.x - spawnAreaMin.x,
            spawnAreaMax.y - spawnAreaMin.y,
            0f
        );
        Gizmos.DrawWireCube(center, size);
    }
}


