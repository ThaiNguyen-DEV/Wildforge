using UnityEngine;
using UnityEngine.SceneManagement;

public class mainMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField]
    private GameObject panel_Tutorial; // Assign your tutorial panel in the Inspector
    [SerializeField]
    private GameObject panel_Shop; // Assign your shop panel in the Inspector

    private void Start()
    {
        // Ensure panels are hidden when the scene starts
        if (panel_Tutorial != null) panel_Tutorial.SetActive(false);
        if (panel_Shop != null) panel_Shop.SetActive(false);
    }

    /// <summary>
    /// Loads the main game scene.
    /// </summary>
    public void PlayGame()
    {
        // Make sure "GameScene" is the exact name of your game scene file.
        SceneManager.LoadScene("gameScene");
    }

    /// <summary>
    /// Toggles the visibility of the tutorial panel.
    /// Intended for the main "Tutorial" button.
    /// </summary>
    public void ToggleTutorialPanel()
    {
        if (panel_Tutorial != null)
        {
            panel_Tutorial.SetActive(!panel_Tutorial.activeSelf);
        }
    }

    /// <summary>
    /// Explicitly closes the tutorial panel.
    /// Intended for the "Close" button inside the tutorial panel.
    /// </summary>
    public void CloseTutorialPanel()
    {
        if (panel_Tutorial != null)
        {
            panel_Tutorial.SetActive(false);
        }
    }

    /// <summary>
    /// Toggles the visibility of the shop panel.
    /// Intended for the main "Shop" button.
    /// </summary>
    public void ToggleShopPanel()
    {
        if (panel_Shop != null)
        {
            panel_Shop.SetActive(!panel_Shop.activeSelf);
        }
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();

#if UNITY_EDITOR
        // This also stops play mode when running in the Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
