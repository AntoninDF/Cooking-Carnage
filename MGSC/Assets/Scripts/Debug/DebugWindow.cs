using System.Collections.Generic;
using GameSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using Upgrades;

public class DebugWindow : MonoBehaviour
{
    private bool _visible;
    private Rect _windowRect = new Rect(20, 20, 450, 500);
    private int _currentTab;
    private readonly string[] _tabs = { "General", "Settings", "Timers", "Clients", "Upgrades" };
    
    private float _gameSpeed = 1f;
    private float _deltaTime;

    private int _lastCurrentClientsCount;
    private int _lastHistoryClientsCount;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
            _visible = !_visible;

        _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
    }

    private void OnGUI()
    {
        if (!_visible)
        {
            DrawPressHint();
            return;
        }

        _windowRect = GUI.Window(0, _windowRect, DrawWindow, "Debug Window");
    }

    private void DrawPressHint()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(1f, 1f, 1f, 0.6f) }
        };
        GUI.Label(new Rect(10, 10, 300, 25), "Press F12 to open Debug Window", style);
    }

    private void DrawWindow(int windowID)
    {
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        for (int i = 0; i < _tabs.Length; i++)
        {
            if (GUILayout.Toggle(_currentTab == i, _tabs[i], "Button"))
                _currentTab = i;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        switch (_currentTab)
        {
            case 0: DrawGeneralTab(); break;
            case 1: DrawSettingsTab(); break;
            case 2: DrawTimersTab(); break;
            case 3: DrawClientsTab(); break;
            case 4: DrawUpgradesTab(); break;
        }

        GUILayout.EndVertical();
        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }

    #region Tabs
    private void DrawGeneralTab()
    {
        GUILayout.Label("Game Profile: Not Yet Implemented");
        GUILayout.Label($"Total Time: {Time.realtimeSinceStartup:F1}s");
        
        float fps = 1.0f / _deltaTime;
        GUILayout.Label($"FPS: {fps:F1}");

        DataManager.GameData data = ReferenceManager.Instance.GetManager<DataManager>().Data;

        GUILayout.Space(10);
        GUILayout.Label($"Current Shift :{data.shiftCount:F1}");
        GUILayout.Label($"Current Money :{data.money:F1}€");
        
        if (GUILayout.Button("Reload Scene"))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        GUILayout.Space(5);
        if (GUILayout.Button("Print All Managers"))
            Debug.Log("[DebugWindow] Managers: " + ReferenceManager.Instance);
        
    }

    private void DrawSettingsTab()
    {
        GUILayout.Label("Gameplay Settings", GUI.skin.box);

        GUILayout.Space(10);
        GUILayout.Label($"Game Speed: {_gameSpeed:F2}");
        _gameSpeed = GUILayout.HorizontalSlider(_gameSpeed, 0.1f, 10f);
        Time.timeScale = _gameSpeed;
    }

    private void DrawTimersTab()
    {
        GUILayout.Label("Active Timers", GUI.skin.box);

        var timerManager = ReferenceManager.Instance.GetManager<TimerManager>();
        if (timerManager == null)
        {
            GUILayout.Label("(No TimerManager found)");
            return;
        }

        foreach (var t in timerManager.Timers)
        {
            float remaining = t.Value.Duration - t.Value.Elapsed;
            bool isOver = remaining <= 0.01f;

            GUIStyle style = new GUIStyle(GUI.skin.label);
            if (isOver)
                style.normal.textColor = Color.green;

            GUILayout.Label($"⏱ Timer: {t.Key} — Remaining: {(isOver ? "Over" : $"{remaining:F1}s")}", style);
        }

        foreach (var t in timerManager.Frametimers)
        {
            float remaining = t.Value.FrameDuration - t.Value.ElapsedFrames;
            bool isOver = remaining <= 0.01f;

            GUIStyle style = new GUIStyle(GUI.skin.label);
            if (isOver)
                style.normal.textColor = Color.green;

            GUILayout.Label($"🎞 FrameTimer: {t.Key} — Remaining: {(isOver ? "Over" : $"{remaining:F1} frame(s)")}", style);
        }
    }

    private void DrawClientsTab()
    {
        GUILayout.Label("Client Information", GUI.skin.box);

        var clientManager = ReferenceManager.Instance.GetManager<ClientManager>();
        if (clientManager == null)
        {
            GUILayout.Label("(No ClientManager found)");
            return;
        }

        int currentCount = 0;
        int historyCount = 0;
        int killedCount = 0;
        int servedCount = 0;

        try
        {
            currentCount = clientManager.CurrentClients?.Count ?? 0;
            var history = clientManager.GetClientsHistory() ?? new List<ClientManager.ClientDataWIP>();
            historyCount = history.Count;

            foreach (var c in history)
            {
                if (c.killed ) killedCount++;
                else servedCount++;
            }
        }
        catch (System.Exception e)
        {
            GUILayout.Label($"Error accessing client data: {e.Message}");
        }

        if (currentCount != _lastCurrentClientsCount || historyCount != _lastHistoryClientsCount)
        {
            _lastCurrentClientsCount = currentCount;
            _lastHistoryClientsCount = historyCount;
        }

// Display counts
        GUILayout.Label($"Current Clients: {currentCount}");
        GUILayout.Label($"History Count: {historyCount}");

// Colored served/killed
        GUIStyle greenStyle = new GUIStyle(GUI.skin.label) { normal = { textColor = Color.green } };
        GUIStyle redStyle = new GUIStyle(GUI.skin.label) { normal = { textColor = Color.red } };

        GUILayout.BeginHorizontal();
        GUILayout.Label("Served Clients: ", GUI.skin.label);
        GUILayout.Label(servedCount.ToString(), greenStyle);
        GUILayout.Space(20);
        GUILayout.Label("Killed Clients: ", GUI.skin.label);
        GUILayout.Label(killedCount.ToString(), redStyle);
        GUILayout.EndHorizontal();

    }

    private void DrawUpgradesTab()
    {
        GUILayout.Label("Upgrades", GUI.skin.box);
        var upgradeManager = ReferenceManager.Instance.GetManager<UpgradeManager>();
        if (upgradeManager == null)
        {
            GUILayout.Label("(No UpgradeManager found)");
            return;
        }

        foreach (var up in upgradeManager.Upgrades)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{up.GetType().Name} — Level {up.Level}");
            if (GUILayout.Button("Level Up", GUILayout.Width(80)))
                up.LevelUp();
            GUILayout.EndHorizontal();
        }
    }
    #endregion
}
