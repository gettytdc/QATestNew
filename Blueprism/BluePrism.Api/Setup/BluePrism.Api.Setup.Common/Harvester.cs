namespace BluePrism.Api.Setup.Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using WixSharp;
    using File = WixSharp.File;

    public delegate IHarvester CreateHarvesterMethod(string projectName);

    public class Harvester : IHarvester
    {
        private readonly WixEntity[] _innerEntities;
        private readonly WixEntity[] _outerEntities;
        private readonly IDirectoryTools _directoryTools;
        private readonly string _rootDirectory;
        private const string _packagesPath = @"..\packages\";
        private static readonly Regex PackageVersionRegex =
            new Regex(@"(?<v>(?:\d+\.){2}\d+)$", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

        public string ProjectName { get; }

        public static CreateHarvesterMethod GetFactoryMethod(IDirectoryTools directoryTools) => projectName =>
            new Harvester(projectName, new WixEntity[0], new WixEntity[0], directoryTools);

        private Harvester(
            string projectName,
            WixEntity[] innerEntities,
            WixEntity[] outerEntities,
            IDirectoryTools directoryTools)
        {
            _innerEntities = innerEntities;
            _outerEntities = outerEntities;
            _directoryTools = directoryTools;

            ProjectName = projectName;
            _rootDirectory = @"%ProgramFiles%\Blue Prism Limited\Blue Prism API";
        }

        public ManagedProject GetProject()
        {
            var project = new ManagedProject(
                ProjectName,
                _outerEntities.Concat(new[] {new Dir(_rootDirectory, _innerEntities),}).Cast<WixObject>().ToArray());
            return project;
        }

        public IHarvester AddInnerEntity(WixEntity entity) =>
            new Harvester(ProjectName, _innerEntities.Concat(new[] {entity}).ToArray(), _outerEntities,
                _directoryTools);

        public IHarvester AddInnerEntity(WixEntity entity, params Feature[] features)
        {
            entity.Features = features;
            return new Harvester(ProjectName, _innerEntities.Concat(new[] {entity}).ToArray(), _outerEntities,
                _directoryTools);
        }
               

        public IHarvester AddOuterEntity(WixEntity entity) =>
            new Harvester(ProjectName, _innerEntities, _outerEntities.Concat(new[] {entity}).ToArray(),
                _directoryTools);

        public IHarvester AddOuterEntity(IList<Property> properties) =>
            new Harvester(ProjectName, _innerEntities, _outerEntities.Concat(properties).ToArray(), _directoryTools);

        public IHarvester AddOuterEntity(WixEntity entity, params Feature[] features)
        {
            entity.Features = features;
            return new Harvester(ProjectName, _innerEntities, _outerEntities.Concat(new[] {entity}).ToArray(),
                _directoryTools);
        }

        public IHarvester AddDirectory(string path, string name) =>
            AddInnerEntity(GetDirFromPath(path, name, new WixEntity[0]));

        public IHarvester AddDirectory(string path, string name, params Feature[] features) =>
            AddInnerEntity(GetDirFromPath(path, name, new WixEntity[0]), features);

        public IHarvester AddFilesFromPackageDirectory(string packageName, string sourceDirectory)
        {
            var packageDir = GetContentDirectoriesForPackage(packageName).First();

            if (packageDir.IsNullOrEmpty())
                throw new ArgumentException($"Directory: {sourceDirectory} does not exist in package: {packageName}");

            return AddFilesFromDirectory(Path.Combine(packageDir, sourceDirectory));
        }

        public IHarvester AddFilesFromDirectory(string directory)
        {
            var harvestedDirectory = Harvest(directory);

            return new Harvester(
                ProjectName,
                _innerEntities
                    .Concat(harvestedDirectory.Files)
                    .Concat(harvestedDirectory.Dirs)
                    .ToArray(),
                _outerEntities,
                _directoryTools);
        }

        public IHarvester AddContentFromPackage(string packageName)
        {
            var contentDirectories = GetContentDirectoriesForPackage(packageName);

            if (contentDirectories.Length == 0)
                return this;

            IHarvester AddFilesFromDirectory(IHarvester configurator, string directory) =>
                configurator.AddFilesFromDirectory(directory);

            IHarvester harvester = this;

            return contentDirectories.Aggregate(
                harvester,
                AddFilesFromDirectory);
        }

        public IHarvester AddContentFromPackage(string packageName, string directoryName)
        {
            var contentDirectories = GetContentDirectoriesForPackage(packageName);

            if (contentDirectories.Length == 0)
                return this;

            IHarvester harvester = this;

            return contentDirectories.Aggregate(
                harvester,
                (current, directory) =>
                    current.AddDirectory(directory, directoryName));
        }

        public IHarvester AddContentFromPackage(string packageName, string directoryName, params Feature[] features)
        {
            var contentDirectories = GetContentDirectoriesForPackage(packageName);

            if (contentDirectories.Length == 0)
                return this;

            IHarvester harvester = this;

            return contentDirectories.Aggregate(
                harvester,
                (current, directory) =>
                    current.AddDirectory(directory, directoryName, features));
        }

        public IHarvester AddContentFromPackagesToSingleDirectory(IEnumerable<string> packageNames,
            string directoryName, params Feature[] features)
        {
            var contentDirectories =
                packageNames.Select(GetContentDirectoriesForPackage).Where(x => x != null).SelectMany(x => x);

            Dir ConcatDir(Dir left, Dir right) =>
                new Dir(left.Name, RemoveRootDirectory(left).Concat(RemoveRootDirectory(right)).ToArray());

            var directories = contentDirectories.Select(x => GetDirFromPath(x, "_temp", new WixEntity[0]));

            return AddInnerEntity(directories.Aggregate(new Dir(directoryName), ConcatDir), features);
        }

        private string GetMaxPackageVersion(string packageName)
        {
            var maxVersion =
                Directory.GetDirectories(_packagesPath, $"{packageName}.*")
                    .Select(x => PackageVersionRegex.Match(x).Value)
                    .Select(Version.Parse)
                    .Max(x => x)
                    ?.ToString();

            if (maxVersion.IsNullOrEmpty())
                throw new ArgumentNullException($"Coudn't locate max version of: {packageName}");


            return maxVersion;
        }

        private string[] GetContentDirectoriesForPackage(string packageName)
        {
            var maxVersion =
                Directory.GetDirectories(_packagesPath, $"{packageName}.*")
                    .Select(x => PackageVersionRegex.Match(x).Value)
                    .Select(Version.Parse)
                    .OrderByDescending(x => x)
                    .FirstOrDefault()
                    ?.ToString();

            return maxVersion == null
                ? Array.Empty<string>()
                : Directory.GetDirectories($@"{_packagesPath}{packageName}.{maxVersion}\contents\");
        }

        private Dir GetDirFromPath(string path, string name, IEnumerable<WixEntity> innerEntities) =>
            new Dir(
                name,
                RemoveRootDirectory(Harvest(path))
                    .Concat(innerEntities)
                    .ToArray());

        private static string GetDeepestDirectoryName(string path) =>
            path.Split(
                    new[] {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar},
                    StringSplitOptions.RemoveEmptyEntries)
                .Last();

        private static IEnumerable<WixEntity> RemoveRootDirectory(Dir directory) =>
            directory.Dirs.Cast<WixEntity>()
                .Concat(directory.Files);

        private Dir Harvest(string path) => HarvestDirectory(path);

        private Dir HarvestDirectory(string directoryPath) =>
            new Dir(GetDeepestDirectoryName(directoryPath),
                _directoryTools.GetDirectories(directoryPath).Select(HarvestDirectory).Cast<WixEntity>()
                    .Concat(_directoryTools.GetFiles(directoryPath).Select(HarvestFile))
                    .ToArray());

        private static File HarvestFile(string filePath) =>
            new File(filePath);

    }
}
