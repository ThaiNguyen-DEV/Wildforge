using UnityEngine;

public class DeadCanvasFinder : MonoBehaviour
{
    private void Awake()
    {
        // Find the GameManager and register this canvas with it.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterDeadCanvas(gameObject);
        }
        else
        {
            Debug.LogError("GameManager not found! Cannot register DeadCanvas.");
        }
    }
}