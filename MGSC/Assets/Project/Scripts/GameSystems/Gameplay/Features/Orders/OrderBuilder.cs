using Cooking;
using System.Collections.Generic;
using Clients;
using GameSystem;

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

    public OrderBuilder SetRecipe(SO_Recipe recipe)
    {
        Recipes.Add(recipe);
        return this;
    }
    public OrderBuilder SetOwner(Client client)
    {
        Owner = client;
        return this;
    }
    public OrderBuilder SetPrice()
    {
        return this;
    }
    public Order Build()
    {
        // Choisir une recette aléatoire parmi celles disponibles
        SO_Recipe chosen = Recipes.Count > 0 
            ? Recipes[UnityEngine.Random.Range(0, Recipes.Count)] 
            : null;

        int id = Reference.GetManager<OrderManager>() != null
            ? Reference.GetManager<GameSystem.OrderManager>().Orders.Count + 1
            : 0;

        if (Owner == null)
            UnityEngine.Debug.LogWarning("[OrderBuilder] Building an Order with no Owner");
        if (chosen == null)
            UnityEngine.Debug.LogWarning("[OrderBuilder] Building an Order with no Recipe");

        current = new Order(Owner, chosen, id);
        return current;
    }
    
}