using UI;
using UnityEngine;
using Cooking;
using Features;
using System.Runtime.InteropServices.WindowsRuntime;
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
        

        if (cutModel != null)
            Cuttable.OnTransformation += () => SetActiveModel(cutModel);


        //if (data == null)
        //throw new MissingReferenceException("Please assign an ingredient data in the inspector : " + name);
    }

    protected override void TryInteract(InteractionContext context)
    {
        Debug.Log("Ingredient interaction");
        if (TryGetGrabbed(context)) return;
        Debug.Log("Ingredient was not grabbed directly");

        if (!context.TryGetFeature(out F_Container playerContainer)) return;
        Debug.Log("1");
        if (playerContainer.HeldItem == null) return;
        Debug.Log("2");
        if (!playerContainer.HeldItem.TryGetSibling(out F_Container containerObject)) return;
        Debug.Log("Player has a container object");
        bool success = containerObject.RequestGrab(Grabbable);
        Debug.Log("Grab requested " + (success ? "successfully" : "and failed"));
    }

    #endregion

}