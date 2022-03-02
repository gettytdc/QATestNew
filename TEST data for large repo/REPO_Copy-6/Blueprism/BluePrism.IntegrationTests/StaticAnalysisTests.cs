#if UNITTESTS
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace BluePrism.IntegrationTests
{
    public class StaticAnalysisTests
    {
        private List<string> _excludedPaths = new List<string>() { "BluePrism.Api", "qa", @"AutomateControls\AutomateControls\APIs" };
        private readonly string _autoGenerateString = "<AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>";
        private readonly string _dependantAssemblyString = "<dependentAssembly>";

        [Test]
        public void  AllIncludedProjectsMustHaveAutoGenerateBindingsRedirect()
        {
            var path = Path.GetFullPath( Assembly.GetExecutingAssembly().Location);
            path = path.Substring(0, path.LastIndexOf(@"\"));
            path = path.Substring(0, path.LastIndexOf(@"\"));
            path = path.Substring(0, path.LastIndexOf(@"\"));
            path = path.Substring(0, path.LastIndexOf(@"\"));

            _excludedPaths = _excludedPaths.Select(s => Path.Combine(path, s)).ToList();
            var errorPaths = new List<string>();
            WalkDirectoryTree(new DirectoryInfo( path), ref errorPaths, "*.csproj",_autoGenerateString, true);

            Assert.IsEmpty(errorPaths);
        }

        [Test]
        public void  AllIncludedProjectsAppConfigShouldNotHaveDependentAssemblies()
        {
            var path = Path.GetFullPath( Assembly.GetExecutingAssembly().Location);
            path = path.Substring(0, path.LastIndexOf(@"\"));
            path = path.Substring(0, path.LastIndexOf(@"\"));
            path = path.Substring(0, path.LastIndexOf(@"\"));
            path = path.Substring(0, path.LastIndexOf(@"\"));

            _excludedPaths = _excludedPaths.Select(s => Path.Combine(path, s)).ToList();
            var errorPaths = new List<string>();
            WalkDirectoryTree(new DirectoryInfo( path), ref errorPaths, "app.config",_dependantAssemblyString, false);

            Assert.IsEmpty(errorPaths);
        }

        private void WalkDirectoryTree(DirectoryInfo root, ref List<string> errorPaths, string filename, string searchVerb, bool shouldInclude)
        {            
            if (_excludedPaths.Contains(root.FullName))
            {
                return;
            }

            var files = root.GetFiles(filename);
            foreach (var file in files)
            {
                var contents = string.Empty;
                using (var sr = new StreamReader(file.FullName))
                {
                    contents = sr.ReadToEnd();
                }
                if (shouldInclude)
                {
                    if (!contents.Contains(searchVerb))
                    {
                        errorPaths.Add(file.FullName);
                    }
                }
                else
                {
                    if (contents.Contains(searchVerb))
                    {
                        errorPaths.Add(file.FullName);
                    }
                }

            }
            var subDirs = root.GetDirectories();

            foreach (var dirInfo in subDirs)
            {
                WalkDirectoryTree(dirInfo, ref errorPaths, filename, searchVerb, shouldInclude);
            }

        }
    }
}
#endif
