using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Component References")]
    [SerializeField]
    private RoomFirstDungeonGenerator dungeonGenerator;
    [SerializeField]
    private GameObject deadCanvas; // Assign your DeadCanvas in the Inspector

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
        // This method runs every time a scene is loaded.
        // We check if the new scene is our game scene.
        if (scene.name == "gameScene") // Use the exact name of your game scene
        {
            // Find the new instances of components in the loaded scene
            dungeonGenerator = FindObjectOfType<RoomFirstDungeonGenerator>();
            deadCanvas = GameObject.FindWithTag("DeadCanvas"); // Switched to FindWithTag for safety

            // Reset player health reference for the new player that will be spawned
            playerHealth = null;

            // Start the stage
            StartNewStage();
        }
    }

    public void RegisterPlayer(Health health)
    {
        playerHealth = health;
        playerHealth.OnDeath += HandlePlayerDeath;
    }

    private void HandlePlayerDeath()
    {
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
        // Hide the death canvas if it's active
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