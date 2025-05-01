using UnityEngine;
using Unity.MLAgents; // Required for the Agent type

// Remove SceneManagement if not needed elsewhere
// using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    // --- ML-Agents Change ---
    // Add a reference to the Agent you want to control.
    // You'll need to drag the Agent GameObject onto this slot
    // in the GameManager's Inspector window in the Unity Editor.
    public Agent agentToControl;
    // ------------------------

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Consider if DontDestroyOnLoad is still needed if you aren't reloading scenes.
            // If the GameManager only manages state within a single scene load,
            // you might not need DontDestroyOnLoad. Keep it if it serves another purpose.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Call this to end the agent's current episode
    public void GameOver()
    {
        // --- ML-Agents Change ---
        // Check if the agent reference is set
        if (agentToControl != null)
        {
            // Call EndEpisode() on the referenced agent
            Debug.Log($"GameManager triggering EndEpisode for agent: {agentToControl.gameObject.name}");
            agentToControl.EndEpisode();
        }
        else
        {
            Debug.LogError("GameOver called, but 'agentToControl' is not assigned in the GameManager Inspector!");
        }
        // ------------------------

        // --- Removed Scene Loading ---
        // // Option A: reload this scene immediately
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        //
        // // —OR— Option B: load a separate “GameOver” scene
        // // SceneManager.LoadScene("GameOverScene");
        // -----------------------------
    }

    // Optional: Helper method if you need to set the agent dynamically
    public void SetAgentToControl(Agent agent)
    {
        agentToControl = agent;
    }
}