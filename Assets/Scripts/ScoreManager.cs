using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text scoreText;     // assign in Inspector
    public TMP_Text highScoreText; // assign in Inspector

    private float elapsedTime = 0f;
    private bool  running     = true;
    private int   highScore   = 0;

    void Start()
    {
        // Load and display saved high score
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        UpdateHighScoreUI();

        // Start current run
        ResetScore();
    }

    void Update()
    {
        if (!running) return;
        elapsedTime += Time.deltaTime;
        scoreText.text = Mathf.FloorToInt(elapsedTime).ToString();
    }

    public void StopScoring()
    {
        running = false;
        int finalScore = Mathf.FloorToInt(elapsedTime);
        if (finalScore > highScore)
        {
            highScore = finalScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
            UpdateHighScoreUI();
        }
    }

    public void ResetScore()
    {
        elapsedTime = 0f;
        running     = true;
        scoreText.text = "0";
    }

    /// <summary>
    /// Clears the stored high score and updates the UI.
    /// </summary>
    public void ResetHighScore()
    {
        PlayerPrefs.DeleteKey("HighScore");
        PlayerPrefs.Save();
        highScore = 0;
        UpdateHighScoreUI();
    }

    private void UpdateHighScoreUI()
    {
        if (highScoreText != null)
            highScoreText.text = highScore.ToString();
    }
}
