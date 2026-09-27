using Controller;
using Features;
using UnityEngine;

public class DebugInfo
{
    public bool DisplayAll;

    float lastFpsUpdate;
    float delayBetweenFpsUpdates = 0.2f;
    int currentFps;
    public void Show()
    {
        // Setup for auto layout
        int px18 = Screen.currentResolution.height / 60; // 18 pixels is the size we want for a reference FHD screen, but it should scale with higher rez
        int px15 = Screen.currentResolution.height / 72; // Same
        int defaultY = px18 * 2;
        int yIncrement = px18*2;
        int currentLine = 1;
        Rect GetPos() => new Rect(px18, Screen.height - (defaultY + yIncrement * currentLine++), Screen.width * 0.45f, yIncrement);
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = px15;

        UpdateFpsCount();

        if (DisplayAll || Globals.Authorizations.DisplayFPS)
            GUI.Label(GetPos(), "FPS : " + currentFps, labelStyle);

        if (!DisplayAll) return;

        GUI.Label(GetPos(), "GameState : " + GameLoop.CurrentState.GetType().Name, labelStyle);
        GUI.Label(GetPos(), "Look target : " + GetLookedAtObject(), labelStyle);
        GUI.Label(GetPos(), "Held item : " + GetHeldItem(), labelStyle);
    }

    private void UpdateFpsCount()
    {
        // Get current FPS only a few times per second to avoid flickering text
        if ((Time.time - lastFpsUpdate >= delayBetweenFpsUpdates))
        {
            currentFps = Mathf.RoundToInt(1f / (Time.smoothDeltaTime / Time.timeScale));
            lastFpsUpdate = Time.time;
        }
    }

    private string GetLookedAtObject()
    {
        var p = PlayerSetup.Player;
        string result = "null";
        if (p.LookedAtObject != null)
        {
            result = p.LookedAtObject.Owner.ToString();
            if (p.LookedAtObject.TryGetSibling(out F_Container cont))
            {
                string content = cont.HeldItem != null ? cont.HeldItem.Owner.ToString() : "null";
                result += " (contains " + content + ")";
            }
        }
        return result;
    }
    private string GetHeldItem()
    {
        if (!PlayerSetup.Player.TryGetFeature(out F_Container container))
            return "invalid";
        if (container.HeldItem != null)
            return container.HeldItem.Owner.ToString();
        return "null";
    }
}