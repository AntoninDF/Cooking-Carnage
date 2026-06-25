using Cooking;
using System.Collections.Generic;
using Clients;

public class OrderBuilder
{
    Order current;
    List<SO_Recipe> Recipes = new();
    Client Owner;
    int Price;

    protected OrderBuilder() { }
    public static OrderBuilder New()
    {
        var builder = new OrderBuilder();
        builder.current = new Order();
        return builder;
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
    public Order Build() => current;
}