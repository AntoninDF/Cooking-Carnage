
using Orders;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking
{
    /// <summary>
    /// The ContainerFilter component should be added to a gameobject with a container component. It filters out which Grabbables can be grabbed.
    /// </summary>
    public class ContainerFilter : MonoBehaviour
    {
        #region Fields
        [SerializeField] public List<Filter> _filters = new();

        #endregion
        public bool IsValid(MonoBehaviour comparedItem)
        {
            foreach (Filter filter in _filters)
            {
                switch (filter.Condition)
                {
                    case EFilterCondition.MustBe:
                        if (!IsValid(comparedItem, filter.ItemType)) return false;
                        break;
                    case EFilterCondition.MustNotBe:
                        if (IsValid(comparedItem, filter.ItemType)) return false;
                        break;
                    default:
                        continue;
                }
            }
            return true;
        }
        private bool IsValid(MonoBehaviour item, EItemType type)
        {
            switch (type)
            {
                case EItemType.Ingredient:
                    return item is Ingredient;
                case EItemType.BaseIngredient:
                    return (item is Ingredient && ((Ingredient)item).IsBaseIngredient);
                case EItemType.Meal:
                    return item is Meal;
                case EItemType.Plate:
                    return item is Plate;
                case EItemType.Container:
                    return item is IContainer;
                case EItemType.OrderTicket:
                    return item is OrderTicket;
                case EItemType.Cookable:
                    return item is ICookable;
                case EItemType.Trashable:
                    return item is ITrashable;
                
                
            }
            return false;
        }
        public void AddFilter(Filter filter) => _filters.Add(filter);
        public void AddFilter(EFilterCondition condition, EItemType itemType) => _filters.Add(new Filter(condition, itemType));
        public void Clear() => _filters.Clear();

        
        public enum EFilterCondition
        {
            MustBe,
            MustNotBe
        }

        // When an option is added to this enum, also add the corresponding logic to IsValid()
        // Do not change the IDs !! It will break every existing filter
        public enum EItemType
        {
            Ingredient=0,
            BaseIngredient=1,
            Meal=2,
            Plate=3,
            Container=4,
            OrderTicket=5,

            Cookable=6,
            Trashable=7
        }

        [System.Serializable]
        public struct Filter
        {
            public Filter(EFilterCondition condition, EItemType itemType)
            {
                Condition = condition;
                ItemType = itemType;
            }
            public EFilterCondition Condition;
            public EItemType ItemType;

            public static bool operator ==(Filter left, Filter right) => (left.Condition == right.Condition && left.ItemType == right.ItemType);
            public static bool operator !=(Filter left, Filter right) => !(left == right);

            public override bool Equals(object obj) => base.Equals(obj);
            public override int GetHashCode() => base.GetHashCode();
            public override string ToString() => base.ToString();
        }
        
    }
}