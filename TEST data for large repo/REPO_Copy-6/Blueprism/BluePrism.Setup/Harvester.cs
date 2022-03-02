using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using WixSharp;
using WixSharp.CommonTasks;

namespace BluePrism.Setup
{

    class Harvester
    {
        private static readonly string _heat = Path.Combine(Compiler.WixLocation, "heat.exe");
        private readonly Project _project;
        private readonly string _sourceDir;
        private readonly string _targetDir;
        private readonly IList<ProjectReference> _components;


        public Harvester(Project project, string sourceDir, string targetDir)
        {
            _project = project;
            _sourceDir = sourceDir;
            _targetDir = targetDir;
            _components = new List<ProjectReference>();
            _project.WixSourceGenerated += WixSourceGenerated;
        }

        private void WixSourceGenerated(XDocument document)
        {
            var ns = XNamespace.Get("http://schemas.microsoft.com/wix/2006/wi");
            var defaultFeature = document.Descendants(ns + "Feature")
                .FirstOrDefault(x => x.Attribute("Id")?.Value == _project.DefaultFeature.Id);

            foreach (var project in _components)
            {
                if (project.Features == null)
                {
                    AddComponentGroupRefs(project, defaultFeature);
                }
                else
                {
                    foreach (var projectFeature in project.Features)
                    {
                        var feature = document.Descendants(ns + "Feature")
                            .FirstOrDefault(x => x.Attribute("Id")?.Value == projectFeature.Id);

                        AddComponentGroupRefs(project, feature);
                    }
                }
            }
        }

        private static void AddComponentGroupRefs(ProjectReference project, XElement feature)
        {
            if (project.Binaries)
                feature.AddElement(
                    new XElement("ComponentGroupRef",
                        new XAttribute("Id", $"{project.Name}.Binaries")));
            if (project.Content)
                feature.AddElement(
                    new XElement("ComponentGroupRef",
                        new XAttribute("Id", $"{project.Name}.Content")));
            if (project.Satellites)
                feature.AddElement(
                    new XElement("ComponentGroupRef",
                        new XAttribute("Id", $"{project.Name}.Satellites")));
        }

        public void AddProjects(IEnumerable<ProjectReference> projects)
        {
            foreach (ProjectReference p in projects)
                AddProject(p);
        }

        public void AddProject(ProjectReference project)
        {
            var sourceDir = project.SourceDir ?? _sourceDir;
            var targetDir = project.TargetDir ?? _targetDir;
            if (!System.IO.File.Exists(project.ProjectPath))
                throw new InvalidOperationException($"Project '{project.ProjectPath}' not found.");


            var output = Path.Combine(_project.OutDir, Path.ChangeExtension(Path.GetFileName(project.ProjectPath), "wxs"));
            var projectDir = Path.GetDirectoryName(project.ProjectPath);

            var args = new[] {
                $"project {project.ProjectPath}",
                project.Binaries ? "-pog Binaries" : "",
                project.Content ? "-pog Content" : "",
                project.Satellites ? "-pog Satellites" : "",
                "-ag","-sfrag",
                $"-directoryid {targetDir}",
                "-template fragment",
                $"-platform AnyCPU",
                $"-projectname {project.Name}",
                $"-out {output}" };

            var pi = new ProcessStartInfo
            {
                FileName = _heat,
                Arguments = string.Join(" ", args),
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            using (var p = Process.Start(pi))
            {
                p.WaitForExit();
                if (p.ExitCode != 0)
                    throw new InvalidOperationException(p.StandardOutput.ReadToEnd());
            }

            var xml = XDocument.Load(output);
            foreach (var fragment in xml.Root.Elements())
                _project.AddXml("Wix", fragment.ToString());

            _project.CandleOptions +=
                $" -d\"{project.Name}.TargetDir\"={sourceDir} " + //As intended the TargetDir for candle is the sourceDir
                $" -d\"{project.Name}.ProjectDir\"={projectDir} ";
            _components.Add(project);

            if (!Compiler.PreserveTempFiles)
                System.IO.File.Delete(output);
        }
    }
}
