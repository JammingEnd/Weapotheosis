using Models.Boons;
using UnityEngine;

[CreateAssetMenu(fileName = "BoonCard", menuName = "ScriptableObjects/BoonCard", order = 1)]
public class BoonCardSC : ScriptableObject
{
    public string CardName;
    public string Description;
    public BoonRarity Rarity;
    public Sprite Icon;
    public BoonEffectSC BoonEffect;
}
