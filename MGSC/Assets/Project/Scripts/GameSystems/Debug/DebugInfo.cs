using UnityEngine;

public class DebugInfo
{
    float lastFpsUpdate;
    float delayBetweenFpsUpdates = 0.2f;
    int currentFps;
    public void Show()
    {
        int px18 = Screen.height / 60; // 18 pixels is the size we want for a reference FHD screen, but it should scale with higher rez
        int px15 = Screen.height / 72; // Same
        int defaultY = px18;
        int yIncrement = px18;
        int currentLine = 1;

        Rect GetPos() => new Rect(px18, Screen.height - (defaultY + yIncrement * currentLine++), Screen.width * 0.45f, yIncrement);

        if ((Time.time - lastFpsUpdate >= delayBetweenFpsUpdates))
        {
            currentFps = Mathf.RoundToInt(1f / (Time.smoothDeltaTime / Time.timeScale));
            lastFpsUpdate = Time.time;
        }

        if (Globals.Authorizations.DisplayFPS)
        {
            GUI.Label(GetPos(), "FPS : " + currentFps);
        }
    }
}