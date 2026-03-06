using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UITabButton : MonoBehaviour
{
    public bool Enabled {
        get => _enabled;
        set
        {
            if (value == _enabled) return;
            _enabled = value;
            if (_enabled) OnTabEnabled.Invoke();
            else OnTabDisabled.Invoke();
        }
    }

    public UITabContent controlledTab;
    public Image controlledImage;
    public Button Button;
    public UnityEvent OnTabEnabled;
    public UnityEvent OnTabDisabled;

    private bool _enabled;

    private void Start()
    {
        if (Button == null)
            Button = GetComponent<Button>();

        BindEvents();
    }

    private void BindEvents()
    {
        if (Button == null) return;

        //Button.onClick.AddListener(EnableTab);

        if (controlledTab == null) return;
    }

    public void EnableTab()
    {
        Enabled = true;
        if (controlledTab == null) return;
        controlledTab.Enable();

        controlledImage.enabled = true;
    }
    public void DisableTab()
    {
        Enabled = false;
        Debug.Log("Disabling...");
        if (controlledTab == null) return;
        Debug.Log("Disabled !!!"); 
        controlledTab.Disable();

        controlledImage.enabled = false;
    }
}
