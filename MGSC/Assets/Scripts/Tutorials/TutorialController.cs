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
            foreach (var b in Reference.GetAll<Blender>())
            {
                //b.IsLocked = !active;
            }
        }
        public void SetAllActive_SittingCounters(bool active)
        {
            foreach (var b in Reference.GetAll<SittingCounter>())
            {
                b.ServingLocked = !active;
            }
        }
        
        public void SetAllActive_KillButtons(bool active)
        {
            foreach (var b in Reference.GetAll<SittingCounter>())
            {
                b.LockWithTutorial = !active;
            }
        }

    }
}