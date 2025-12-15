using System;
using System.Collections.Generic;
using Models.Stats;
using UnityEngine;
using UnityEditor;


[Serializable]
public struct FloatStatEntry
{
    public StatType key;
    public float value;
}

[Serializable]
public struct IntStatEntry
{
    public StatType key;
    public int value;
}

[Serializable]
public struct BoolStatEntry
{
    public StatType key;
    public bool value;
}

[CreateAssetMenu(fileName = "New Player Stats", menuName = "Stats/Player Stats")]
public class PlayerStats : ScriptableObject
{
    public List<FloatStatEntry> FloatStatList = new List<FloatStatEntry>();
    public List<IntStatEntry> IntStatList = new List<IntStatEntry>();
    public List<BoolStatEntry> BoolStatList = new List<BoolStatEntry>();

    // Runtime dictionaries for fast lookup
    [HideInInspector] public Dictionary<StatType, float> FloatStats = new Dictionary<StatType, float>();
    [HideInInspector] public Dictionary<StatType, int> IntStats = new Dictionary<StatType, int>();
    [HideInInspector] public Dictionary<StatType, bool> BoolStats = new Dictionary<StatType, bool>();

    private void OnEnable()
    {
        GenerateDictionaries();
    }

    public void GenerateDictionaries()
    {
        FloatStats.Clear();
        IntStats.Clear();
        BoolStats.Clear();

        foreach (var f in FloatStatList) FloatStats[f.key] = f.value;
        foreach (var i in IntStatList) IntStats[i.key] = i.value;
        foreach (var b in BoolStatList) BoolStats[b.key] = b.value;
    }

    public object GetStat(StatType statType)
    {
        if (FloatStats.ContainsKey(statType)) return FloatStats[statType];
        if (IntStats.ContainsKey(statType)) return IntStats[statType];
        if (BoolStats.ContainsKey(statType)) return BoolStats[statType];
        Debug.LogWarning($"Stat {statType} not found.");
        return null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        GenerateDictionaries();
    }
#endif
}


[CustomEditor(typeof(PlayerStats))]
public class PlayerStatsEditor : Editor
{
    private SerializedProperty floatStatList;
    private SerializedProperty intStatList;
    private SerializedProperty boolStatList;

    private void OnEnable()
    {
        floatStatList = serializedObject.FindProperty("FloatStatList");
        intStatList = serializedObject.FindProperty("IntStatList");
        boolStatList = serializedObject.FindProperty("BoolStatList");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawStatList(floatStatList, "Float Stats");
        DrawStatList(intStatList, "Int Stats");
        DrawStatList(boolStatList, "Bool Stats");

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawStatList(SerializedProperty listProp, string label)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        if (GUILayout.Button("Add New"))
        {
            listProp.arraySize++;
        }

        for (int i = 0; i < listProp.arraySize; i++)
        {
            SerializedProperty element = listProp.GetArrayElementAtIndex(i);
            SerializedProperty key = element.FindPropertyRelative("key");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(key, GUIContent.none);

            if (element.FindPropertyRelative("value") != null)
            {
                SerializedProperty valueProp = element.FindPropertyRelative("value");
                EditorGUILayout.PropertyField(valueProp, GUIContent.none);
            }

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                listProp.DeleteArrayElementAtIndex(i);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
    }
}    
    
    

