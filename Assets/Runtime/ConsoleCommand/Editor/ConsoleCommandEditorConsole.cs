/*************************************************************************\
Code provided under the "Apache Licence 2.0".
For more details, refer to the "LICENCE" file at the root of the project.
Written by RDNTLG (August 2025).
\*************************************************************************/

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RDNTLG.Command {

    /// <summary>
    /// Half-baked console to implement a way to interact with the
    /// <see cref="ConsoleCommandSystem"/> while being in the editor.
    /// </summary>
    public class ConsoleCommandEditorConsole : EditorWindow {

        #region Attributes

        /// <summary>
        /// The current command being written in
        /// the <see cref="EditorGUILayout.TextField(string,string,GUILayoutOption[])"/>.
        /// </summary>
        private string currentCommandInput = string.Empty;
        /// <summary>
        /// <see cref="List{T}"/> of logs being displayed
        /// in this window.
        /// </summary>
        private readonly List<string> logs = new List<string>();
        
        private Vector2 feedScroll = Vector2.zero;
        
        #endregion

        /// <summary>
        /// Show/Create a new <see cref="ConsoleCommandEditorConsole"/>.
        /// </summary>
        [MenuItem("Tools/RDNTLG/Console Command/Console Window (Experimental)")]
        public static void DisplayWindow() {
            GetWindow<ConsoleCommandEditorConsole>("Console Command (Experimental)");
        }

        #region EditorWindow's Methods

        private void OnEnable() {
            logs.Clear();
            Application.logMessageReceived += HandleUnityLogs;
        }

        private void OnDisable() {
            Application.logMessageReceived -= HandleUnityLogs;
        }

        private void OnGUI() {
            //"Style" stuff
            GUIStyle _logEntryBackground = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft };
            GUI.skin.label.wordWrap = true;
            GUI.skin.label.richText = true;
            
            //Title of the Window (Header)
            GUILayout.BeginHorizontal();
            GUILayout.Label("RDNTLG Console Command (Experimental)");
            if (GUILayout.Button("Clear", GUILayout.Width(100))) {
                logs.Clear();
                Repaint();
            }
            GUILayout.EndHorizontal();
            GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true));
            
            //Actual log feed
            feedScroll = GUILayout.BeginScrollView(feedScroll, false, false);
            for (int i=0; i<logs.Count; i++) {
                GUILayout.BeginVertical(_logEntryBackground);
                GUILayout.Label(logs[i]);
                GUILayout.EndVertical();
                
                GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true));
            }
            GUILayout.EndScrollView();
            GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true));
            
            //InputField/Execute buttons (Footer)
            GUILayout.BeginHorizontal();
            currentCommandInput = EditorGUILayout.TextField("Command To Execute:", currentCommandInput);
            if (GUILayout.Button("Execute", GUILayout.Width(150))) {
                if (!EditorApplication.isPlaying) {
                    Debug.Log($"[{nameof(ConsoleCommandAT)}] Unable to execute commands while not being in 'Play Mode'!");
                } else {
                    ConsoleCommandSystem.Execute(currentCommandInput);
                    currentCommandInput = ""; //Reset
                }
            }
            GUILayout.EndHorizontal();
        }

        #endregion
        
        #region ConsoleCommandEditorConsole's Internal Methods

        private void HandleUnityLogs(string _log, string _, LogType _type) {
            if (logs.Count > 500) { //Allow only 500 logs to be displayed (WAY too much but whatever)
                logs.RemoveAt(0);
            }
            
            logs.Add(_log);
            Repaint(); //Redraw everything
        }

        #endregion
        
    }
    
}