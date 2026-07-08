using UnityEngine;

namespace GameSystem
{
    /// <summary>
    /// Empty component just to find the player start position
    /// </summary>
    public class PlayerStart : MonoBehaviour
    {
        public static PlayerStart Instance { get; set; }
        private void OnEnable()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Debug.LogWarning("More than one PlayerStart component was put in the scene. Please leave only one");
                Destroy(this);
            }
        }
        private void OnDisable()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}