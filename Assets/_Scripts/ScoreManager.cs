using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int Score { get; private set; }
    private int scoreAtRunStart;
    private const string ScoreKey = "PlayerTotalScore";

    public event Action<int> OnScoreChanged;
    public event Action<int> OnScoreAdded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadScore(); // Load total score when the game starts
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Marks the current score as the starting point for a new run.
    /// </summary>
    public void MarkScoreAtRunStart()
    {
        scoreAtRunStart = Score;
    }

    /// <summary>
    /// Calculates and applies a penalty based on the score earned during the current run.
    /// </summary>
    /// <param name="percentageToLose">The percentage (0.0 to 1.0) of the run's score to lose.</param>
    public void ApplyDeathPenalty(float percentageToLose)
    {
        int scoreEarnedThisRun = Score - scoreAtRunStart;
        if (scoreEarnedThisRun <= 0) return; // No penalty if no score was earned

        int scoreToLose = Mathf.RoundToInt(scoreEarnedThisRun * percentageToLose);
        Score -= scoreToLose;

        // Ensure score doesn't drop below what they started the run with
        if (Score < scoreAtRunStart)
        {
            Score = scoreAtRunStart;
        }

        Debug.Log($"Earned {scoreEarnedThisRun}, lost {scoreToLose}. New total score: {Score}");

        OnScoreChanged?.Invoke(Score);
        SaveScore();
    }

    public void AddScore(int amount)
    {
        if (amount <= 0) return;

        Score += amount;
        OnScoreChanged?.Invoke(Score);
        OnScoreAdded?.Invoke(amount);
        SaveScore();
    }

    public bool TrySpendScore(int amount)
    {
        if (Score >= amount)
        {
            Score -= amount;
            OnScoreChanged?.Invoke(Score);
            SaveScore();
            return true;
        }
        return false;
    }

    private void LoadScore()
    {
        Score = PlayerPrefs.GetInt(ScoreKey, 0);
        OnScoreChanged?.Invoke(Score);
    }

    private void SaveScore()
    {
        PlayerPrefs.SetInt(ScoreKey, Score);
        PlayerPrefs.Save();
    }
}