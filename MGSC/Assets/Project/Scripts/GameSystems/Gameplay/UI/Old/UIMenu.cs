using UnityEngine;

public class UIMenu : MonoBehaviour
{


    private void Awake()
    {
        Disable();
    }

    public void Disable()
    {
        CursorUtility.TryFreeCursor(this);
        gameObject.SetActive(false);
    }
    public void Enable()
    {
        CursorUtility.RequestCursor(this);
        gameObject.SetActive(true);
    }
    public void SwitchTo(UIMenu otherMenu)
    {
        otherMenu.Enable();
        this.Disable();
    }
}
