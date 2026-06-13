using Clients;
using Controller;
using Cooking;
using Features;
using GameSystem;
using UI;


public static class TrackedTypes
{
    public static System.Type[] GetTypes()
    {
        return new System.Type[]
        {
            // Add types to be tracked here
            typeof(Item),
            typeof(Ingredient),
            typeof(Oven),
            typeof(Blender),
            typeof(PlayerStart),
            typeof(Player),

            // ...
        };
    }
}