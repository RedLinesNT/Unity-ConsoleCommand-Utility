/*************************************************************************\
Code provided under the "Apache Licence 2.0".
For more details, refer to the "LICENCE" file at the root of the project.
Written by RDNTLG (August 2025).
\*************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace RDNTLG.Command {

    /// <summary>
    /// <see cref="ConsoleCommandAT"/> (<i>Console Command Authoring Tools</i>) provide methods
    /// to perform authoring operations inside the editor related to the
    /// <see cref="RDNTLG.Command"/> namespace.
    /// </summary>
    public static class ConsoleCommandAT {

        /// <summary>
        /// Check the signature of each method in the assembly
        /// using the <see cref="ConsoleCommandAttribute"/>.
        /// </summary>
        /// <remarks>
        /// The expected signature is defined in <see cref="ConsoleCommandSystem.CommandMethod"/>.
        /// </remarks>
        [MenuItem("Tools/RDNTLG/Console Command/Check signatures...")]
        public static void CheckSignatures() {
            //Almost the entire code here is a re-paste of the one written in 
            //ConsoleCommandSystem.Initialize(), which is bad practice.
            //I should have a method that both methods uses instead...
            
            //Get the methods with our attribute...
            MethodInfo[] _methods = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(_a => _a.GetTypes())
                .Where(_x => _x.IsClass)
                .SelectMany(_x => _x.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                .Where(_x => _x.GetCustomAttributes(typeof(ConsoleCommandAttribute), false).FirstOrDefault() != null).ToArray();
            
            foreach (MethodInfo _method in _methods) {
                ConsoleCommandAttribute _attribute = _method.GetCustomAttribute<ConsoleCommandAttribute>();

                Delegate _commandHandler = Delegate.CreateDelegate(typeof(ConsoleCommandSystem.CommandMethod), _method, false);
                if (_commandHandler != null) { //The method signature is valid! EPIC!!!!
                    //Ignore...
                } else { //Invalid signature...
                    Debug.LogError($"[{nameof(ConsoleCommandAT)}] Invalid signature for '{_method.DeclaringType} - {_method.Name}'!");
                }
            }

            
            Debug.Log($"[{nameof(ConsoleCommandAT)}] Checked the signature of {_methods.Length} {nameof(ConsoleCommandAttribute)}!");
        }

        /// <summary>
        /// Dump all <see cref="ConsoleCommandAttribute"/> into
        /// "<c>{PROJ_ROOT}/ConsoleCommandsDump.md</c>"
        /// </summary>
        [MenuItem("Tools/RDNTLG/Console Command/Dump commands to markdown...")]
        public static void DumpForMarkdown() {
            Stopwatch _watch = new Stopwatch();
            _watch.Start();
            
            Dictionary<ConsoleCommandAttribute, ConsoleCommandSystem.CommandMethod> _commands = new Dictionary<ConsoleCommandAttribute, ConsoleCommandSystem.CommandMethod>();
            
            //This is ALSO a re-paste of the one written in 
            //ConsoleCommandSystem.Initialize(), which is bad practice.
            //I should have a method that both methods uses instead...
            
            //Get the methods with the attribute
            MethodInfo[] _methods = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(_a => _a.GetTypes())
                .Where(_x => _x.IsClass)
                .SelectMany(_x => _x.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                .Where(_x => _x.GetCustomAttributes(typeof(ConsoleCommandAttribute), false).FirstOrDefault() != null).ToArray();

            foreach (MethodInfo _method in _methods) {
                ConsoleCommandAttribute _attribute = _method.GetCustomAttribute<ConsoleCommandAttribute>();

                Delegate _commandHandler = Delegate.CreateDelegate(typeof(ConsoleCommandSystem.CommandMethod), _method, false);
                if (_commandHandler != null) { //The method definition is valid
                    if (!_commands.ContainsKey(_attribute)) {
                        _commands.Add(_attribute, (ConsoleCommandSystem.CommandMethod)_commandHandler);
                    }
                } else { 
                    //Ignore invalid signatures...
                }
            }
            
            string _filePath = Path.GetFullPath($"{Application.dataPath}/../CommandsDump.md");

            //Write the feed
            string _feed = "# This file is auto-generated; any changes to it won't be saved!\n" +
                           "### Invalid signatures are ignored and not displayed here.\n" +
                            $"Active version '{Application.version}'<br>\n" +
                            $"Export requested by '{Environment.UserName}' ({SystemInfo.deviceName} - {SystemInfo.operatingSystem}) at '{DateTime.Now:yyyy-MM-dd HH:mm:ss}' into '{_filePath}'.\n" +
                            "<hr>\n \n" +
                            "| Command | Supported Args | Example | Execute on MainThread | Target Method |\n" +
                            "|---------|----------------|---------|-----------------------|---------------|\n";
            
            foreach (KeyValuePair<ConsoleCommandAttribute, ConsoleCommandSystem.CommandMethod> _command in _commands) {
                _feed += $"| {_command.Key.Command} | " +
                         $"{_command.Key.SupportedArgs} | " +
                         $"{_command.Key.Example} | " +
                         $"{_command.Key.ExecOnMainThread} | " +
                         $"{_command.Value.Method.DeclaringType?.Namespace}.{_command.Value.Method.Name} |\n";
            }
            
            //Finalize the write process
            if (File.Exists(_filePath)) {
                File.Delete(_filePath);
            } 
            
            File.Create(_filePath).Close();
            File.WriteAllText(_filePath, _feed, System.Text.Encoding.UTF8);

            _watch.Stop();
            Debug.Log($"[{nameof(ConsoleCommandAT)}] {_commands.Count} {nameof(ConsoleCommandAttribute)}s have been dumped into '{_filePath}'! ({_watch.ElapsedMilliseconds}ms elapsed)");
        }
        
    }
    
}