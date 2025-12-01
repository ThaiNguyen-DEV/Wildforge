using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("Pause UI")]
    [SerializeField]
    private GameObject pausePanel;

    [Header("Pause Buttons")]
    [SerializeField]
    private Button resumeButton;
    [SerializeField]
    private Button quitButton;

    // Public property to check if the game is paused
    public bool IsPaused { get; private set; } = false;

    // Debounce mechanism to prevent rapid toggling
    private float lastToggleTime;
    private const float TOGGLE_COOLDOWN = 0.2f;

    private void Awake()
    {
        // Ensure this manager persists across scene loads if needed, but for now, standard setup.
        // DontDestroyOnLoad(gameObject); // Optional: Uncomment if you need the pause manager to persist.

        if (pausePanel == null)
        {
            Debug.LogError("PausePanel is not assigned in the Inspector!", this);
        }
        else
        {
            pausePanel.SetActive(false); // Ensure UI is hidden on start
        }

        // Assign button listeners
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
        }
        else
        {
            Debug.LogWarning("Resume button is not assigned.", this);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
        else
        {
            Debug.LogWarning("Quit button is not assigned.", this);
        }
    }

    private void OnDestroy()
    {
        // Always clean up listeners to prevent memory leaks
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(ResumeGame);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(QuitGame);
        }

        // Ensure time scale is reset if the pause manager is destroyed
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Toggles the paused state of the game.
    /// </summary>
    public void TogglePause()
    {
        // Debounce: Ignore if called too recently
        if (Time.unscaledTime - lastToggleTime < TOGGLE_COOLDOWN)
        {
            return;
        }
        lastToggleTime = Time.unscaledTime;

        // Invert the paused state
        if (IsPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    /// <summary>
    /// Pauses the game, freezes time, and shows the pause menu.
    /// </summary>
    private void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0f; // Freeze game time

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
        
        Debug.Log("Game Paused");
    }

    /// <summary>
    /// Resumes the game, unfreezes time, and hides the pause menu.
    /// </summary>
    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f; // Resume game time

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Debug.Log("Game Resumed");
    }

    /// <summary>
    /// Quits the application or stops playmode in the editor.
    /// </summary>
    private void QuitGame()
    {
        Debug.Log("Quitting game...");
        Time.timeScale = 1f; // Always reset time scale before quitting

        SceneManager.LoadScene("mainMenuScene");
    }
}