using UnityEngine;
using UnityEngine.UI; // For heart UI

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("UI References")]
    public Image[] hearts;
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;

    [Header("References")]
    public ScoreManager scoreManager;   

    private Player agent;

    void Awake()
    {
        // Get the Player agent component
        agent = GetComponent<Player>();
        if (agent == null)
            Debug.LogError("Player component (Agent) not found on the same GameObject as PlayerHealth!", this);

        // Auto-find ScoreManager if not set
        if (scoreManager == null)
        {
            scoreManager = FindObjectOfType<ScoreManager>();
            if (scoreManager == null)
                Debug.LogWarning("ScoreManager not found in scene. Clock will not stop on death.", this);
        }
    }

    void Start()
    {
        ResetHealth();
    }

    public void TakeDamage(int amount, bool isBigBulletSource)
    {
        if (currentHealth <= 0) return;  // Already dead

        int previousHealth = currentHealth;
        currentHealth = Mathf.Max(currentHealth - amount, 0);
        UpdateHeartUI();

        if (currentHealth < previousHealth && agent != null)
            agent.NotifyDamageTaken(isBigBulletSource);

        if (currentHealth <= 0)
            Die();
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        UpdateHeartUI();
    }

    private void UpdateHeartUI()
    {
        if (hearts == null || hearts.Length == 0) return;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
                hearts[i].sprite = (i < currentHealth) ? fullHeartSprite : emptyHeartSprite;
            else
                Debug.LogWarning($"Heart UI element at index {i} is not assigned.", this);
        }
    }

    private void Die()
    {
        Debug.Log("Player Died.");

        // Stop the game timer when the player dies
        if (scoreManager != null)
            scoreManager.StopScoring();

      
    }


}
