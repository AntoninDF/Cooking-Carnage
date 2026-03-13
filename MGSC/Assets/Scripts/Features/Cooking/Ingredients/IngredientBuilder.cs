using Cooking;
using Features;

public class IngredientBuilder
{
    private Features.Ingredient current;

    protected IngredientBuilder() { }

    private static IngredientBuilder Make(Features.Ingredient ing)
    {
        IngredientBuilder builder = new();
        builder.current = ing;
        return builder;
    }
    public static IngredientBuilder MakeEye() => Make(new Eye(null));
    public static IngredientBuilder MakeTooth() => Make(new Tooth(null));
    public static IngredientBuilder MakeFiak() => Make(new Fiak(null));

    public IngredientBuilder SetOwner(IFeatureCollection owner)
    {
        current.SetOwner(owner);
        return this;
    }
    public IngredientBuilder SetModel(SO_Ingredient model)
    {
        current.Model = model;
        return this;
    }

    public Features.Ingredient Build() => current;
}
