using UnityEditor;
using UnityEngine;

namespace GameSystem
{
    public class MethodUtility : MonoBehaviour
    {

        public void QuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }

        public void OpenURL(string link)
        {
            Application.OpenURL(link);
        }
    }
}