using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using LocaleTools;

namespace XMLprocessor
{
    internal static class Program
    {
        private const string bluePrismConfig = @"C:\ProgramData\Blue Prism Limited\Automate V3\Automate.config";

        static readonly Dictionary<string, Dictionary<string, string>> langData =
            new Dictionary<string, Dictionary<string, string>>();

        static List<string> locales = GetLocalesFromFile();

        /**
         * This program iterates through all VBO xml files, extracts any published names and creates new 
         * resource entries for any missing items. It can then create static localized versions of each VBO.
         * 
         * It can also read strings from the database using the database command line flag and process static files.
         * It is closely related to the LocaleTools dll as it populates the resource files which LocaleTools refrences.
         */
        private const string gswLocale = "gsw-LI";

        private static readonly List<string> _files = new List<string>
        {
            "misc", "holiday", "roleperms", "validation", "tile", "vbo", "actions", "elements"
        };

        private static Dictionary<string, string> dbdefs = new Dictionary<string, string>();

        // pre-defined database queries to generate each resource file
        // The format is: <query>^prefix
        // If two columns are selected, the second will be used as the index in the resource file.
        private static void DBQueryInit()
        {
            dbdefs.Add("validation",
                "SELECT description,checkid FROM BPAValCheck^check;SELECT description FROM BPAValType^type;SELECT description FROM BPAValAction^action;SELECT description FROM BPAValCategory^cat");
            dbdefs.Add("tile",
                "SELECT name FROM BPATile;SELECT description FROM BPATile;SELECT distinct sys.parameters.name as name from BPATileDataSources, sys.parameters where object_id = object_id(BPATileDataSources.spname) order by name^param");
            dbdefs.Add("misc",
                "SELECT name from BPAScheduleList;SELECT description from BPAScheduleList;SELECT name FROM BPAUserRole;Select description from BPAStatus^status;SELECT type FROM BPAExceptionType^exception_type");
            dbdefs.Add("holiday",
                "SELECT name FROM BPAPublicHolidayGroup;SELECT name FROM BPAPublicHoliday;SELECT name from BPACalendar^calendar;SELECT description from BPACalendar^calendar");
            dbdefs.Add("roleperms",
               "SELECT name FROM BPAPerm^perm;SELECT name FROM BPAPermGroup^group;SELECT name FROM BPAUserRole^role");
        }
        private static readonly List<string> databaseIds = new List<string> //MUST match DBQueryInit() above
        {
            "validation", "tile", "misc", "holiday", "roleperms"
        };


        // Command line tool which takes no arguments
        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Length == 1 && args[0] == "all")
            {
                try
                {
                    DoAll();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception during processing" +
                        Environment.NewLine + ex.ToString());
                }

                return;
            }

            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "database":
                        DBQueryInit();
                        if (args.Length == 3)
                        {
                            string connectionString = "";
                            string sqlInstance = args[1];
                            string database = args[2];

                            connectionString = GetDatabase(sqlInstance, database, false);
                            if (connectionString != "")
                                foreach (var dbId in databaseIds)
                                {
                                    ReadAllLanguageFiles(dbId);
                                    ProcessDatabase(dbId, connectionString);
                                    ReadTextFileIfExists("manualStrings-" + dbId + ".txt");
                                    WriteAllLanguageFiles(dbId);
                                }
                        }
                        else
                        {
                            Console.WriteLine("Invalid database type, try one of: " +
                                              string.Join(", ", dbdefs.Keys.ToList()));
                        }

                        break;

                    case "textfile":
                        if (args.Length > 1)
                        {
                            var file = "misc";
                            if (args.Length > 3)
                            {
                                file = args[3];
                            }
                            ReadAllLanguageFiles(file);
                            ReadTextFile(args[1], args.Length > 2 ? args[2] : null);
                            WriteAllLanguageFiles(file);
                        }
                        else
                        {
                            Console.WriteLine("ERROR: please specify textfile path!");
                        }

                        break;

                    case "vbo":
                        ReadAllLanguageFiles("vbo");
                        storedStats();
                        processVBOs(@"../VBO");
                        storedStats();
                        WriteAllLanguageFiles("vbo");
                        break;

                    case "localise-vbo":
                        ReadAllLanguageFiles("vbo");
                        foreach (var locale in locales)
                        {
                            processVBOs(@"../VBO", locale);
                        }

                        break;

                    case "actions":
                        ReadAllLanguageFiles("actions");
                        processActionFile(@"../ApplicationManager/AMI/Actions.xml");
                        WriteAllLanguageFiles("actions");
                        break;

                    case "elements":
                        ReadAllLanguageFiles("elements");
                        processElementsFile(@"../ApplicationManager/AMI/SAPElements.xml");
                        WriteAllLanguageFiles("elements");
                        break;

                    case "prep-english":
                        Console.WriteLine("Preparing files for translation - internal only");
                        string workLocale = "ja-JP";
                        string destLocale = "en-US";
                        foreach (var file in _files)
                        {
                            Console.WriteLine("Preparing: " + file);
                            langData[workLocale] = new Dictionary<string, string>();
                            langData[destLocale] = new Dictionary<string, string>();
                            ReadLanguageFile(GetLanguageFileName(file, workLocale), workLocale);
                            RemoveNewValuePlaceholders(workLocale, destLocale);

                            WriteLanguageFile("resource-just-for-translation/" + file + "_" + destLocale + ".resx", destLocale);
                        }
                        break;


                    default:
                        Console.WriteLine("Unknown command: " + args[0]);
                        break;
                }
            }
            else
            {
                string[] usage =
                {
                     @"database SQL_server_instance database_name   eg. database PCname\MSSQLSERVER2017 myDB",
                     "vbo",
                     "actions",
                     "elements",
                     "prep-english"
                };

                Console.WriteLine("");
                Console.WriteLine("!!! this tool must be run in the XMLprocessor folder !!!");
                Console.WriteLine("");
                Console.WriteLine("USAGE: one of below");
                foreach (var str in usage)
                    Console.WriteLine(str);
                Console.WriteLine("!!! the _gsw-LI.resx files should be present as embedded in LocaleTools.csproj !!!");

            }
        }

        // remove the start and end characters added to new values
        private static void RemoveNewValuePlaceholders(string locale, string destLocale)
        {
            foreach (var key in langData[locale].Keys)
            {
                var value = langData[locale][key] ?? "";
                value = Regex.Replace(value, "^始", "");
                value = Regex.Replace(value, "終$", "");
                langData[destLocale][key] = value;
            }
        }

        // read known strings from the database
        private static void ProcessDatabase(string id, string connectionString)
        {
            try
            {
                using (SqlConnection myConnection = new SqlConnection(connectionString))
                {
                    try
                    {
                        myConnection.Open();
                        Console.WriteLine("Reading all database text properties for '{0}'", id);

                        if (dbdefs.TryGetValue(id, out var str))
                        {
                            foreach (var query in str.Split(';'))
                            {
                                GetSqlStrings(myConnection, query.Trim());
                            }
                        }
                        else
                        {
                            Console.WriteLine("Cannot find any database definitions for: " + id);
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void GetSqlStrings(SqlConnection myConnection, string query)
        {
            var queryParams = query.Split('^');

            try
            {
                var myCommand = new SqlCommand(queryParams[0], myConnection);
                var myReader = myCommand.ExecuteReader();
                while (myReader.Read())
                {
                    CheckAndAdd(myReader[0].ToString(), queryParams.Length > 1 ? queryParams[1] : null,
                        myReader.FieldCount > 1 ? myReader[1].ToString() : null);
                }

                myReader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("SQL error: " + e.Message + " on query: " + query);
            }
        }

        // process strings from an optional text file
        private static void ReadTextFileIfExists(string path)
        {
            if (File.Exists(path))
            {
                ReadTextFile(path);
            }
        }

        // process strings from a text file
        private static void ReadTextFile(string path, string prepend = null)
        {
            Console.WriteLine("Processing " + path);
            var text = File.ReadAllText(path);
            foreach (var txt in text.Split('\n'))
            {
                if (txt.Trim().Length > 0)
                {
                    CheckAndAdd(txt.Trim(), prepend);
                }
            }
        }

        // Process VBO files found in the specified path
        // if toLocale is set then write out localized versions
        private static void processVBOs(string path, string toLocale = null)
        {
            var files = new List<string>(Directory.EnumerateFiles(path));

            foreach (var file in files)
            {
                var basename = Basename(file);
                if (basename.EndsWith(".xml"))
                {
                    Console.WriteLine("Processing file: " + basename);
                    ReadVboXml(file, toLocale);
                }
                else
                {
                    Console.WriteLine("Ignoring file: " + basename);
                }
            }
        }


        // Ensure dictionary exists
        private static void NoNullLocale(string locale)
        {
            if (langData.ContainsKey(locale))
                return;
            langData[locale] = new Dictionary<string, string>();
        }


        // Get dictionary key or null if undefined
        static string get(string locale, string key)
        {
            return langData[locale].ContainsKey(key) ? langData[locale][key] : null;
        }


        // Set dictionary key
        static void set(string locale, string key, string value)
        {
            if (locale == null || key == null || value == null)
                return;
            langData[locale][key] = value;
        }


        // Get name of resx file for this language
        private static string GetLanguageFileName(string prefix, string locale)
        {
            return prefix + "." + locale + ".resx";
        }


        // Read in all language resx files
        private static void ReadAllLanguageFiles(string prefix)
        {
            Console.WriteLine("Reading all existing language files for '{0}' ...", prefix);

            foreach (var locale in locales)
            {
                NoNullLocale(locale);
                string fileName = GetLanguageFileName(prefix, locale);
                Console.WriteLine("Reading in: {0}.{1} - file {2}", prefix, locale, fileName);
                ReadLanguageFile(fileName, locale);
            }

            Console.WriteLine("Read in all existing language files for '{0}'", prefix);
        }

        // Write out all language resx files
        private static void WriteAllLanguageFiles(string prefix)
        {
            Console.WriteLine("Writing all existing language files for '{0}' ...", prefix);
            foreach (var locale in locales)
            {
                WriteLanguageFile(GetLanguageFileName(prefix, locale), locale);
            }
            Console.WriteLine("Wrote all existing language files for '{0}'", prefix);
        }


        // Describe the number of keys stored for each language
        static void storedStats()
        {
            foreach (var lang in langData.Keys)
            {
                Console.WriteLine("Language: " + lang + " - Resource Keys: " + langData[lang].Count);
            }
        }


        // Read in a resx file for a locale
        private static void ReadLanguageFile(string file, string locale)
        {
            if (File.Exists(file))
            {
                var allText = File.ReadAllText(file);
                var xml = XElement.Parse(allText);
                var dataItems = xml.Elements("data").ToList();

                foreach (var item in dataItems)
                {
                    var name = item.Attribute("name")?.Value;
                    var value = item.Element("value")?.Value;
                    set(locale, name, value);
                }
            }
            else
            {
                Console.WriteLine("File does not exist for reading: " + file);
            }
        }


        // Read in resx file and Write out resx file appending new entries
        private static void WriteLanguageFile(string file, string locale)
        {
            var resourceEntries = new Hashtable();
            if (!File.Exists(file))
            {
                File.Copy("template.resx", file);
            }

            Console.WriteLine("Writing file {0} - locale {1} ...", file, locale);

            var reader = new ResXResourceReader(file) { UseResXDataNodes = true };
            var resourceWriter = new ResXResourceWriter(file);
            ITypeResolutionService typeResolution = null;

            foreach (DictionaryEntry d in reader)
            {
                if (d.Value == null)
                    resourceEntries.Add(d.Key.ToString(), "");
                else
                {
                    var val = ((ResXDataNode)d.Value).GetValue(typeResolution).ToString();
                    resourceEntries.Add(d.Key.ToString(), val);
                }

                var dataNode = (ResXDataNode)d.Value;
                resourceWriter.AddResource(dataNode);
            }

            reader.Close();

            foreach (var key in langData[locale].Keys)
            {
                if (resourceEntries.ContainsKey(key))
                    continue;
                var value = langData[locale][key] ?? "";
                resourceWriter.AddResource(key, value);
            }

            resourceWriter.Generate();
            resourceWriter.Close();
            Console.WriteLine("Wrote file " + file);
        }

        // Process actions.xml file for AMI
        private static void processActionFile(string file)
        {
            var xmlStr = File.ReadAllText(file);
            var str = XElement.Parse(xmlStr);

            var actions = str.Elements("action");
            foreach (var action in actions)
            {
                var value = action.Element("name")?.Value;
                CheckAndAdd(value, "name");
                value = action.Element("helptext")?.Value;
                CheckAndAdd(value, "helptext");

                var arguments = action.Elements("argument");
                foreach (var argument in arguments)
                {
                    value = argument.Element("name")?.Value;
                    CheckAndAdd(value, "name");
                    value = argument.Element("description")?.Value;
                    CheckAndAdd(value, "description");
                }
            }
        }
        // Process SAPElements.xml file for AMI
        private static void processElementsFile(string file)
        {
            var xmlStr = File.ReadAllText(file);
            var str = XElement.Parse(xmlStr);

            var elements = str.Elements("element");
            foreach (var action in elements)
            {
                var value = action.Element("name")?.Value;
                CheckAndAdd(value, "name");
                value = action.Element("helptext")?.Value;
                CheckAndAdd(value, "helptext");
            }
        }

        // Read in a VBO file and process every published name
        private static void ReadVboXml(string file, string toLocale = null)
        {
            var xmlStr = File.ReadAllText(file);
            var str = XElement.Parse(xmlStr);

            // add filename as a string
            CheckAndAdd(StripSuffix(Basename(file)));

            // handle document name
            var documentName = str.Attribute("name");
            if (documentName != null)
            {
                if (toLocale != null)
                {
                    documentName.SetValue(LTools.Get(documentName.Value, "vbo", toLocale));
                }
                else
                {
                    CheckAndAdd(documentName.Value);
                }
            }

            // handle document narrative
            var documentNarrative = str.Attribute("narrative");
            if (documentNarrative != null)
            {
                if (toLocale != null)
                {
                    documentNarrative.SetValue(LTools.Get(documentNarrative.Value, "vbo", toLocale));
                }
                else
                {
                    CheckAndAdd(documentNarrative.Value);
                }
            }


            // handle published names
            var publishedItems = str.Elements("subsheet").Where(x => x.Attribute("published").Value.Equals("True"))
                .ToList();

            foreach (var item in publishedItems)
            {
                var value = item.Element("name")?.Value;
                if (toLocale != null)
                {
                    item.Element("name")?.SetValue(LTools.Get(value, "vbo", toLocale));
                }
                else
                {
                    CheckAndAdd(value);
                }
            }


            var stages = str.Elements("stage");
            foreach (var stage in stages)
            {
                // handle narratives
                var narratives = stage.Elements("narrative")
                    .ToList();

                foreach (var item in narratives)
                {
                    var narrative = item.Value;
                    if (narrative.Trim().Length <= 0)
                        continue;
                    if (toLocale != null)
                    {
                        item.SetValue(LTools.Get(narrative, "vbo", toLocale));
                    }
                    else
                    {
                        CheckAndAdd(narrative);
                    }
                }
            }

            // if writing out localized resource files
            if (toLocale != null)
            {
                const string folder = @"..\VBO-perLocale";
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                var subFolder = folder + @"\" + toLocale;
                if (!Directory.Exists(subFolder))
                {
                    Directory.CreateDirectory(subFolder);
                }

                var outputFile = Path.Combine(subFolder, $"{LTools.Get(StripSuffix(Basename(file)), "vbo", toLocale)}.xml");
                Console.WriteLine("Writing localized file: " + outputFile);
                var xmlSettings = new XmlWriterSettings { Indent = true };
                var writer = XmlWriter.Create(outputFile, xmlSettings);

                str.WriteTo(writer);
                writer.Flush();
                writer.Close();
            }
        }


        // Check if item exists in locale, if not add it with pseudo data
        private static void CheckAndAdd(string value, string prepend = null, string altKey = null)
        {
            if (value == null)
                return;
            if (value == "")
                return;

            var munge = (prepend != null ? prepend + "_" : "") + (altKey ?? LTools.GetKey(value));


            // for each locale
            foreach (var locale in locales)
            {
                if (get(locale, munge) != null)
                    continue;

                Console.WriteLine("--> " + munge + " <--" + " Missing from: " + locale);
                var pseudo = GetInitialTranslatedValue(locale, value);
                set(locale, munge, pseudo);
            }
        }


        // convert a string to pseudo locale version by appending the japanese start and end characters
        private static string GetInitialTranslatedValue(string locale, string value)
        {
            if (locale == gswLocale)
            {
                return CreatePseudoTranslation(value);
            }
            else
            {
                return "始" + value + "終";
            }
        }

        private static string CreatePseudoTranslation(string value)
        {
            return PseudoTextConvertor.Convert(value);
        }
        // get just the filename from a path
        private static string Basename(string value)
        {
            return value.Substring(value.LastIndexOf("\\") + 1);
        }

        // remove the .xyz extension
        private static string StripSuffix(string value)
        {
            return value.Substring(0, value.LastIndexOf(@"."));
        }


        private static void ClearLocales()
        {
            foreach (var locale in locales)
                langData[locale] = new Dictionary<string, string>();
        }

        private static string GetDatabase(string sqlInstance, string database, bool fromConfig)
        {
            try
            {
                if (fromConfig)
                {
                    // reasd from config file first and if not found try 
                    // product config file
                    database = ConfigurationManager.AppSettings["DatabaseName"];

                    if (!string.IsNullOrEmpty(database))
                    {
                        Console.WriteLine("Read database name '{0}' from app config file", database);
                    }
                    else
                    {
                        Console.WriteLine("Extracting Database from product config file ...");

                        var xml = XDocument.Load(bluePrismConfig);

                        var connections = xml.Descendants("connections");
                        var connection = connections.FirstOrDefault();

                        var server = connection.Descendants("server");
                        sqlInstance = server.FirstOrDefault().Value;

                        var db = connection.Descendants("dbname");
                        database = db.FirstOrDefault().Value;

                        Console.WriteLine("Database name is '" + database + "'");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Failed to extract database name" +
                    Environment.NewLine + e.ToString());
                return "";
            }

            string connectionString = "Data Source=" + sqlInstance + "; Initial Catalog=" + database + "; Integrated Security=SSPI;Connection Timeout=5"; //windows authentication

            try
            {
                using (SqlConnection myConnection = new SqlConnection(connectionString))
                {
                    myConnection.Open();
                }

                Console.WriteLine("Verified connection to Database '" + database + "'");
            }
            catch (Exception)
            {
                Console.WriteLine("ERROR: Failed to connect to Database '{0}'", database);
                return "";
            }

            return connectionString;
        }

        private static void DoAll()
        {
            Console.WriteLine("STARTING 'ALL' CONVERSION ...");
            Console.WriteLine(string.Empty);

            DBQueryInit();
            string connectionStringFromConfig = GetDatabase("", "", true);
            if (connectionStringFromConfig != "")
            {
                foreach (var dbId in databaseIds)
                {
                    ClearLocales();
                    Console.WriteLine(string.Empty);
                    ReadAllLanguageFiles(dbId);
                    ProcessDatabase(dbId, connectionStringFromConfig);
                    ReadTextFileIfExists("manualStrings-" + dbId + ".txt");
                    WriteAllLanguageFiles(dbId);
                }
            }

            Console.WriteLine(string.Empty);

            ClearLocales();
            ReadAllLanguageFiles("vbo");
            storedStats();
            processVBOs(@"../VBO");
            storedStats();
            WriteAllLanguageFiles("vbo");

            Console.WriteLine(string.Empty);
            ClearLocales();
            ReadAllLanguageFiles("actions");
            processActionFile(@"../ApplicationManager/AMI/Actions.xml");
            WriteAllLanguageFiles("actions");

            Console.WriteLine(string.Empty);
            ClearLocales();
            ReadAllLanguageFiles("elements");
            processElementsFile(@"../ApplicationManager/AMI/SAPElements.xml");
            WriteAllLanguageFiles("elements");

            Console.WriteLine(string.Empty);

            Console.WriteLine("Preparing files for translation ...");
            string workLocale = "ja-JP";
            string destLocale = "en-US";
            foreach (var file in _files)
            {
                Console.WriteLine("Preparing: '{0}'", file);
                langData[workLocale] = new Dictionary<string, string>();
                langData[destLocale] = new Dictionary<string, string>();
                ReadLanguageFile(GetLanguageFileName(file, workLocale), workLocale);
                RemoveNewValuePlaceholders(workLocale, destLocale);

                WriteLanguageFile("resource-just-for-translation/" + file + "." + destLocale + ".resx", destLocale);
            }

            Console.WriteLine("COMPLETED 'ALL' PROCESSING");
            Console.WriteLine(string.Empty);

        } //DoAll

        private static List<string> GetLocalesFromFile()
        {
            XmlDocument LocaleXML = new XmlDocument();
            LocaleXML.Load(@"locales.xml");
            List<string> localeList = new List<string>();
            foreach (XmlNode node in LocaleXML.FirstChild.ChildNodes)
            {
                localeList.Add(node.InnerText);
            }
            return localeList;
        }
    }
}
