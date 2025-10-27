using UnityEngine;
using UnityEngine.Events;

namespace Tutorials
{
    public class Tutorial : MonoBehaviour
    {
        [SerializeField] private UnityEvent _onTutorialComplete;

        public virtual void Complete() { _onTutorialComplete?.Invoke(); }
        public virtual void NextTutorial(Tutorial tutorial)
        {
            if (tutorial == null)
                return;
            tutorial.Setup();
        }
        public virtual void Setup() { }
    }
}