using System;
using System.Collections.Generic;
using GameSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using Upgrades;

[ExecuteInEditMode]
public class DebugWindow : MonoBehaviour
{
    private bool _visible;
    private Rect _windowRect = new Rect(20, 20, 450, 1000);
    private int _currentTab;
    private readonly string[] _tabs = { "General", "Settings", "Timers", "Clients", "Upgrades","Managers" };
    
    private float _gameSpeed = 1f;
    //private float _clientSpeed = 1f;
    private float _deltaTime;
    private float newMoney ;

    private int _lastCurrentClientsCount;
    private int _lastHistoryClientsCount;
    private int newShift;
    
    private Vector2 _clientScrollPos;

    private bool _previousMouseVisibility;
    private CursorLockMode _previousMouseLockMode;
    
    private readonly Vector2 referenceWindowSize = new Vector2(450, 500); 

    
    #region Methods
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            if (!_visible)
            {
                _previousMouseVisibility = Cursor.visible;
                _previousMouseLockMode = Cursor.lockState;

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = _previousMouseLockMode;
                Cursor.visible = _previousMouseVisibility;
            }
            _visible = !_visible;
        }

        _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
    }

    private void OnGUI()
    {
        if (!_visible)
        {
            //DrawPressHint();
            return;
        }
        
        float scaleX = _windowRect.width / referenceWindowSize.x;
        float scaleY = _windowRect.height / referenceWindowSize.y;
        Vector2 scale = new Vector2(scaleX, scaleY);
        
        Matrix4x4 oldMatrix = GUI.matrix;
        
        Vector2 pos = new Vector2(_windowRect.x, _windowRect.y);
        GUI.matrix = Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(scale.x, scale.y, 1f));
        
        GUI.Window(0, new Rect(0, 0, referenceWindowSize.x, referenceWindowSize.y), DrawWindow, "Debug Window");
        
        GUI.matrix = oldMatrix;
        
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
            case 5: DrawManagersTab(); break;
        }

        GUILayout.EndVertical();
        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }
    
    #endregion
    
    #region Tabs
    private void DrawGeneralTab()
    {
        GUILayout.Label("Game Profile: Not Yet Implemented");
        GUILayout.Label($"Total Time: {Time.realtimeSinceStartup:F1}s");
        
        float fps = 1.0f / _deltaTime;
        GUILayout.Label($"FPS: {fps:F1}");

        DataManager.GameData data = Reference.GetManager<DataManager>().Data;

        GUILayout.Space(10);
        if (data != null)
        {
            GUILayout.Label($"Current Shift :{data.shiftCount:F1}");
            GUILayout.Label($"Current Money :{data.money:F1}€");
        }
        else
        {
            GUILayout.Label($"No game data loaded");
        }

        if (GUILayout.Button("Reload Scene"))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void DrawSettingsTab()
    {
        GUILayout.Label("Settings", GUI.skin.box);

        GUILayout.Space(10);
        GUILayout.Label($"Time Scale: {_gameSpeed:F2}");
        _gameSpeed = GUILayout.HorizontalSlider(_gameSpeed, 0.1f, 10f);
        Time.timeScale = _gameSpeed;
        
        GUILayout.Space(10);
        
        GUILayout.Label($"Set Shift: {newShift:F2}");
        newShift = Mathf.RoundToInt(GUILayout.HorizontalSlider(newShift, 0, 99f));
        
        GUILayout.Label($"Set Money: {newMoney:F2}€");
        newMoney = GUILayout.HorizontalSlider(newMoney, 0.0f, 999f);

        if (GUILayout.Button("Reset Camera", GUILayout.Width(160)))
        {
            Reference.GetManager<CameraManager>().EnableFirstPersonView();
        }
        GUILayout.Space(10);

        if (GUILayout.Button("Save and reload", GUILayout.Width(160)))
        {
            DataManager.GameData data = new DataManager.GameData();
            data.shiftCount = newShift;
            data.money = newMoney;
            data.upgrades = Reference.GetManager<DataManager>().Data.upgrades;
            Reference.GetManager<DataManager>().SaveGameData(data);
            StudioEventEmitterExtension.StopAll_Static();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        GUILayout.Space(10);
        if (GUILayout.Button("Reset Save", GUILayout.Width(80)))
        {
            Reference.GetManager<DataManager>().ResetSave();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void DrawTimersTab()
    {
        GUILayout.Label("Active Timers", GUI.skin.box);

        var timerManager = Reference.GetManager<TimerManager>();
        if (timerManager == null)
        {
            GUILayout.Label("(No TimerManager found)");
            return;
        }

        // Standard Timers
        foreach (var t in timerManager.Timers)
        {
            float remaining = t.Value.Duration - t.Value.Elapsed;
            bool isOver = remaining <= 0.01f;

            GUIStyle style = new GUIStyle(GUI.skin.label);
            if (isOver)
                style.normal.textColor = Color.green;

            if (!t.Value.FrameBased)
            {
                GUILayout.Label($"⏱ Timer: {t.Key} — Remaining: {(isOver ? "Over" : $"{remaining:F1}s")}", style);
            }
            else
            {
                GUILayout.Label($"⏱ Timer: {t.Key} — Remaining: {(isOver ? "Over" : $"{remaining:F1} frames")}", style);
            }

            // Buttons for this timer
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Pause", GUILayout.Width(60)))
                t.Value.Pause(); 
            if (GUILayout.Button("Resume", GUILayout.Width(60)))
                t.Value.Resume();
            if (GUILayout.Button("Achieve", GUILayout.Width(60)))
                t.Value.End();
            if (GUILayout.Button("Reset", GUILayout.Width(60)))
                t.Value.Reset(t.Value.Name, t.Value.Duration, t.Value.onComplete, t.Value.onUpdate, t.Value.FrameBased);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
        }
    }

    private void DrawClientsTab()
{
    GUILayout.Label("Client Information", GUI.skin.box);

    var clientManager = Reference.GetManager<ClientManager>();
    if (clientManager == null)
    {
        GUILayout.Label("(No ClientManager found)");
        return;
    }

    int currentCount = 0;
    int historyCount = 0;
    int killedCount = 0;
    int servedCount = 0;

    List<ClientManager.ClientData> history = null;

    try
    {
        currentCount = clientManager.CurrentClients?.Count ?? 0;
        history = clientManager.GetClientsHistory() ?? new List<ClientManager.ClientData>();
        historyCount = history.Count;

        foreach (var c in history)
        {
            if (c.killed) killedCount++;
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

    // Summary counts
    GUILayout.Label($"Current Clients: {currentCount}");
    GUILayout.Label($"History Count: {historyCount}");

    GUIStyle greenStyle = new GUIStyle(GUI.skin.label) { normal = { textColor = Color.green } };
    GUIStyle redStyle = new GUIStyle(GUI.skin.label) { normal = { textColor = Color.red } };
    GUIStyle yellowStyle = new GUIStyle(GUI.skin.label) { normal = { textColor = Color.yellow } };

    GUILayout.BeginHorizontal();
    GUILayout.Label("Served Clients: ");
    GUILayout.Label(servedCount.ToString(), greenStyle);
    GUILayout.Space(20);
    GUILayout.Label("Killed Clients: ");
    GUILayout.Label(killedCount.ToString(), redStyle);
    GUILayout.EndHorizontal();

    GUILayout.Space(10);
    GUILayout.Label("Client List", GUI.skin.box);
    
    _clientScrollPos = GUILayout.BeginScrollView(_clientScrollPos, GUILayout.Height(300));

    // Remove hover highlight effect for boxes
    GUIStyle cleanBox = new GUIStyle(GUI.skin.box)
    {
        hover = { background = null },
        active = { background = null },
        normal = { background = GUI.skin.box.normal.background }
    };

    // ---- Active Clients ----
    if (clientManager.CurrentClients != null && clientManager.CurrentClients.Count > 0)
    {
        GUILayout.Label("Active Clients:", yellowStyle);
        foreach (var c in clientManager.CurrentClients)
        {
            string name = c.name ?? "Unknown";
            string type = c.Type.ToString() ?? "Unknown";
            string order = c.Order?.Recipe.Name ?? "None";

            GUILayout.BeginHorizontal(cleanBox);
            GUILayout.Label($"{name} ({type}) — Order: {order}");
            GUILayout.FlexibleSpace();
            GUILayout.Label("Status: Active", yellowStyle);
            GUILayout.EndHorizontal();
        }
    }
    else
    {
        GUILayout.Label("No active clients.");
    }

    GUILayout.Space(5);
    GUILayout.Label("Past Clients:", GUI.skin.box);

    // ---- Past Clients ----
    if (history != null && history.Count > 0)
    {
        foreach (var c in history)
        {
            string id = c.id.ToString() ?? "Unknown";
            string type = c.ClientType.ToString() ?? "Unknown";
            string order = c.order?.Recipe.Name ?? "None";
            
            GUIStyle statusStyle = c.killed ? redStyle : greenStyle;
            string statusText = c.killed ? "Killed" : "Served";
            
            if (!c.killed)
            {
                string plate = c.order?.ServingInfo == null ? "None" : c.order.ServingInfo.Recipe.Name;

                GUILayout.BeginHorizontal(cleanBox);
                GUILayout.Label($"{id}({type}) Order: {order} Plate Received: {plate} ");
            }
            else
            {
                GUILayout.BeginHorizontal(cleanBox);
                GUILayout.Label($"{id} ({type}) Order: {order}");
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label($"Status: {statusText}", statusStyle);
            GUILayout.EndHorizontal();
        }
    }
    else
    {
        GUILayout.Label("No history yet.");
    }

    GUILayout.EndScrollView(); 
}

    private void DrawUpgradesTab()
    {
        GUILayout.Label("Upgrades", GUI.skin.box);
        var upgradeManager = Reference.GetManager<UpgradeManager>();
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

    private void DrawManagersTab()
    {
        GUILayout.Label("Managers Overview", GUI.skin.box);

        var refManager = Reference.Instance;
        if (refManager == null)
        {
            GUILayout.Label("(No ReferenceManager found)");
            return;
        }

        var managers = refManager.Managers;
        if (managers == null || managers.Count == 0)
        {
            GUILayout.Label("No managers registered.");
            return;
        }
        
        foreach (var kvp in managers)
        {
            Type type = kvp.Key;

            
            GUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label($"{type.Name}", GUILayout.Width(150));
            
            bool info = Debugger.CheckDebug(type, LogType.Log);
            bool warning = Debugger.CheckDebug(type, LogType.Warning);
            bool error = Debugger.CheckDebug(type, LogType.Error);

            bool newInfo = GUILayout.Toggle(info, "Info", GUILayout.Width(50));
            bool newWarning = GUILayout.Toggle(warning, "Warning", GUILayout.Width(70));
            bool newError = GUILayout.Toggle(error, "Error", GUILayout.Width(50));

            
            if (newInfo != info)
                Debugger.ToggleDebugManager(type, LogType.Log);
            if (newWarning != warning)
                Debugger.ToggleDebugManager(type, LogType.Warning);
            if (newError != error)
                Debugger.ToggleDebugManager(type, LogType.Error);

            GUILayout.EndHorizontal();
        }
    }

    #endregion
}
