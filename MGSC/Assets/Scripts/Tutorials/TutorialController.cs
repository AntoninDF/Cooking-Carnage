using Clients;
using Cooking;
using GameSystem;
using UnityEngine;

namespace Tutorials
{

    public class TutorialController : MonoBehaviour
    {
        public void SetAllActive_Blenders(bool active)
        {
            foreach (var b in ReferenceManager.GetAll<Blender>())
            {
                b.Locked = !active;
            }
        }
        public void SetAllActive_SittingCounters(bool active)
        {
            foreach (var b in ReferenceManager.GetAll<SittingCounter>())
            {
                b.ServingLocked = !active;
            }
        }

    }
}