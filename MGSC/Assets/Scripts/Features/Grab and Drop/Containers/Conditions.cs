using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Features
{
    /// <summary>
    /// The Conditions component should be added to a gameobject with a container component. It filters out which Grabbables can be grabbed.
    /// </summary>
    public class Conditions : Feature
    {
        public List<Func<bool>> _conditions0 = new();
        public List<Func<object, bool>> _conditions1 = new();

        public Conditions(IFeatureCollection owner) : base(owner)
        {
        }
        public bool IsValid()
        {
            bool result = true;

            foreach(var condition in _conditions0)
            {
                if (!condition())
                {
                    result = false; break;
                }
            }
            return result;
        }
        public bool IsValid(object comparedObject, bool checkDefaultConditions=true)
        {
            if (checkDefaultConditions && !IsValid())
                return false;
            bool result = true;

            foreach(var condition in _conditions1)
            {
                if (!condition(comparedObject))
                {
                    result = false; break;
                }
            }
            return result;
        }
        
        public void AddCondition(Func<object, bool> condition) => _conditions1.Add(condition);
        public void RemoveCondition(Func<object, bool> condition) => _conditions1.Remove(condition);
        public void AddCondition(Func<bool> condition) => _conditions0.Add(condition);
        public void RemoveCondition(Func<bool> condition) => _conditions0.Remove(condition);

        public static implicit operator bool(Conditions conditions) => conditions.IsValid();
    }        
}