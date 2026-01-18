
using Orders;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Cooking
{
    /// <summary>
    /// The ContainerFilter component should be added to a gameobject with a container component. It filters out which Grabbables can be grabbed.
    /// </summary>
    public class ContainerFilter : MonoBehaviour
    {
        #region Fields
        [SerializeField] public List<Filter> _filters = new();

        [SerializeField] private UnityEvent _onValidItem;
        [SerializeField] private UnityEvent _onInvalidItem;

        #endregion
        public bool IsValid(MonoBehaviour comparedItem)
        {
            bool result = true;
            foreach (Filter filter in _filters)
            {
                switch (filter.Condition)
                {
                    case EFilterCondition.MustBe:
                        if (!IsValid(comparedItem, filter.ItemType)) result = true;
                        break;
                    case EFilterCondition.MustNotBe:
                        if (IsValid(comparedItem, filter.ItemType)) result = true;
                        break;
                    default:
                        continue;
                }
            }
            if (result)
                _onValidItem?.Invoke();
            else
                _onInvalidItem?.Invoke();
            return result;
        }
        private bool IsValid(MonoBehaviour item, EItemType type)
        {
            switch (type)
            {
                case EItemType.Ingredient:
                    return item is Ingredient;
                case EItemType.CutIngredient:
                    return (item is Ingredient && ((Ingredient)item).IsCut);
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
                case EItemType.Oven:
                    return item is Oven;
                
                
            }
            return false;
        }
        public void AddFilter(Filter filter) => _filters.Add(filter);
        public void AddFilter(EFilterCondition condition, EItemType itemType) => _filters.Add(new Filter(condition, itemType));
        public void Clear() => _filters.Clear();

        
    }
    public enum EFilterCondition
    {
        MustBe,
        MustNotBe
    }

    // When an option is added to this enum, also add the corresponding logic to IsValid(MonoBehaviour, EItemType)
    // Do not change the IDs !! It will break every existing filter
    public enum EItemType
    {
        Ingredient=0,
        CutIngredient=1,
        Meal=2,
        Plate=3,
        Container=4,
        OrderTicket=5,

        Cookable=6,
        Trashable=7,

        Oven=8,
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