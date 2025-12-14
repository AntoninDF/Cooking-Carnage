using UnityEngine;

[CreateAssetMenu(fileName = "SubtitleSettings", menuName = "Scriptable Objects/SubtitleSettings")]
public class SubtitleSettings : ScriptableObject
{
    
}

[System.Serializable]
public struct SubtitleParameter
{
    public string ID;
    public float Duration;
    public GameObject Prefab;
}