using UnityEngine;

public class PlayerSkinApplier : MonoBehaviour
{
    [Header("Skin Data")]
    [SerializeField] private SkinData[] allSkins; // Assign all your SkinData assets here

    private const string EquippedSkinKey = "EquippedSkinIndex";

    private void Awake()
    {
        ApplyEquippedSkin();
    }

    private void ApplyEquippedSkin()
    {
        int equippedSkinIndex = PlayerPrefs.GetInt(EquippedSkinKey, 0);

        if (equippedSkinIndex < 0 || equippedSkinIndex >= allSkins.Length)
        {
            Debug.LogWarning($"Invalid skin index ({equippedSkinIndex}), defaulting to 0.");
            equippedSkinIndex = 0;
        }

        SkinData skinToApply = allSkins[equippedSkinIndex];
        
        // This will find the Animator on your "Sprite" child object
        Animator animator = GetComponentInChildren<Animator>(); 

        if (animator != null && skinToApply.animatorController != null)
        {
            animator.runtimeAnimatorController = skinToApply.animatorController;
        }
        else
        {
            Debug.LogError("Could not apply skin. Animator or Animator Controller is missing.", this);
        }
    }
}