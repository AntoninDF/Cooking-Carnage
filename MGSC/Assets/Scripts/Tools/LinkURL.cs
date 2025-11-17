using UnityEngine;

public class LinkURL : MonoBehaviour
{
    #region Fields
    [Header("Parameters")]
    [SerializeField] private string _defaultLink = "";

    #endregion

    #region Methods

    public void OpenURL(string link)
    {
        Application.OpenURL(link);
    }
    public void OpenDefaultURL()
    {
        OpenURL(_defaultLink);
    }

    #endregion
}
