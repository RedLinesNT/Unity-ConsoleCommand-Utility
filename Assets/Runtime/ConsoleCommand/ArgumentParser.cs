/*************************************************************************\
Code provided under the "Apache Licence 2.0".
For more details, refer to the "LICENCE" file at the root of the project.
Written by RDNTLG (August 2025).
\*************************************************************************/

#if RDNTLG_CCM || UNITY_EDITOR
using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace RDNTLG.Command {
    
    /// <summary>
    /// Parse arguments.
    /// </summary>
    /// <remarks>
    /// <para>Creating a new <see cref="ArgumentsParser"/>
    /// without supplying any arguments will use the
    /// "<i>Environment Command Line Args</i>" instead.</para>
    /// <para>Only available under the <c>RDNTLG_CCM</c>/<c>UNITY_EDITOR</c>
    /// preprocessor definitions.</para>
    /// </remarks>
    /// <example>
    /// -param value
    /// -param "value"
    /// /param:"value"
    /// /param=value
    /// -param5 'value'
    /// </example>
    public class ArgumentsParser {
        
        #region Attributes

        private readonly StringDictionary parameters = null;

        #endregion

        #region Properties

        /// <summary>
        /// Retrieve a parameter value if it exists.
        /// </summary>
        /// <param name="_param">The parameter of the wanted value to retrieve.</param>
        public string this[string _param] { get { return parameters[_param]; } }

        #endregion
        
        /// <inheritdoc cref="ArgumentsParser"/>
        public ArgumentsParser(string[] _args = null) {
            if (_args == null) { //If no arguments where specified, use the Environment one
                _args = Environment.GetCommandLineArgs();
            }
            
            parameters = new StringDictionary();
            Regex _splitter = new Regex(@"^-{1,2}|^/|=|:",RegexOptions.IgnoreCase|RegexOptions.Compiled);
            Regex _remover = new Regex(@"^['""]?(.*?)['""]?$",RegexOptions.IgnoreCase|RegexOptions.Compiled);
            
            string _currentParam = null;
            string[] _parts = null;
   
            foreach(string _text in _args){
                _parts = _splitter.Split(_text,3); //Look for new parameters
                
                switch(_parts.Length){
                    case 1: //Found a value
                        if(_currentParam != null) {
                            if(!parameters.ContainsKey(_currentParam)) {
                                _parts[0] = _remover.Replace(_parts[0],"$1");
                                parameters.Add(_currentParam,_parts[0]);
                            }
                            
                            _currentParam = null;
                        }
                        break; //Else, skipped
                    
                    case 2: //Found just a parameter
                        if(_currentParam != null) { //The last parameter is still waiting. With no value, set it to true.
                            if(!parameters.ContainsKey(_currentParam)) parameters.Add(_currentParam, string.Empty);
                        }
                        _currentParam = _parts[1];
                        break;
                    
                    case 3: //Parameter with enclosed value
                        if(_currentParam != null) { //The last parameter is still waiting. With no value, set it to true.
                            if(!parameters.ContainsKey(_currentParam)) parameters.Add(_currentParam, string.Empty);
                        }
                        
                        _currentParam = _parts[1];
                        
                        if(!parameters.ContainsKey(_currentParam)) { //Remove possible enclosing characters (",')
                            _parts[2] = _remover.Replace(_parts[2], "$1");
                            parameters.Add(_currentParam,_parts[2]);
                        }
                        
                        _currentParam=null;
                        break;
                }
            }
            
            if(_currentParam != null) { //For when a parameter is still waiting
                if(!parameters.ContainsKey(_currentParam)) parameters.Add(_currentParam, string.Empty);
            }
        }
        
    }
    
}
#endif