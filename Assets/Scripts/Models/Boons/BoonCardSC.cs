using System.Collections.Generic;
using Models.Boons;
using UnityEngine;

[CreateAssetMenu(fileName = "BoonCard", menuName = "Boons/BoonCard", order = 1)]
public class BoonCardSC : ScriptableObject
{
    public string CardName;
    public string Description;
    public int MaxStacks = 1;
    public BoonRarity Rarity;
    public Sprite Icon;
    public List<StatModifier> effects;
    
    [HideInInspector]
    public int BoonId;
}
