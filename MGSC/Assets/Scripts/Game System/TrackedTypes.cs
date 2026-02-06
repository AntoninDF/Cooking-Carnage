using Clients;
using Controller;
using Cooking;
using UI;

namespace GameSystem
{
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
            typeof(Furnace),
            typeof(Oven),
            typeof(Oven2),
            typeof(TrashCan),
            typeof(Meal),
            typeof(Blender),
            typeof(SimpleClientSpawner),
            typeof(PlayerStart),
            typeof(PlayerInterface),
            typeof(OvenSetup),
            typeof(Entrance),
            typeof(PlayerController),
            typeof(UITarget),
            typeof(ItemSpawner),
            typeof(Blender)

                // ...
            };
        }
    }
}