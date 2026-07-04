using Cooking;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Orders
{
    /// <summary>
    /// Gives information on how, when, what... has been served
    /// </summary>
    public struct ServingInfo
    {
        public Servable      Meal   { get; private set; }
        public SO_Recipe Recipe { get; private set; } 
        public float Timestamp { get; private set; }

        public ServingInfo(Servable servedMeal)
        {
            Meal      = servedMeal;
            Recipe    = servedMeal?.Recipe;
            Timestamp = Time.time;
        }
        
        public ServingInfo(SO_Recipe recipe)
        {
            Meal      = null;
            Recipe    = recipe;
            Timestamp = Time.time;
        }

        public static bool operator == (ServingInfo left, ServingInfo right) => (left.Meal == right.Meal && left.Timestamp == right.Timestamp);
        public static bool operator != (ServingInfo left, ServingInfo right) => !(left == right);

        public override bool Equals(object obj)
        {
            return obj is ServingInfo info &&
                   EqualityComparer<Servable>.Default.Equals(Meal, info.Meal) &&
                   Timestamp == info.Timestamp;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Meal, Timestamp);
        }
    }
}