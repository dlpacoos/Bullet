using UnityEngine;
using Unity.MLAgents; // Required if you want the Key to directly give rewards

public class Key : MonoBehaviour
{
    public float delayTime = 45f;
    public float pickupReward = 5f; // Reward given upon pickup

    public bool isCollectible { get; private set; } = false; // Track if the key can be picked up

    [Header("Spawn Settings")]
    public Vector2 spawnAreaMin = new Vector2(-9f, -6f);
    public Vector2 spawnAreaMax = new Vector2(9f, 6f);

    void Awake() // Use Awake to ensure it's ready before Player's Start/OnEpisodeBegin might access it
    {
        // Initial state setup is handled by ResetKey called from Player.OnEpisodeBegin
        // or potentially the first time by Start if needed independently.
        // Make sure it's initially visible but not collectible.
        gameObject.SetActive(true);
        isCollectible = false;
    }

    // Call this method to start the timer and make the key appear correctly
    public void ResetAndStartTimer()
    {
        CancelInvoke(nameof(EnableCollection)); // Stop any previous timers

        isCollectible = false;      // Ensure it's not collectible yet
        RandomizePosition();        // Set a new random position
        gameObject.SetActive(true); // Make sure it's visible

        // Schedule the key to become collectible after the delay
        Invoke(nameof(EnableCollection), delayTime);
        Debug.Log($"Key timer started. Will be collectible in {delayTime} seconds.");
    }

    // This method will be called by Invoke after delayTime
    void EnableCollection()
    {
        isCollectible = true;
        Debug.Log("Key is now collectible!");
        // Optional: Add a visual cue, like changing color or adding a particle effect
        // GetComponent<SpriteRenderer>().color = Color.yellow; // Example
    }

    // Call this method when the key is successfully picked up
    public void HideKey()
    {
        CancelInvoke(nameof(EnableCollection)); // Stop the timer if it was still running
        isCollectible = false;                  // Make it non-collectible again
        gameObject.SetActive(false);            // Hide the key
        Debug.Log("Key hidden after pickup.");
        // Optional: Reset visual cues if any were added in EnableCollection
        // GetComponent<SpriteRenderer>().color = Color.white; // Example reset
    }

    // Call this to simply make the key disappear without triggering pickup logic
    // Useful for resetting the environment cleanly at the start of an episode
    public void DeactivateKey()
    {
        CancelInvoke(nameof(EnableCollection));
        isCollectible = false;
        gameObject.SetActive(false);
    }


    // Sets a new random position for the key within the defined area
    public void RandomizePosition()
    {
        float randomX = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
        float randomY = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
        transform.localPosition = new Vector2(randomX, randomY);
    }

    // Handle collision only if the key is collectible
    void OnTriggerEnter2D(Collider2D other)
    {
        // Only proceed if the key is collectible AND the collider belongs to the player agent
        if (!isCollectible) return;

        // Check if the colliding object is the Player agent
        // Using GetComponent<Agent> is more robust if other things might collide
        Player playerAgent = other.GetComponent<Player>();
        // Alternatively, check by tag if the Player GameObject has a specific tag:
        // if (other.CompareTag("Player")) // Make sure your player GameObject has the "Player" tag

        if (playerAgent != null) // Or use the tag check result here
        {
            Debug.Log($"Key picked up by {playerAgent.gameObject.name}! Giving reward: {pickupReward}");
            playerAgent.AddReward(pickupReward); // Give reward to the specific agent

            // You might want the episode to end when the key is picked up,
            // which should be handled by the Player script.
            // Let the player script call EndEpisode if needed.
            // playerAgent.EndEpisode(); // Example if pickup ends the episode

            HideKey(); // Hide the key after it's picked up
        }
    }
}