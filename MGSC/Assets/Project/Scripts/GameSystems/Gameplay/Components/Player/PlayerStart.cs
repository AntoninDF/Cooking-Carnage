using UnityEngine;

namespace GameSystem
{
    /// <summary>
    /// Empty component just to find the player start position
    /// </summary>
    public class PlayerStart : MonoBehaviour
    {
        public static PlayerStart Instance { get; set; }
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Debug.LogWarning("More than one PlayerStart component was put in the scene. Please leave only one");
                Destroy(this);
            }
        }
    }
}