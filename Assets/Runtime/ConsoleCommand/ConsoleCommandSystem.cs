/*************************************************************************\
Code provided under the "Apache Licence 2.0".
For more details, refer to the "LICENCE" file at the root of the project.
Written by RDNTLG (August 2025).
\*************************************************************************/

#if RDNTLG_CCM || UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.Scripting;
using System.Text;

namespace RDNTLG.Command {

    /// <summary>
    /// Utility made to listen for incoming commands via
    /// either the terminal or editor to execute specific
    /// methods within the assembly using the
    /// <see cref="ConsoleCommandAttribute"/>.
    /// </summary>
    /// <remarks>
    /// <para>The expected method's signature is explained
    /// in the <see cref="ConsoleCommandAttribute"/>'s documentation.</para>
    /// <para>Authoring tools are available under the
    /// <c>Tools > RDNTLG > Console Command</c> tab in the editor.</para>
    /// <para>Only available under the <c>RDNTLG_CCM</c>/<c>UNITY_EDITOR</c>
    /// preprocessor definitions.</para>
    /// </remarks>
    public static class ConsoleCommandSystem {

        #region Attributes

        /// <summary>
        /// Encapsulate a valid method using the
        /// <see cref="ConsoleCommandAttribute"/>.
        /// </summary>
        public delegate void CommandMethod(ArgumentsParser _args);
        
        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of <see cref="CommandMethod"/>s
        /// using the <see cref="ConsoleCommandAttribute"/>.
        /// </summary>
        private static readonly Dictionary<ConsoleCommandAttribute, CommandMethod> commands = new Dictionary<ConsoleCommandAttribute, CommandMethod>();
        /// <summary>
        /// The <see cref="Thread"/> used to listen for incoming
        /// inputs from a terminal with <see cref="Console.ReadLine"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="ListenInput"/>.
        /// </remarks>
        private static Thread systemThread = null;
        /// <summary>
        /// <see cref="Queue{T}"/> of <see cref="Action"/>s to be
        /// executed onto the Main Thread.
        /// </summary>
        private static Queue<Action> mainThreadQueue = new Queue<Action>();

        #endregion

        /// <summary>
        /// Initialize the <see cref="ConsoleCommandSystem"/>.
        /// </summary>
        /// <remarks>
        /// Launch time: <see cref="RuntimeInitializeLoadType.SubsystemRegistration"/>.
        /// </remarks>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize() {
            //Get the methods with our attribute...
            MethodInfo[] _methods = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName == Assembly.GetExecutingAssembly().GetName().FullName) //Get only methods from THIS assembly
                .SelectMany(_a => _a.GetTypes())
                .Where(_x => _x.IsClass)
                .SelectMany(_x => _x.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                .Where(_x => _x.GetCustomAttributes(typeof(ConsoleCommandAttribute), false).FirstOrDefault() != null).ToArray();
            
            foreach (MethodInfo _method in _methods) {
                ConsoleCommandAttribute _attribute = _method.GetCustomAttribute<ConsoleCommandAttribute>();

                Delegate _commandHandler = Delegate.CreateDelegate(typeof(CommandMethod), _method, false);
                if (_commandHandler != null) { //The method signature is valid! EPIC!!!!
                    if (!commands.ContainsKey(_attribute)) {
                        commands.Add(_attribute, (CommandMethod)_commandHandler);
                    }
                } else { //Invalid signature...
                    Debug.LogError($"[{nameof(ConsoleCommandSystem)}] Invalid signature for '{_method.DeclaringType} - {_method.Name}'!");
                }
            }
            
            //Insert the "FrameUpdate" method into the PlayerLoop.
            //"subSystemList[4]" correspond to "Update". If you don't know any of this
            //and wish to understand what the hell I'm trying to do, please read:
            //https://docs.unity3d.com/ScriptReference/LowLevel.PlayerLoop.html/
            //
            //This will also ensure the system runs on the Thread used by Unity's
            //scripting assemblies.
            PlayerLoopSystem _currentLoopSystem = PlayerLoop.GetCurrentPlayerLoop();
            _currentLoopSystem.subSystemList[4].updateDelegate += FrameUpdate;
            PlayerLoop.SetPlayerLoop(_currentLoopSystem);

            Application.quitting += Terminate;
            
            //Set up the thread
            systemThread = new Thread(ListenInput);
            systemThread.Start();
            
            Debug.Log($"[{nameof(ConsoleCommandSystem)}] {_methods.Length} {nameof(ConsoleCommandAttribute)} has been loaded!");
        }

        #region ConsoleCommandSystem's Internal Methods

        /// <summary>
        /// Terminate the <see cref="ConsoleCommandSystem"/>.
        /// </summary>
        private static void Terminate() {
            systemThread?.Abort();
        }

        /// <summary>
        /// Listen for incoming inputs with <see cref="Console.ReadLine"/>.
        /// </summary>
        /// <remarks>
        /// DO NOT ATTEMPT to run that on the main thread!
        /// </remarks>
        private static void ListenInput() {
            while (true) {
                string _input = Console.ReadLine();
                if (string.IsNullOrEmpty(_input)) continue;

                if (_input.ToLower() == "exit") {
                    mainThreadQueue.Enqueue(Application.Quit);
                    return;
                }

                Execute(_input.Trim().ToLower());
            }
        }

        /// <summary>
        /// Called each frame from the main thread
        /// to dequeue <see cref="Action"/>s from
        /// <see cref="mainThreadQueue"/>.
        /// </summary>
        private static void FrameUpdate() {
            if (!mainThreadQueue.TryDequeue(out Action _action)) return;

            try {
                _action?.Invoke();
            } catch (Exception _ex) {
                Debug.LogException(_ex); //???
            }
        }
        
        #endregion

        #region ConsoleCommandSystem's External Methods

        /// <summary>
        /// Try to execute a command.
        /// </summary>
        public static void Execute(string _command) {
            if (string.IsNullOrEmpty(_command)) return;
            
            foreach (KeyValuePair<ConsoleCommandAttribute, CommandMethod> _com in commands) {
                if (!_command.Contains(_com.Key.Command.Trim(), StringComparison.CurrentCultureIgnoreCase)) continue;
                
                Regex _regex = new Regex(@"(?:[^\s""]+|""[^""]*"")+"); //Parsing argument stuff, insanely funny
                MatchCollection _matches = _regex.Matches(_command.Replace(_com.Key.Command.Trim(), string.Empty).Trim());

                string[] _commandSplit = new string[_matches.Count];
                for (int i = 0; i < _matches.Count; i++) {
                    _commandSplit[i] = _matches[i].Value;
                }
                
                ArgumentsParser _args = new ArgumentsParser(_commandSplit);
                
                try {
                    string _logHeader = $"---------- Result for command: {_com.Key.Command} ----------";
                    string _logFooter = $"";
                    for (int i = 0; i < _logHeader.Length; i++) _logFooter += "-";
                    
                    
                    if (_com.Key.ExecOnMainThread) { //Must be executed on the MainThread
                        mainThreadQueue.Enqueue(() => {
                                Debug.Log(_logHeader);
                                _com.Value.Invoke(_args);
                                Debug.Log(_logFooter);
                            });
                        
                    } else { //Doesn't matter
                        Debug.Log(_logHeader);
                        _com.Value.Invoke(_args);
                        Debug.Log(_logFooter);
                    }
                } catch (Exception _ex) {
                    Debug.LogError($"[{nameof(ConsoleCommandSystem)}] Failed to execute command {_command}! Exception: {_ex}");
                }
                
                return;
            }
            
            Debug.LogWarning($"[{nameof(ConsoleCommandSystem)}] Unknown command '{_command}'.");
            //TODO: Unknown command "something", do you meant? ...
        }

        #endregion

        #region ConsoleCommandSystem's Console Commands

        /// <summary>
        /// Print all <see cref="ConsoleCommandAttribute"/> registered
        /// and available to use.
        /// </summary>
        [ConsoleCommand("help", "Print all commands", null, false), Preserve]
        private static void HandleDumpDebugCommand(ArgumentsParser _args) {
            string _printResult = string.Empty;

            foreach (KeyValuePair<ConsoleCommandAttribute, CommandMethod> _command in commands) {
                _printResult += $"     {_command.Key.Command}\n" +
                                $"           Target: {_command.Value.Method.DeclaringType?.Namespace}.{_command.Value.Method.Name}\n" +
                                $"           Execute On Main Thread: {_command.Key.ExecOnMainThread}\n" +
                                $"           Supported Args: {_command.Key.SupportedArgs}\n" +
                                $"           Example: {_command.Key.Example}\n";
            }
            
            Debug.Log($"Displaying all {commands.Count} commands available...\n \n" +
                      $"{_printResult.TrimEnd('\n')}");
        }
        
        /// <summary>
        /// Print the version of the application.
        /// </summary>
        [ConsoleCommand("version", "Print the current version", "none", true), Preserve]
        private static void HandleVersionPrintCommand(ArgumentsParser _args) {
            Debug.Log($"RDNTLG ConsoleCommand Utility, running on: {Application.productName} - {Application.version}");
        }

        #endregion
        
    }
    
}
#endif