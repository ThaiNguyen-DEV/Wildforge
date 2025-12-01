using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Player UI References")]
    public TMP_Text healthText;
    public Image dashCooldownImage;
    public Image skillCooldownImage;

    [Header("Health Flash Effect")]
    [SerializeField]
    private Color healthFlashColor = Color.red;
    [SerializeField]
    private float healthFlashDuration = 0.15f;
    private Color originalHealthColor;

    [Header("Stage Intro")]
    [SerializeField]
    private TMP_Text stageIntroText; // Assign your new text element here
    [SerializeField]
    private float stageIntroFadeTime = 0.5f;
    [SerializeField]
    private float stageIntroDisplayTime = 1.5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (healthText != null)
        {
            originalHealthColor = healthText.color;
        }
        // Ensure stage text is hidden on start
        if (stageIntroText != null)
        {
            stageIntroText.alpha = 0;
        }
    }

    public void FlashHealthUI()
    {
        if (healthText != null)
        {
            StartCoroutine(HealthFlashCoroutine());
        }
    }

    private IEnumerator HealthFlashCoroutine()
    {
        healthText.color = healthFlashColor;
        yield return new WaitForSeconds(healthFlashDuration);
        healthText.color = originalHealthColor;
    }

    public void ShowStageIntro(int stageNumber)
    {
        if (stageIntroText != null)
        {
            StartCoroutine(StageIntroCoroutine(stageNumber));
        }
    }

    private IEnumerator StageIntroCoroutine(int stageNumber)
    {
        stageIntroText.text = $"STAGE {stageNumber}";

        // Fade In
        float timer = 0;
        while (timer < stageIntroFadeTime)
        {
            timer += Time.deltaTime;
            stageIntroText.alpha = Mathf.Lerp(0, 1, timer / stageIntroFadeTime);
            yield return null;
        }
        stageIntroText.alpha = 1;

        // Hold
        yield return new WaitForSeconds(stageIntroDisplayTime);

        // Fade Out
        timer = 0;
        while (timer < stageIntroFadeTime)
        {
            timer += Time.deltaTime;
            stageIntroText.alpha = Mathf.Lerp(1, 0, timer / stageIntroFadeTime);
            yield return null;
        }
        stageIntroText.alpha = 0;
    }
}