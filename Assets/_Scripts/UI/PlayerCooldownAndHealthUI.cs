using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Health))]
public class PlayerCooldownAndHealthUI : MonoBehaviour
{
    [Header("Cooldown Settings")]
    [SerializeField]
    private float dashCooldownSeconds = 3f;
    [SerializeField]
    private float skillCooldownSeconds = 10f;

    // UI element references are now private
    private TMP_Text healthText;
    private Image dashCooldownImage;
    private Image skillCooldownImage;

    private float dashTimer;
    private float skillTimer;

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void Start()
    {
        // Get UI references from the UIManager
        if (UIManager.Instance != null)
        {
            healthText = UIManager.Instance.healthText;
            dashCooldownImage = UIManager.Instance.dashCooldownImage;
            skillCooldownImage = UIManager.Instance.skillCooldownImage;
        }
        else
        {
            Debug.LogError("UIManager instance not found!");
            return;
        }

        // Initialize UI states
        InitializeCooldownUI(dashCooldownImage);
        InitializeCooldownUI(skillCooldownImage);
        
        // Request the health component to send its initial values
        health.RequestInitialHealth();
    }

    private void InitializeCooldownUI(Image image)
    {
        if (image != null)
        {
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Radial360;
            image.fillAmount = 0f;
            image.enabled = false;
        }
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnHealthChanged += OnHealthChanged;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnHealthChanged -= OnHealthChanged;
        }
    }

    private void Update()
    {
        UpdateDashCooldown();
        UpdateSkillCooldown();
    }

    public bool TryUseDash()
    {
        if (dashTimer > 0f) return false;

        dashTimer = dashCooldownSeconds;

        if (dashCooldownImage != null)
        {
            dashCooldownImage.enabled = true;
            dashCooldownImage.fillAmount = 1f;
        }
        return true;
    }

    public bool TryUseSkill()
    {
        if (skillTimer > 0f) return false;

        skillTimer = skillCooldownSeconds;

        if (skillCooldownImage != null)
        {
            skillCooldownImage.enabled = true;
            skillCooldownImage.fillAmount = 1f;
        }
        return true;
    }

    private void UpdateDashCooldown()
    {
        if (dashTimer <= 0f)
        {
            if (dashCooldownImage != null && dashCooldownImage.enabled)
            {
                dashCooldownImage.fillAmount = 0f;
                dashCooldownImage.enabled = false;
            }
            return;
        }

        dashTimer -= Time.deltaTime;
        if (dashTimer < 0f) dashTimer = 0f;

        if (dashCooldownImage != null)
        {
            dashCooldownImage.fillAmount = Mathf.Clamp01(dashTimer / dashCooldownSeconds);
        }
    }

    private void UpdateSkillCooldown()
    {
        if (skillTimer <= 0f)
        {
            if (skillCooldownImage != null && skillCooldownImage.enabled)
            {
                skillCooldownImage.fillAmount = 0f;
                skillCooldownImage.enabled = false;
            }
            return;
        }

        skillTimer -= Time.deltaTime;
        if (skillTimer < 0f) skillTimer = 0f;

        if (skillCooldownImage != null)
        {
            skillCooldownImage.fillAmount = Mathf.Clamp01(skillTimer / skillCooldownSeconds);
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        if (healthText != null)
        {
            healthText.text = $"{current}";
        }
    }
}