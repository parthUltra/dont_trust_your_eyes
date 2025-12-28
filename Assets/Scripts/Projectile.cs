using UnityEngine;

public class Projectile : MonoBehaviour
{
    public enum ShapeType { Square, Circle }
    public ShapeType shapeType;
    public bool isReal;
    public float speed; 

    private PlayerController playerController;
    private bool hasBeenHit = false;

    [Header("Static Approach Audio")]
    [SerializeField] private AudioClip fireballApproachSound; // Assign Square SFX
    [SerializeField] private AudioClip electricApproachSound; // Assign Circle SFX
    
    private AudioSource activeSpeaker; // Tracks which speaker is playing for this object

    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();

        // Only the "Real" projectile triggers the directional approach sound
        if (isReal)
        {
            // 1. Locate speakers in the scene
            GameObject leftObj = GameObject.Find("LeftAudioSource");
            GameObject rightObj = GameObject.Find("RightAudioSource");

            // 2. Identify side and set the activeSpeaker
            if (IsOnLeftSide() && leftObj != null)
                activeSpeaker = leftObj.GetComponent<AudioSource>();
            else if (!IsOnLeftSide() && rightObj != null)
                activeSpeaker = rightObj.GetComponent<AudioSource>();

            // 3. Play sound if a speaker was found
            if (activeSpeaker != null)
            {
                AudioClip clipToPlay = (shapeType == ShapeType.Square) ? fireballApproachSound : electricApproachSound;
                if (clipToPlay != null)
                {
                    // Use Play() instead of PlayOneShot() so we can stop it later
                    activeSpeaker.clip = clipToPlay;
                    activeSpeaker.Play();
                }
            }
        }

        // Physics Setup
        Rigidbody2D rb = GetComponent<Rigidbody2D>() ?? gameObject.AddComponent<Rigidbody2D>();
        rb.isKinematic = true;
        
        Collider2D collider = GetComponent<Collider2D>() ?? gameObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, Vector3.zero, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, Vector3.zero) < 0.1f)
        {
            if (isReal && !hasBeenHit && playerController != null) 
                playerController.RegisterMiss();
            
            Destroy(gameObject);
        }
    }

    // [NEW] Logic to stop audio immediately when this projectile is removed
    private void OnDestroy()
    {
        if (activeSpeaker != null && activeSpeaker.isPlaying)
        {
            activeSpeaker.Stop();
        }
    }

    public bool HasBeenHit() => hasBeenHit;
    public void OnSwordHit(bool wasCorrect) => hasBeenHit = true;
    public bool IsOnLeftSide() => transform.position.x < 0;
}