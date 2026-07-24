using Cooking;
using System;
using System.Collections.Generic;
using Clients;
using UnityEngine;

namespace Orders
{
    /// <summary>
    /// Gives information on how, when, what... has been served
    /// </summary>
    public struct ServingInfo
    {
        public Servable     Meal          { get; private set; }
        public SO_Recipe    Recipe        { get; private set; }
        public EClientType? ServedDrink   { get; private set; }
        public float        Timestamp     { get; private set; }
        public bool         HasBeenServed => Meal != null || Recipe != null;

        public ServingInfo(Servable servedMeal, EClientType? servedDrink = null)
        {
            Meal        = servedMeal;
            Recipe      = servedMeal?.Recipe;
            ServedDrink = servedDrink;
            Timestamp   = Time.time;
        }

        public ServingInfo(SO_Recipe recipe, EClientType? servedDrink = null)
        {
            Meal      = null;
            Recipe    = recipe;
            ServedDrink = servedDrink;
            Timestamp = Time.time;
        }

        public static bool operator ==(ServingInfo left, ServingInfo right) => (left.Meal == right.Meal && left.Timestamp == right.Timestamp);
        public static bool operator !=(ServingInfo left, ServingInfo right) => !(left == right);

        public override bool Equals(object obj) =>
            obj is ServingInfo info &&
            EqualityComparer<Servable>.Default.Equals(Meal, info.Meal) &&
            Timestamp == info.Timestamp;

        public override int GetHashCode() => HashCode.Combine(Meal, Timestamp);
    }
}