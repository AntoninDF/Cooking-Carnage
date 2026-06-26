using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectParameterPlayable))]
public class ObjectParameterPlayableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var data = (ObjectParameterPlayable)target;
        base.OnInspectorGUI();

        if (GUILayout.Button("Add parameter from component"))
        {
            GenericMenu menu = new GenericMenu();

            foreach(var comp in target.GetComponents<Component>())
            {
                string compName = comp.GetType().Name + "/";
                System.Type type = comp.GetType();
                var fields = type.GetFields();
                var ppties = type.GetProperties();
                MemberInfo[] members = new MemberInfo[fields.Length + ppties.Length];
                for (int i = 0; i < fields.Length; i++) members[i] = fields[i];
                for (int i = 0; i < ppties.Length; i++) members[i + fields.Length] = ppties[i];

                foreach (var member in members)
                {
                    var itemName = compName + member.Name;
                    bool isContained = data.AnimatedParameters.Where(x => x.Name == member.Name).Count() > 0;
                    menu.AddItem(new GUIContent(itemName), isContained, () => data.AnimatedParameters.Add(new(member.Name, comp)));
                }
            }
            

            menu.ShowAsContext();
        }
    }
}
