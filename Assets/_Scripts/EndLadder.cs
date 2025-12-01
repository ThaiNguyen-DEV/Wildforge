using UnityEngine;

public class EndLadder : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object that entered is the player
        if (other.CompareTag("Player"))
        {
            // Tell the GameManager to advance to the next stage
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AdvanceStage();
            }
        }
    }
}