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
    public SO_Ingredient Model => data;
    public bool IsBaseIngredient => Model.IsBaseIngredient;
    public F_Ingredient IngredientFeature { get; set; }
    public F_Cuttable Cuttable { get; set; }
    public F_Cookable Cookable { get; set; }

    #endregion

    #region Fields
    [Header("Ingredient references")]
    [Tooltip("Data model for this ingredient")]
    [SerializeField] protected SO_Ingredient data;
    [Tooltip("The collider that serves as viewing target for the player")]
    [SerializeField] protected ModelController cutModel;

    #endregion

    #region Methods

    protected override void Awake()
    {
        base.Awake();
        IngredientFeature = new(this, Model);
        Cuttable = new(this);
        Cookable = new(this);
        

        if (cutModel != null)
            Cuttable.OnTransformation += () => SetActiveModel(cutModel);


        //if (data == null)
        //throw new MissingReferenceException("Please assign an ingredient data in the inspector : " + name);
    }

    

    #endregion

}