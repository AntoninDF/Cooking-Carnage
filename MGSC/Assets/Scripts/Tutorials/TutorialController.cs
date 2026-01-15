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

            }
        }

    }
}