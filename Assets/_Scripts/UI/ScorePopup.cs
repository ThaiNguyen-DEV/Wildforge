using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ScorePopup : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 40f;
    [SerializeField]
    private float fadeOutDuration = 1f;
    [SerializeField]
    private Vector3 moveDirection = Vector3.down;

    private TextMeshProUGUI popupText;
    private Color initialColor;

    private void Awake()
    {
        popupText = GetComponent<TextMeshProUGUI>();
        initialColor = popupText.color;
    }

    public void Init(int scoreAmount)
    {
        popupText.text = $"+{scoreAmount}";
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        float timer = 0f;

        while (timer < fadeOutDuration)
        {
            // Move the text
            transform.position += moveDirection * moveSpeed * Time.deltaTime;

            // Fade out the text
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeOutDuration);
            popupText.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);

            timer += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}