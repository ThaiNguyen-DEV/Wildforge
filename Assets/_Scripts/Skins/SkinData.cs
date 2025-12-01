using UnityEngine;

[CreateAssetMenu(fileName = "New Skin", menuName = "Skins/Skin Data")]
public class SkinData : ScriptableObject
{
    [Header("Skin Info")]
    public string skinName;
    public int price;

    [Header("In-Game Assets")]
    public RuntimeAnimatorController animatorController;
    // The Animator on your "Sprite" GameObject will use this controller.
}