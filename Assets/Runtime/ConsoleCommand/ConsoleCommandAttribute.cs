/*************************************************************************\
Code provided under the "Apache Licence 2.0".
For more details, refer to the "LICENCE" file at the root of the project.
Written by RDNTLG (August 2025).
\*************************************************************************/

#if RDNTLG_CCM || UNITY_EDITOR
using System;

namespace RDNTLG.Command {

    /// <summary>
    /// Define a method to execute when receiving a specific
    /// command through the console via the <see cref="ConsoleCommandSystem"/>.
    /// </summary>
    /// <remarks>
    /// <para>Methods that use this attribute must be declared
    /// as follows "<i>static void METHOD_NAME(<see cref="ArgumentParser"/>)</i>".</para>
    ///
    /// <para>Only available under the <c>RDNTLG_CCM</c>/<c>UNITY_EDITOR</c>
    /// preprocessor definitions.</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class ConsoleCommandAttribute : Attribute {

        #region Attributes

        /// <summary>
        /// The name of the command.
        /// </summary>
        public readonly string Command;
        /// <summary>
        /// Arguments this command will try to use.
        /// </summary>
        public readonly string SupportedArgs;
        /// <summary>
        /// Short usage example.
        /// </summary>
        public readonly string Example;
        /// <summary>
        /// Should the method using this attribute
        /// but only called on the main thread?
        /// </summary>
        /// <remarks>
        /// <para>Most of the time (or if you don't know
        /// what this parameter means), leave it to true.</para>
        /// <para>In the editor, methods are ALWAYS
        /// called in the main thread.</para>
        /// </remarks>
        public readonly bool ExecOnMainThread;

        #endregion

        public ConsoleCommandAttribute(string _command, string _supportedArgs = null, string _example = null, bool _execOnMainThread = true) {
            Command = _command;
            SupportedArgs = _supportedArgs;
            Example = _example;
            ExecOnMainThread = _execOnMainThread;
        }

    }
    
}
#endif