using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI scoreText;

    [Header("Score Popup")]
    [SerializeField]
    private GameObject scorePopupPrefab;
    [SerializeField]
    private Transform popupSpawnPoint;

    private void Start()
    {
        if (ScoreManager.Instance != null)
        {
            UpdateScoreText(ScoreManager.Instance.Score);
            ScoreManager.Instance.OnScoreChanged += UpdateScoreText;
            ScoreManager.Instance.OnScoreAdded += HandleScoreAdded;
        }
    }

    private void OnDestroy()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= UpdateScoreText;
            ScoreManager.Instance.OnScoreAdded -= HandleScoreAdded;
        }
    }

    private void HandleScoreAdded(int amount)
    {
        if (scorePopupPrefab == null || popupSpawnPoint == null) return;

        GameObject popupInstance = Instantiate(scorePopupPrefab, popupSpawnPoint.position, Quaternion.identity, popupSpawnPoint);
        popupInstance.GetComponent<ScorePopup>()?.Init(amount);
    }

    private void UpdateScoreText(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString("D6");
        }
    }
}