using UnityEngine;

namespace Cooking
{

    public class EnglobingSlotContainer : SlotContainer, IGrabbable
    {
        public override void Grab(IGrabbable item)
        {
            Debug.Log("Esc grabbing : " +  item);
            base.Grab(item);
        }
    }

}