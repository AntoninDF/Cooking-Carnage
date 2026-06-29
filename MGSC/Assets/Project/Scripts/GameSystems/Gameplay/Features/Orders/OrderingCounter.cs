using System;
using Clients;

namespace Features
{
    public class OrderingCounter : Feature
    {
        public Action<Order> OnNewOrder { get; set; }


        public OrderingCounter(IFeatureCollection owner) : base(owner)
        {
        }

        public Order GetNewOrder(Client client)
        {
            var allRecipes = RecipeFinder.AllRecipes?.Recipes;
    
            var builder = OrderBuilder.New().SetOwner(client);
    
            if (allRecipes != null)
                foreach (var recipe in allRecipes)
                    builder.SetRecipe(recipe);

            var result = builder.Build();
            OnNewOrder?.Invoke(result);
            return result;
        }
    }
}