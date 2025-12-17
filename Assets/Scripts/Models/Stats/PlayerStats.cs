using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Models.Stats;

[CreateAssetMenu(fileName = "New Player Stats", menuName = "Stats/Player Stats")]
public class PlayerStats : ScriptableObject
{
   [SerializedDictionary("Stat Type", "Value")]
   public SerializedDictionary<StatType, float> Stats = new();
}


    

