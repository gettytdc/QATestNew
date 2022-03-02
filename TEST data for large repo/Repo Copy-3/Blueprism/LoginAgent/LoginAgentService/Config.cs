using System;
using System.Configuration;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;

namespace LoginAgentService
{
    /// <summary>
    /// Class to encapsulate the configuration of the LoginAgent service
    /// </summary>
    public class Config
    {
        #region - Class scope declarations -

        /// <summary>
        /// Basic "name:[values]" wrapping class which encapsulates a command line
        /// argument for the automate.exe program
        /// </summary>
        public class Argument
        {
            // The name of this argument
            private string _name;

            // The collection of values associated with this argument.
            private IList<string> _values;

            /// <summary>
            /// Creates a new argument with the given name
            /// </summary>
            /// <param name="name">The name of the argument to create</param>
            public Argument(string name) : this(name, new string[0]) { }

            /// <summary>
            /// Creates a new argument with the given name and the given arguments.
            /// </summary>
            /// <param name="name">The name of the argument to create</param>
            /// <param name="values">The values to initialise into this argument.
            /// </param>
            public Argument(string name, params string[] values)
            {
                _name = name;
                _values = new List<string>(values);
            }

            /// <summary>
            /// The name of the argument
            /// </summary>
            public string Name { get { return _name; } }

            /// <summary>
            /// The collection of values associated with this argument - never null.
            /// </summary>
            public ICollection<string> Values { get { return _values; } }

            /// <summary>
            /// The first value set in this argument - if setting, this will replace
            /// all values set in this argument with the given value.
            /// </summary>
            public string Value
            {
                get
                {
                    foreach (string value in Values)
                        return value;
                    return null;
                }
                set
                {
                    _values.Clear();
                    _values.Add(value);
                }
            }
        }

        /// <summary>
        /// The default path to the configuration file used by this class.
        /// </summary>
        public static readonly string DefaultConfigFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            @"Blue Prism Limited\Automate V3\LoginAgentService.config");

        /// <summary>
        /// The name of the executable to run within the Blue Prism installation
        /// </summary>
        private const string ExecutableName = "Automate.exe";

        /// <summary>
        /// Working directory default path. Set to "NOTINSTALLED" to indicate that 
        /// the path to Blue Prism Automate installation directory is unknown.
        /// </summary>
        private const string DefaultWorkingDirectory = "NOTINSTALLED";

        /// <summary>
        /// Loads the configuration for the LoginAgent service from the specified
        /// location.
        /// </summary>
        /// <param name="configPath">The path to the config file from where the
        /// LoginAgent service configuration should be drawn.</param>
        /// <returns>A Config object initialised from the XML file found in the
        /// given location. If none was found, or it was found to be corrupt,
        /// a valid XML file is created in that location and an object representing
        /// that config is returned.</returns>
        public static Config Load(string configPath)
        {
            Config c = new Config();

            // Use a try catch mechanism instead of a simple File.Exists()
            // since we could also have corrupt xml
            try
            {
                c.LoadXML(configPath);
            }
            catch
            {
                c._startupArgs = new List<Argument>();
                c._startupArgs.Add(new Argument("resourcepc"));
                c._startupArgs.Add(new Argument("public"));
                c._startupArgs.Add(new Argument("port", "8181"));
                c.SaveXML(configPath);
            }

            return c;
        }

        #endregion

        #region - Member Variables -

        // The path to the file that this object was loaded from.
        private string _cfgFile;

        // The args to execute the resource PC with
        private ICollection<Argument> _startupArgs;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new, empty config object which points to the default working
        /// directory.
        /// </summary>
        public Config()
        {
            _startupArgs = new List<Argument>();
            WorkingDirectory = DefaultWorkingDirectory;
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// The port on which the resource machine maintained by the LoginAgent
        /// service should be run. This can be set in the config, otherwise a default
        /// value of 8181 is used.
        /// </summary>
        public int ResourcePort
        {
            get
            {
                Argument arg = GetArg("port");
                if (arg == null)
                    return 8181;
                return int.Parse(arg.Value ?? "8181");
            }
        }

        /// <summary>
        /// Whether the resource machine is using ssl.
        /// </summary>
        public bool UseSsl
        {
            get
            {
                var arg = GetArg("sslcert");
                return (arg != null && !string.IsNullOrWhiteSpace(arg.Value));
            }
        }

        /// <summary>
        /// The Blue Prism working directory - ie. the directory in which the
        /// automate.exe program to be used resides
        /// </summary>
        public string WorkingDirectory { get; private set; }

        /// <summary>
        /// The path to the executable to run within the login agent, as configured
        /// in this object.
        /// ie. This returns the full path and executable name - the combination of
        /// the working directory and program name.
        /// </summary>
        public string ExecutablePath
        {
            get { return Path.Combine(WorkingDirectory, ExecutableName); }
        }

        /// <summary>
        /// The arguments to pass to automate.exe when starting the resource PC
        /// maintained by the LoginAgent service
        /// </summary>
        public ICollection<Argument> StartupArguments { get { return _startupArgs; } }

        /// <summary>
        /// All the command line arguments defined in this config expanded into their
        /// string parts.
        /// </summary>
        public ICollection<string> ExpandedCommandLineArguments
        {
            get
            {
                List<string> elems = new List<string>();
                foreach (Config.Argument arg in StartupArguments)
                {
                    elems.Add("/" + arg.Name);
                    foreach (string v in arg.Values)
                        elems.Add(v);
                }
                return elems;
            }
        }

        /// <summary>
        /// Gets the command line escaped into a single string.
        /// </summary>
        public string EscapedCommandLineArguments
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                bool first = true;
                foreach (string elem in ExpandedCommandLineArguments)
                {
                    // Prepend the next element with a space unless it's the first
                    // element
                    if (first)
                        first = false;
                    else
                        sb.Append(' ');

                    // Any spaces? We need to quote the value if so.
                    if (elem.IndexOf(' ') >= 0)
                    {
                        // Wrap the arg in quotes, ensuring any quotes already in the
                        // arg are escaped first.
                        sb.Append('"').Append(elem.Replace("\"", "\"\"")).Append('"');
                    }
                    else
                    {
                        sb.Append(elem);
                    }
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// A new process start info object describing the executable configured in
        /// this object.
        /// </summary>
        /// <returns>A <see cref="ProcessStartInfo"/> object initialised with the
        /// working directory, executable path and arguments from this config object.
        /// The instance is unique to each call - ie. each call to the StartInfo
        /// property creates a new ProcessStartInfo object.
        /// </returns>
        public ProcessStartInfo StartInfo
        {
            get
            {
                ProcessStartInfo info = new ProcessStartInfo(ExecutablePath);
                info.Arguments = EscapedCommandLineArguments;
                info.WorkingDirectory = WorkingDirectory;
                return info;
            }
        }

        #endregion

        #region - Methods -

        /// <summary>
        /// Gets the argument set in this config object with the given name, or null
        /// if no such argument was found.
        /// </summary>
        /// <param name="name">The name of the required argument</param>
        /// <returns>The argument in this config associated with the given name.
        /// </returns>
        public Argument GetArg(string name)
        {
            foreach (Argument arg in StartupArguments)
            {
                if (arg.Name == name)
                    return arg;
            }
            return null;
        }

        /// <summary>
        /// Saves the config file to the location it was loaded from or last saved
        /// to, whichever was done most recently
        /// </summary>
        public void Save()
        {
            SaveXML(_cfgFile);
        }

        /// <summary>
        /// Validate the configuration.
        /// </summary>
        public void Validate()
        {
            if (WorkingDirectory == null ||
                WorkingDirectory.Equals(DefaultWorkingDirectory,
                                        StringComparison.OrdinalIgnoreCase))
            {
                throw new ConfigurationErrorsException(
                    "Configuration Error: WorkingDirectory has not been set to the Blue Prism installation directory");
            }
        }

        /// <summary>
        /// Loads the configuration data found at the given path into this object.
        /// </summary>
        /// <param name="configPath">The path to the file at which the config
        /// data should be loaded from</param>
        /// <exception cref="Exception">If no file was found, or the data within the
        /// file was found to be invalid.</exception>
        private void LoadXML(string configPath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(configPath);
            _cfgFile = configPath;
            LoadXML(doc);
        }

        /// <summary>
        /// Loads the configuration data found in the given XML document into this
        /// object.
        /// </summary>
        /// <param name="doc">The XML document which the config data should be loaded
        /// from</param>
        /// <exception cref="Exception">If the data within the file was found to be
        /// invalid.</exception>
        internal void LoadXML(XmlDocument doc)
        {

            /* Example XML
            <config>
              <workingdirectory path="C:\Program Files\Blue Prism Limited\Automate"/>
              <startuparguments>
                <argument name="public"/>
                <argument name="resourcepc"/>
                <argument name="port"><value>8181</value></port>
                  ...
              </startuparguments>
             </config>
            */
            XmlElement xRoot = doc.DocumentElement;
            if (xRoot.Name != "configuration")
                throw new Exception("Invalid root element found: " + xRoot.Name);

            foreach (XmlElement xSetting in xRoot.ChildNodes)
            {
                if (xSetting.Name == "workingdirectory")
                    WorkingDirectory = xSetting.GetAttribute("path");

                if (xSetting.Name == "startuparguments")
                {
                    _startupArgs = new List<Argument>();
                    foreach (XmlElement xArg in xSetting.ChildNodes)
                    {
                        // We're only interested in 'argument' nodes
                        if (xArg.Name != "argument")
                            continue;

                        Argument arg = new Argument(xArg.GetAttribute("name"));

                        foreach (XmlElement xValue in xArg.ChildNodes)
                            arg.Values.Add(xValue.InnerText);

                        _startupArgs.Add(arg);
                    }
                }
            }
        }

        /// <summary>
        /// Saves the configuration data in this Config object to the given path.
        /// </summary>
        /// <param name="configPath">The path to the file to which this config data
        /// should be saved</param>
        /// <exception cref="Exception">If any errors occur while attempting to
        /// write the XML to the given path.</exception>
        private void SaveXML(string configPath)
        {
            using (XmlTextWriter xw = new XmlTextWriter(configPath, Encoding.UTF8))
            {
                xw.Formatting = Formatting.Indented;
                xw.WriteStartDocument();

                xw.WriteStartElement("configuration");
                xw.WriteStartElement("workingdirectory");
                xw.WriteAttributeString("path", WorkingDirectory);
                xw.WriteEndElement(); //workingdirectory

                xw.WriteStartElement("startuparguments");

                foreach (Argument arg in _startupArgs)
                {
                    xw.WriteStartElement("argument");
                    xw.WriteAttributeString("name", arg.Name);
                    foreach (string v in arg.Values)
                    {
                        xw.WriteStartElement("value");
                        xw.WriteValue(v);
                        xw.WriteEndElement(); //value
                    }
                    xw.WriteEndElement(); //argument
                }

                xw.WriteEndElement(); //startuparguments
                xw.WriteEndElement(); //config
            }
            _cfgFile = configPath;
        }

        #endregion
    }
}
