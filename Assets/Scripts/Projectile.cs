using UnityEngine;

public class Projectile : MonoBehaviour
{
    public enum ShapeType { Square, Circle }
    public ShapeType shapeType;
    public bool isReal;
    public float speed; // Speed is now set by the Spawner

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            // Only the real projectile makes directional sound
            audioSource.mute = !isReal;
        }
    }

    void Update()
    {
        // Moves toward the center
        transform.position = Vector3.MoveTowards(transform.position, Vector3.zero, speed * Time.deltaTime);

        // Auto-destruct and register miss if real projectile hits center
        if (Vector3.Distance(transform.position, Vector3.zero) < 0.1f)
        {
            if (isReal)
            {
                PlayerController pc = FindObjectOfType<PlayerController>();
                if (pc != null) pc.RegisterMiss();
            }
            Destroy(gameObject);
        }
    }
}