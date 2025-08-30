/*************************************************************************\
Code provided under the "Apache Licence 2.0".
For more details, refer to the "LICENCE" file at the root of the project.
Written by RDNTLG (August 2025).
\*************************************************************************/

#if RDNTLG_CCM || UNITY_EDITOR
using UnityEngine;
using UnityEngine.Scripting;

namespace RDNTLG.Command {

    /// <summary>
    /// Sample commands using the <see cref="ConsoleCommandAttribute"/>.
    /// </summary>
    public class Sample {

        [ConsoleCommand("hello_what", "name", "hello_what -name 'world'", true), Preserve]
        private static void HandleHelloWorldCommand(ArgumentsParser _args) {
            if (_args["name"] is not null or "") {
                Debug.Log($"Hello {_args["name"]}!");
            } else {
                Debug.Log("Hello World!");
            }
        }
        
    }
    
}
#endif