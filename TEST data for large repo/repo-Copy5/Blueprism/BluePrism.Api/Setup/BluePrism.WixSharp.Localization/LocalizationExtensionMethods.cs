namespace BluePrism.WixSharp.Localization
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using global::WixSharp;
    using global::WixSharp.CommonTasks;
    using Microsoft.Deployment.WindowsInstaller;
    using Assembly = System.Reflection.Assembly;

    public static class LocalizationExtensionMethods
    {
        public static ManagedProject UseWinFormsLocalization(this ManagedProject project)
        {
            AddResourceBinariesToProject(project);
            project.UIInitialized += e => AppDomain.CurrentDomain.AssemblyResolve += GetAssemblyResolveEvent(e.Session);

            return project;
        }

        private static void AddResourceBinariesToProject(ManagedProject project)
        {
            var resourceBuildDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);

            if(resourceBuildDirectory == null)
                throw new Exception("Could not get path to entry assembly");

            var resourcesFiles = Directory.GetFiles(resourceBuildDirectory, "*.resources.dll", SearchOption.AllDirectories);

            var resourceBinaries = resourcesFiles.Select(x => new Binary(GetIdForResourceFile(x), x));

            project.AddBinaries(resourceBinaries.ToArray());
        }

        private static Id GetIdForResourceFile(string resourceFilePath) =>
            new Id(
                $"{Path.GetFileNameWithoutExtension(resourceFilePath)?.ToLowerInvariant()}_{Path.GetFileName(Path.GetDirectoryName(resourceFilePath))?.ToLowerInvariant()}".Replace('-', '_'));


        private static readonly Regex AssemblyDetailsRegex = new Regex("(?<n>[^,]+),.*Culture=(?<c>[^,]+)", RegexOptions.Compiled);
        private static ResolveEventHandler GetAssemblyResolveEvent(Session session) =>
            (sender, args) =>
            {
                var details = AssemblyDetailsRegex.Match(args.Name);

                if (!details.Groups["n"].Success || !details.Groups["c"].Success)
                {
                    session.Log("Unable to find resource binary for {0}", args.Name);
                    return null;
                }

                var name = details.Groups["n"].Value;
                var culture = details.Groups["c"].Value;

                session.Log("Resolving resource binary {0}, {1}", name, culture);

                var result = GetResourceAssemblyFromBinaries(name, culture, session);

                if(result == null)
                    session.Log("Unable to find resource binary");
                else
                    session.Log("Resource binary found");

                return result;
            };

        private static Assembly GetResourceAssemblyFromBinaries(string name, string culture, Session session)
        {
            var binaryName = $"{name.ToLowerInvariant()}_{culture.ToLowerInvariant().Replace('-', '_')}";

            session.Log("Searching for binary {0}", binaryName);

            using (var view =
                session.Database.OpenView($"SELECT `Data` FROM `Binary` WHERE `Name` = '{binaryName}'"))
            {
                view.Execute();
                using (var item = view.Fetch())
                {
                    if (item == null) return null;

                    var dataStream = item.GetStream("Data");
                    using (var assemblyData = new MemoryStream())
                    {
                        dataStream.CopyTo(assemblyData);
                        return Assembly.Load(assemblyData.ToArray());
                    }
                }
            }
        }
    }
}
