using UnityEngine;

namespace Cooking
{
    /// <summary>
    /// The ContainerFilter component should be added to a gameobject with a container component. It filters out which Grabbables can be grabbed.
    /// </summary>

    public class ContainerFilter : MonoBehaviour
    {
        #region Fields
        [SerializeField] public ELogicOperator filterOperator;
        [SerializeField] public bool item = true;
        [SerializeField] public bool ingredient = true;
        [SerializeField] public bool plate = true;
        [SerializeField] public bool meal = true;
        [SerializeField] public bool container = true;

        #endregion
        public bool IsValid(MonoBehaviour comparedItem)
        {
            if (filterOperator == ELogicOperator.Or)
            {
                if (item && comparedItem is Item) return true;
                if (item)
                {
                    if (ingredient && comparedItem is Ingredient) return true;
                    if (plate && comparedItem is Plate) return true;
                    if (meal && comparedItem is Meal) return true;
                }
                if (container && comparedItem is IContainer) return true;
                return false;
            }
            else if (filterOperator == ELogicOperator.And)
            {
                if (item && comparedItem is not Item) return false;
                if (item)
                {
                    if (ingredient && comparedItem is not Ingredient) return false;
                    if (plate && comparedItem is not Plate) return false;
                    if (meal && comparedItem is not Meal) return false;
                }
                else if (!container) return false;
                if (container && comparedItem is not IContainer) return false;
                return true;
            }
            return false;
        }

        public enum ELogicOperator
        {
            Or,
            And
        }
    }
}