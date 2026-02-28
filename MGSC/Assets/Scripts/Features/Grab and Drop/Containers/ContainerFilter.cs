using System;
using System.Collections.Generic;

namespace Features
{
    /// <summary>
    /// The ContainerFilter component should be added to a gameobject with a container component. It filters out which Grabbables can be grabbed.
    /// </summary>
    public class ContainerFilter : Feature
    {
        public List<Func<Grabbable, bool>> Conditions = new();

        public ContainerFilter(IFeatureCollection owner) : base(owner)
        {
        }
        public bool IsValid(Grabbable comparedItem)
        {
            bool result = true;

            foreach(var condition in Conditions)
            {
                if (!condition(comparedItem))
                {
                    result = false; break;
                }
            }

            //UnityEngine.Debug.Log("Filter is " + (result?"valid":"not valid"));
            return result;
        }
        
        public void AddCondition(Func<Grabbable, bool> condition) => Conditions.Add(condition);

        
    }
    public enum EFilterCondition
    {
        MustBe,
        MustNotBe
    }

    // When an option is added to this enum, also add the corresponding logic to IsValid(MonoBehaviour, EItemType)
    // Do not change the IDs !! It will break every existing filter
    public enum EItemType
    {
        
    }

    [System.Serializable]
    public struct Filter
    {
        public Filter(EFilterCondition condition, EItemType itemType)
        {
            Condition = condition;
            ItemType = itemType;
        }
        public EFilterCondition Condition;
        public EItemType ItemType;

        public static bool operator ==(Filter left, Filter right) => (left.Condition == right.Condition && left.ItemType == right.ItemType);
        public static bool operator !=(Filter left, Filter right) => !(left == right);

        public override bool Equals(object obj) => base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();
        public override string ToString() => base.ToString();
    }
        
}