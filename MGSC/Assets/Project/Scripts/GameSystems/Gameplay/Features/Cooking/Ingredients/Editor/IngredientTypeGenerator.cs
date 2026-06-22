using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SourceGen;

[CreateAssetMenu(fileName = "IngredientTypeGenerator", menuName = "Scriptable Objects/IngredientTypeGenerator")]
public class IngredientTypeGenerator : ScriptableObject
{
    public string Path = "Assets/";
    public List<string> Types;

    public void Regenerate()
    {
        EnumGenerator enumGen = new("IngredientType");
        foreach (var type in Types)
        {
            enumGen.Add(type);
            MakeIngredientClass(type);
        }
        enumGen.ToFile(Path, "IngredientType");
    }


    public void MakeIngredientClass(string className)
    {
        ClassGenerator myClass = new(className);
        myClass.InheritFrom(typeof(Ingredient));
        myClass.Add(new RawTextGenerator("public " + className + "(IFeatureCollection owner) : base(owner)\n{\n}"));

        ScriptGenerator myScript = new(className);
        myScript.Add(myClass.WithNamespace("Features"));
        myScript.Using("Cooking");

        myScript.ToFile(Path, className);
    }
}

[CustomEditor(typeof(IngredientTypeGenerator))]
public class IngredientTypeGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        IngredientTypeGenerator gen = (IngredientTypeGenerator)target;

        if (GUILayout.Button("Regenerate"))
        {
            gen.Regenerate();
        }
    }
}