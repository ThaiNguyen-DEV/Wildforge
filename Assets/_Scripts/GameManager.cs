using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Component References")]
    [SerializeField]
    private RoomFirstDungeonGenerator dungeonGenerator;
    private GameObject deadCanvas; // No longer serialized, will be assigned by the finder script

    [Header("Game Settings")]
    [SerializeField, Range(0, 1)]
    private float deathPenaltyPercent = 0.6f;

    private int stage = 1;
    private Health playerHealth;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "gameScene")
        {
            dungeonGenerator = FindObjectOfType<RoomFirstDungeonGenerator>();
            // The DeadCanvas will now be found by its own script.
            playerHealth = null;

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.MarkScoreAtRunStart();
            }

            StartNewStage();
        }
    }

    /// <summary>
    /// Allows the DeadCanvas to register itself with the GameManager.
    /// </summary>
    public void RegisterDeadCanvas(GameObject canvas)
    {
        deadCanvas = canvas;
        // Ensure it's inactive when registered
        deadCanvas.SetActive(false);
    }

    public void RegisterPlayer(Health health)
    {
        playerHealth = health;
        playerHealth.OnDeath += HandlePlayerDeath;
    }

    private void HandlePlayerDeath()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ApplyDeathPenalty(deathPenaltyPercent);
        }

        if (deadCanvas != null)
        {
            deadCanvas.SetActive(true);
        }
        else
        {
            Debug.LogError("DeadCanvas reference is not set in GameManager!");
        }

        if (playerHealth != null)
        {
            playerHealth.OnDeath -= HandlePlayerDeath;
        }
    }

    public void AdvanceStage()
    {
        stage++;
        StartNewStage();
    }

    private void StartNewStage()
    {
        if (deadCanvas != null)
        {
            deadCanvas.SetActive(false);
        }

        if (playerHealth != null)
        {
            playerHealth.ResetHealth();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowStageIntro(stage);
        }

        if (dungeonGenerator != null)
        {
            dungeonGenerator.GenerateDungeon();
        }
        else
        {
            Debug.LogError("Dungeon Generator not found in the scene!");
        }
    }

    public void Retry()
    {
        stage = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        SceneManager.LoadScene("mainmenuScene");
    }
}