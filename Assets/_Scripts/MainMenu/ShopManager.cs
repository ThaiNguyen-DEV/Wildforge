using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("Skin Data")]
    [SerializeField] private SkinData[] skins;

    [Header("UI References")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Button[] buyButtons;
    [SerializeField] private Button[] equipButtons;

    private const string UnlockedSkinKeyPrefix = "SkinUnlocked_";
    private const string EquippedSkinKey = "EquippedSkinIndex";

    private void Start()
    {
        // Ensure default skin is always unlocked
        PlayerPrefs.SetInt(UnlockedSkinKeyPrefix + skins[0].skinName, 1);
        UpdateUI();
    }

    private void OnEnable()
    {
        // Update UI every time the shop panel is opened
        UpdateUI();
    }

    public void BuySkin(int skinIndex)
    {
        if (skinIndex < 0 || skinIndex >= skins.Length) return;

        SkinData skin = skins[skinIndex];
        if (ScoreManager.Instance.TrySpendScore(skin.price))
        {
            // Unlock and save
            PlayerPrefs.SetInt(UnlockedSkinKeyPrefix + skin.skinName, 1);
            PlayerPrefs.Save();
            Debug.Log($"Purchased {skin.skinName}!");
            UpdateUI();
        }
        else
        {
            Debug.Log("Not enough score to purchase!");
        }
    }

    public void EquipSkin(int skinIndex)
    {
        if (skinIndex < 0 || skinIndex >= skins.Length) return;

        // Check if skin is unlocked
        if (PlayerPrefs.GetInt(UnlockedSkinKeyPrefix + skins[skinIndex].skinName, 0) == 1)
        {
            int currentlyEquipped = PlayerPrefs.GetInt(EquippedSkinKey, 0);

            // If trying to equip the already equipped skin, unequip it (revert to default)
            if (currentlyEquipped == skinIndex)
            {
                PlayerPrefs.SetInt(EquippedSkinKey, 0); // Revert to default skin
            }
            else
            {
                PlayerPrefs.SetInt(EquippedSkinKey, skinIndex);
            }
            PlayerPrefs.Save();
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        // Update score text
        if (scoreText != null && ScoreManager.Instance != null)
        {
            scoreText.text = ScoreManager.Instance.Score.ToString();
        }

        int equippedSkinIndex = PlayerPrefs.GetInt(EquippedSkinKey, 0);

        for (int i = 0; i < skins.Length; i++)
        {
            bool isUnlocked = PlayerPrefs.GetInt(UnlockedSkinKeyPrefix + skins[i].skinName, 0) == 1;

            // Update Buy Button state
            if (i < buyButtons.Length && buyButtons[i] != null)
            {
                buyButtons[i].gameObject.SetActive(!isUnlocked);
                buyButtons[i].interactable = ScoreManager.Instance != null && ScoreManager.Instance.Score >= skins[i].price;
            }

            // Update Equip Button state
            if (i < equipButtons.Length && equipButtons[i] != null)
            {
                equipButtons[i].gameObject.SetActive(isUnlocked);
                equipButtons[i].GetComponentInChildren<TMP_Text>().text = (i == equippedSkinIndex) ? "EQUIPPED" : "EQUIP";
            }
        }
    }
}