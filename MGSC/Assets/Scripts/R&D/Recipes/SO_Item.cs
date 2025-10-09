using UnityEngine;

[CreateAssetMenu(fileName = "SO_Item", menuName = "Scriptable Objects/SO_Item")]
public class SO_Item : ScriptableObject
{
    public string Name { get => name;set { name = value; } }
    [SerializeField] new private string name;
}
