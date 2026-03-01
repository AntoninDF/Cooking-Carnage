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

            typeof(OrderingCounter),
            typeof(SittingCounter),
            typeof(Counter),
            typeof(Item),
            typeof(Ingredient),
            typeof(Oven),
            typeof(Meal),
            typeof(Blender),
            typeof(SimpleClientSpawner),
            typeof(PlayerStart),
            typeof(Player),
            typeof(OvenSetup),
            typeof(UITarget),
            typeof(Blender)

            // ...
        };
    }
}