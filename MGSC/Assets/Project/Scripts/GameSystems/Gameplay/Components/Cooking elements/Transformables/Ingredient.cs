using UI;
using UnityEngine;
using Cooking;
using Features;
/// <summary>
/// Implements the base behaviour for every ingredient
/// </summary>
public class Ingredient : Item
{
    #region Properties
    public SO_Ingredient Model => _model;
    public bool IsBaseIngredient => Model.IsBaseIngredient;
    public Features.Ingredient IngredientFeature { get; set; }
    public Cuttable Cuttable { get; set; }
    public Cookable Cookable { get; set; }

    #endregion

    #region Fields
    [Header("Ingredient references")]
    [Tooltip("Data model for this ingredient")]
    [SerializeField] protected SO_Ingredient _model;



    #endregion

    #region Methods

    protected override void Awake()
    {
        base.Awake();
        IngredientFeature = new(this, Model);
        Cuttable = new(this);
        Cookable = new(this);
        //if (_model == null)
            //throw new MissingReferenceException("Please assign an ingredient model in the inspector : " + name);
    }
    void Start()
    {
        Collection.LateSetup();
    }
    #endregion

}