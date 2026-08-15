using GameSystem;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Upgrades;

[ExecuteInEditMode]
public class DebugWindow : MonoBehaviour
{
    private bool _visible;
    // On initialise avec une taille raisonnable, la hauteur s'adaptera toute seule
    private Rect _windowRect = new Rect(20, 20, 450, 400); 
    private int _currentTab;
    private readonly string[] _tabs = { "General", "Settings", "Timers", "Clients", "Upgrades", "Managers", "Global variables" };
    
    private float _gameSpeed = 1f;
    private float _deltaTime;
    private float newMoney;

    private int _lastCurrentClientsCount;
    private int _lastHistoryClientsCount;
    private int newShift;
    
    // Scroll positions
    private Vector2 _clientScrollPos;
    private Vector2 _mainScrollPos; // Nouveau : pour scroller toute la fenêtre si besoin

    private bool _previousMouseVisibility;
    private CursorLockMode _previousMouseLockMode;

    const int INDENT_SIZE = 10;
    int indentLevel = 0;

    DebugInfo _screenInfo = new DebugInfo();

    #region Methods
    private void OnEnable()
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        Reference.AssetReferences.InputActions.FindAction("ShowDebugWindow").performed += ToggleWindow;
#endif
    }
    private void OnDisable()
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        Reference.AssetReferences.InputActions.FindAction("ShowDebugWindow").performed -= ToggleWindow;
#endif
    }
    private void Update()
    {

        _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
    }

    void ToggleWindow(InputAction.CallbackContext ctx) => ToggleWindow();
    private void ToggleWindow()
    {
        if (!_visible)
        {
            CursorUtility.RequestCursor(this);
        }
        else
        {
            CursorUtility.TryFreeCursor(this);
        }
        _visible = !_visible;
    }

    private void OnGUI()
    {
        _screenInfo.Show();
        if (!_visible) return;
        
        // Empêche la fenêtre d'être plus grande que l'écran
        float maxHeight = Screen.height - 40;

        // Création du style de la fenêtre
        GUIStyle style = new (GUI.skin.box);
        var tex = new Texture2D(1,1);
        tex.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.7f));
        tex.Apply();
        style.normal.background = tex;
        style.active.background = tex;
        style.focused.background = tex;
        style.hover.background = tex;
        style.hover.textColor = style.normal.textColor;

        // Création de la fenêtre avec une option pour limiter la hauteur max
        _windowRect = GUILayout.Window(0, _windowRect, DrawWindow, "Debug Window", style, GUILayout.Width(450), GUILayout.MaxHeight(maxHeight));
        
        // Empêche la fenêtre de sortir de l'écran
        _windowRect.x = Mathf.Clamp(_windowRect.x, 0, Screen.width - _windowRect.width);
        _windowRect.y = Mathf.Clamp(_windowRect.y, 0, Screen.height - _windowRect.height);
    }

    private void DrawWindow(int windowID)
    {
        // Début du Scroll global pour la fenêtre (au cas où l'écran est petit)
        _mainScrollPos = GUILayout.BeginScrollView(_mainScrollPos);

        GUILayout.BeginVertical();
        GUILayout.Space(30);
        // Onglets
        GUILayout.BeginHorizontal();
        for (int i = 0; i < _tabs.Length; i++)
        {
            // Petit hack visuel : si beaucoup d'onglets, on retourne à la ligne
            if (i > 0 && i % 3 == 0) 
            {
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
            }
            
            if (GUILayout.Toggle(_currentTab == i, _tabs[i], "Button", GUILayout.Height(25)))
                _currentTab = i;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(15);

        // Contenu
        switch (_currentTab)
        {
            case 0: DrawGeneralTab(); break;
            case 1: DrawSettingsTab(); break;
            case 2: DrawTimersTab(); break;
            case 3: DrawClientsTab(); break;
            case 4: DrawUpgradesTab(); break;
            case 5: DrawManagersTab(); break;
            case 6: DrawGlobalsTab(); break;
        }

        GUILayout.EndVertical();
        GUILayout.EndScrollView(); // Fin du scroll global

        // Zone pour déplacer la fenêtre (sur toute la largeur du haut)
        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }
    
#endregion
    
    #region Tabs
    private void DrawGeneralTab()
    {
        GUILayout.Label("Game Profile", GUI.skin.box);
        GUILayout.Label($"Total Time: {Time.realtimeSinceStartup:F1}s");
        
        float fps = 1.0f / _deltaTime;
        GUILayout.Label($"FPS: {fps:F1}");

        var data = SaveManager.Data;
                
        GUILayout.Space(10);
        if (GUILayout.Button("Reload Scene", GUILayout.Height(30)))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void DrawSettingsTab()
    {
        GUILayout.Label("Settings", GUI.skin.box);

        GUILayout.Space(10);
        GUILayout.Label($"Time Scale: {_gameSpeed:F2}");
        _gameSpeed = GUILayout.HorizontalSlider(_gameSpeed, 0.1f, 10f);
        if (GUI.changed) Time.timeScale = _gameSpeed;
        
        if(GUILayout.Button("Reset Speed")) { _gameSpeed = 1f; Time.timeScale = 1f; }
        
        GUILayout.Space(10);
        
        GUILayout.Label($"Set Shift: {Globals.Gameplay.ShiftNumber}");
        Globals.Gameplay.ShiftNumber.Value = Mathf.RoundToInt(GUILayout.HorizontalSlider(Globals.Gameplay.ShiftNumber, 0, 20));
        
        if (Reference.TryGetManager(out MoneyManager mm))
        {
            GUILayout.Label($"Set Money: {mm.MoneyInfo:F2}€");
            int newMoney = (int)GUILayout.HorizontalSlider(mm.MoneyInfo, 0.0f, 999f);
            mm.SetCurrentMoney(newMoney);
            if (GUILayout.Button("Get 100$"))
                mm.AddMoney(100);
        }

        GUILayout.Space(15);
        
        GUILayout.Space(5);

        if (GUILayout.Button("Save and Reload"))
        {
            SaveManager.GameData data = SaveManager.MakeGameData();
            SaveManager.SaveGameData(data);
            SaveManager.LoadGlobalData();
        }
        
        GUILayout.Space(5);
        
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Reset Save and reload scene"))
        {
            SaveManager.ResetSave();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        GUI.backgroundColor = Color.white;
    }

    private void DrawTimersTab()
    {
        GUILayout.Label("Active Timers", GUI.skin.box);

        if (TimerManager.Timers.Count == 0)
        {
            GUILayout.Label("No timers currently running.");
            return;
        }

        foreach (var t in TimerManager.Timers)
        {
            float remaining = t.Value.Duration - t.Value.Elapsed;
            bool isOver = remaining <= 0.01f;

            GUILayout.BeginVertical(GUI.skin.box);
            
            GUIStyle style = new GUIStyle(GUI.skin.label);
            if (isOver) style.normal.textColor = Color.green;

            string timeStr = !t.Value.FrameBased ? $"{remaining:F1}s" : $"{remaining:F1} frames";
            GUILayout.Label($"<b>{t.Key}</b>: {(isOver ? "Over" : timeStr)}", style);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Pause")) t.Value.Pause(); 
            if (GUILayout.Button("Resume")) t.Value.Resume();
            if (GUILayout.Button("End")) t.Value.End();
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
            GUILayout.Space(5);
        }
    }

    private void DrawClientsTab()
    {
        // Pas besoin de scrollview ICI car la fenêtre entière a maintenant un scrollview si ça dépasse
        // Cependant, pour la liste d'historique qui peut être très longue, on garde un scroll interne de taille fixe
        
        GUILayout.Label("Client Information", GUI.skin.box);

    var clientManager = Reference.GetManager<ClientManager>();
    if (clientManager == null)
    {
        GUILayout.Label("(No ClientManager found)");
        return;
    }

        int currentCount = clientManager.CurrentClients?.Count ?? 0;
        var history = clientManager.GetClientsHistory() ?? new List<ClientData>();
        
        int servedCount = 0;
        int killedCount = 0;
        foreach (var c in history) { if (c.killed) killedCount++; else servedCount++; }

        GUILayout.BeginHorizontal();
        GUI.color = Color.green; GUILayout.Label($"Served: {servedCount}");
        GUI.color = Color.red;   GUILayout.Label($"Killed: {killedCount}");
        GUI.color = Color.white;
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        
        // Active Clients
        GUILayout.Label($"Active Clients ({currentCount})", GUI.skin.box);
        if (clientManager.CurrentClients != null)
        {
            foreach (var c in clientManager.CurrentClients)
            {
                string name = c.name ?? "Unknown";
                //string type = c.Type.ToString();
                //string order = c.Order?.F_Recipe.Name ?? "None";
                //GUILayout.Label($"> {name} ({type}) : {order}");
            }
        }

        GUILayout.Space(10);
        GUILayout.Label($"History ({history.Count})", GUI.skin.box);

        // ScrollView spécifique pour l'historique pour éviter que la liste ne prenne 10km de haut
        _clientScrollPos = GUILayout.BeginScrollView(_clientScrollPos, GUI.skin.box, GUILayout.Height(300));
        
        foreach (var c in history)
        {
            string id = c.id.ToString();
            string type = c.clientType.ToString();
            string order = c.order?.Recipe.Name ?? "None";
            
            GUILayout.BeginHorizontal();
            if(c.killed) 
            {
                GUI.color = new Color(1, 0.5f, 0.5f);
                GUILayout.Label($"[KILLED] {id} ({type}) - Order: {order}");
            }
            else
            {
                GUI.color = Color.white;
                GUILayout.Label($"[SERVED] {id} ({type}) - Order: {order}");
            }
            GUILayout.EndHorizontal();
        }
        GUI.color = Color.white;
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
            GUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label($"{up.GetType().Name}", GUILayout.Width(150));
            GUILayout.Label($"Lvl {up.Level}");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Level Up +", GUILayout.Width(80)))
                up.LevelUp();
            GUILayout.EndHorizontal();
        }
    }

    private void DrawManagersTab()
    {
        GUILayout.Label("Managers Overview", GUI.skin.box);


        var managers = Reference.Managers;
        if (managers == null || managers.Count == 0)
        {
            GUILayout.Label("No managers registered.");
            return;
        }
        
        foreach (var kvp in managers)
        {
            Type type = kvp.Key;
            
            GUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label(type.Name, GUILayout.Width(150));
            
            bool info = Debugger.CheckDebug(type, LogType.Log);
            bool warning = Debugger.CheckDebug(type, LogType.Warning);
            bool error = Debugger.CheckDebug(type, LogType.Error);

            if (GUILayout.Toggle(info, "Info", GUILayout.Width(50)) != info)
                Debugger.ToggleDebugManager(type, LogType.Log);
                
            if (GUILayout.Toggle(warning, "Warn", GUILayout.Width(50)) != warning)
                Debugger.ToggleDebugManager(type, LogType.Warning);
                
            if (GUILayout.Toggle(error, "Err", GUILayout.Width(50)) != error)
                Debugger.ToggleDebugManager(type, LogType.Error);

            GUILayout.EndHorizontal();
        }

    }

    Dictionary<Type, bool> doDisplay = new();
    void DrawGlobalsTab()
    {
        DrawType(typeof(Globals));
    }
    void DrawStaticMembersForType(Type type)
    {
        foreach (var member in type.GetMembers())
        {
            // Filter for fields and properties
            if ((member.MemberType & (MemberTypes.Property | MemberTypes.Field)) == 0) continue;
            // Filter for static ppties/fields
            if (!member.IsStatic()) continue;

            object value;
            if (member.MemberType == MemberTypes.Property)
            {
                var obj = (PropertyInfo)member;
                value = obj.GetValue(null);
                Label(member.Name + " : " + value);
            }
            if (member.MemberType == MemberTypes.Field)
            {
                var obj = (FieldInfo)member;
                value = obj.GetValue(null);
                Label(member.Name + " : " + value);
            }
        }
    }
    void DrawType(Type type)
    {
        GUIStyle typeStyle = new();
        typeStyle.fontStyle = FontStyle.Bold;
        typeStyle.fontSize = 18;
        typeStyle.normal.textColor = Color.white;

        if (!doDisplay.ContainsKey(type))
            doDisplay.Add(type, false);

        doDisplay[type] = Toggle(doDisplay[type], type.Name, typeStyle);

        if (doDisplay[type])
        {
            indentLevel++;
            DrawStaticMembersForType(type);

            foreach (var nestedType in type.GetNestedTypes())
                DrawType(nestedType);
            indentLevel--;
        }

    }
    void Label(string text, GUIStyle style = null)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(INDENT_SIZE * indentLevel);
        if (style != null)
            GUILayout.Label(text, style);
        else
            GUILayout.Label(text);
        GUILayout.EndHorizontal();
    }
    bool Toggle(bool value, string text, GUIStyle style = null)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(INDENT_SIZE * indentLevel);
        if (style != null)
            value = GUILayout.Toggle(value, text, style);
        else
            value = GUILayout.Toggle(value, text);
        GUILayout.EndHorizontal();
        return value;
    }

    #endregion
}