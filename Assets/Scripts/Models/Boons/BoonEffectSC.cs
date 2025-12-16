using System.Collections.Generic;
using Models;
using Models.Stats;
using UnityEngine;

[CreateAssetMenu(fileName = "BoonEffect", menuName = "ScriptableObjects/BoonEffect", order = 1)]
public class BoonEffectSC : ScriptableObject
{
    public List<StatModifier> StatModifiers;
}
