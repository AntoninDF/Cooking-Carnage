using UnityEditor;
using UnityEngine;

namespace GameSystem
{
    public class Quit : MonoBehaviour
    {

        public void QuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }
    }
}