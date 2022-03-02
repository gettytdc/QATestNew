using System.Collections.Generic;
using System.IO;
using WixSharp;

namespace BluePrism.Setup
{
    public class ProjectReference
    {
        public string Name { get; }

        public string SourceDir { get; set; }

        public string TargetDir { get; set; }

        public string ProjectPath { get; }

        public bool Binaries { get; set; } = true;

        public bool Content { get; set; } = true;

        public bool Satellites { get; set; } = true;

        public IEnumerable<Feature> Features { get; set; }

        public ProjectReference(string projectPath)
        {
            ProjectPath = projectPath;
            Name = Path.GetFileNameWithoutExtension(projectPath);
        }
    }
}
