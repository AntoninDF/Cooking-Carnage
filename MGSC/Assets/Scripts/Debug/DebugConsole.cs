using System;
using System.Collections.Generic;
using UnityEngine;

public class DebugConsole : MonoBehaviour
{
    [Header("Console Settings")]
    [SerializeField] private int maxVisibleLines = 6;
    [SerializeField] private Vector2 padding = new Vector2(10, 10);
    [SerializeField] private int fontSize = 10;
    [SerializeField] private float logLifetime = 5f;

    private GUIStyle _labelStyle;
    private List<LogEntry> _displayedLogs = new List<LogEntry>();
    bool _visible = true;

    private void OnEnable()
    {
        Debugger.OnLogAdded += HandleNewLog;
    }

    private void OnDisable()
    {
        Debugger.OnLogAdded -= HandleNewLog;
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11))
        {
            _visible = !_visible;
        }

        if (logLifetime <= 0)
            return;

        float now = Time.time;

        for (int i = _displayedLogs.Count - 1; i >= 0; i--)
        {
            if (now - _displayedLogs[i].TimeCreated > logLifetime)
                _displayedLogs.RemoveAt(i);
        }
    }

    private void HandleNewLog(string message, LogType logType)
    {
        _displayedLogs.Add(new LogEntry
        {
            Message = message,
            Type = logType,
            TimeCreated = Time.time
        });

        if (_displayedLogs.Count > 1000)
            _displayedLogs.RemoveAt(0);
    }

    private void OnGUI()
    {
        if (_visible)
        {
            DrawConsole();
        }
    }

    void DrawConsole()
    {
        if (_labelStyle == null)
        {
            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                wordWrap = false
            };
        }

        float lineHeight = _labelStyle.lineHeight*1.5f;
        int visibleLines = Mathf.Min(maxVisibleLines, _displayedLogs.Count);


        for (int i = 0; i < visibleLines; i++)
        {
            int logIndex = _displayedLogs.Count - visibleLines + i;
            var entry = _displayedLogs[logIndex];
            _labelStyle.normal.textColor = GetColorForLogType(entry.Type);

            Rect lineRect = new Rect(
                padding.x,
                Screen.height - padding.y - (visibleLines - i) * lineHeight,
                Screen.width - 2 * padding.x,
                lineHeight
            );

            GUI.Label(lineRect, entry.Message, _labelStyle);
        }
    }

    private Color GetColorForLogType(LogType type)
    {
        return type switch
        {
            LogType.Warning => Color.yellow,
            LogType.Error => Color.red,
            _ => Color.white
        };
    }

    private class LogEntry
    {
        public string Message;
        public LogType Type;
        public float TimeCreated;
    }
}
