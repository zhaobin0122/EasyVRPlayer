/*************************************************************
 * 
 *  Copyright(c) 2017 Lyrobotix.Co.Ltd.All rights reserved.
 *  NoloVR_Logs.cs
 *   
*************************************************************/

using UnityEngine;
using System.Collections.Generic;
public enum NoloLogType
{
    Console,
    Screen
}
public class NoloVR_Logs : MonoBehaviour {
    private bool drawScreen = true;
    private NoloLogType logType = NoloLogType.Screen;
    public void SetLogType(NoloLogType type)
    {
        logType = type;
    }
    // NoloLog struct
    struct NoloLog
    {
        public string logMessage;
        public string stackTrace;
        public LogType type;
    }
    readonly List<NoloLog> logs = new List<NoloLog>();
    Vector2 screenPosition;// 
    bool collapse;//collapse log
    static readonly Dictionary<LogType, Color> logTypeColors = new Dictionary<LogType, Color>
    {
            { LogType.Assert, Color.white },
            { LogType.Error, Color.red },
            { LogType.Exception, Color.red },
            { LogType.Log, Color.white },
            { LogType.Warning, Color.yellow },
    };

    const int margin = 20;
    static readonly GUIContent clearLabel = new GUIContent("Clear");
    static readonly GUIContent collapseLabel = new GUIContent("Collapse");
    Rect windowRect = new Rect(margin, margin, Screen.width - (margin * 2), Screen.height - (margin * 2));

    #region MomoFunc
    void Start()
    {
        switch (logType)
        {
            case NoloLogType.Console:
                break;
            case NoloLogType.Screen:
                drawScreen = true;
                break;
            default:
                break;
        }
    }
    void OnGUI()
    {
        if (drawScreen)
        {
            windowRect = GUILayout.Window(1, windowRect, DrawConsoleWindow, "Console");//GUI绘制窗口
        }
    }
    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;// 注册log事件
    }
    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;// 注销log事件
    }
    #endregion

    void HandleLog(string message, string stackTrace, LogType type)
    {
        logs.Add(new NoloLog
        {
            logMessage = message,
            stackTrace = stackTrace,
            type = type,
        });

        //TrimExcessLogs();
    }
    void DrawConsoleWindow(int windowID)
    {
        DrawLogsList();
        DrawToolbar();

        // Allow the window to be dragged by its title bar.  
        //GUI.DragWindow(titleBarRect);
    }
    void DrawLogsList()
    {
        screenPosition = GUILayout.BeginScrollView(screenPosition);
        
        // Iterate through the recorded logs.  
        for (var i = 0; i < logs.Count; i++)
        {
            var log = logs[i];

            // Combine identical messages if collapse option is chosen.  
            if (collapse && i > 0)
            {
                var previousMessage = logs[i - 1].logMessage;

                if (log.logMessage == previousMessage)
                {
                    continue;
                }
            }

            GUI.contentColor = logTypeColors[log.type];
            GUILayout.Label(log.logMessage);
        }

        GUILayout.EndScrollView();

        // Ensure GUI colour is reset before drawing other components.  
        GUI.contentColor = Color.white;
    }
    void DrawToolbar()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(clearLabel))
        {
            logs.Clear();
        }
        collapse = GUILayout.Toggle(collapse, collapseLabel, GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
    }

}
