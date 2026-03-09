using Cooking;
using System.Collections.Generic;

public class OrderBuilder
{

    List<SO_Recipe> Recipes = new();
    Client Owner;
    int Price;

    public static OrderBuilder New()
    {
        return new OrderBuilder();
    }

    public OrderBuilder AddRecipe(SO_Recipe recipe)
    {
        Recipes.Add(recipe);
        return this;
    }
    public OrderBuilder SetOwner()
    {
        return this;
    }
    public OrderBuilder SetPrice()
    {
        return this;
    }

}