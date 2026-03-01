using UI;
using UnityEngine;
using Cooking;
/// <summary>
/// Implements the base behaviour for every ingredient
/// </summary>
public class Ingredient : Item
{
    #region Properties
    public SO_Ingredient Model => _model;
    public bool IsBaseIngredient => Model.IsBaseIngredient;
    public Features.Ingredient IngredientFeature { get; set; }

    #endregion

    #region Fields
    [Header("Ingredient references")]
    [Tooltip("Data model for this ingredient")]
    [SerializeField] protected SO_Ingredient _model;


    private ItemManager ItemManager => Reference.GetManager<ItemManager>();

    #endregion

    #region Methods

    protected override void Awake()
    {
        base.Awake();
        if (_model == null)
            throw new MissingReferenceException("Please assign an ingredient model in the inspector : " + name);
    }
    void Start()
    {
        Collection.LateSetup();
    }
    #endregion

}