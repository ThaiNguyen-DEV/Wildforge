using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int Score { get; private set; }
    private const string ScoreKey = "PlayerTotalScore";

    public event Action<int> OnScoreChanged;
    public event Action<int> OnScoreAdded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadScore(); // Load score when the game starts
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddScore(int amount)
    {
        if (amount <= 0) return;

        Score += amount;
        OnScoreChanged?.Invoke(Score);
        OnScoreAdded?.Invoke(amount);
        SaveScore(); // Save score whenever it changes
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