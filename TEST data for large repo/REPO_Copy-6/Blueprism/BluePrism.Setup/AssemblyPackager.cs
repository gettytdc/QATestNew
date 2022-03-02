using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace BluePrism.Setup
{
    public static class Extensions
    {
        public static void WriteTo(this BinaryReader reader, Stream stream)
        {
            int count;
            var buffer = new byte[4096];
            while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                stream.Write(buffer, 0, count);
        }
    }

    class AssemblyPackager
    {
        private const string resourceBase = "Satellite.Packages";
        private const string assemblyName = "Satellite.resources";
        private const string assemblyFile = "Satellite.resources.dll";
        private static readonly string[] locales = { "ja-JP", "zh-Hans", "gsw-LI", "fr-FR", "de-DE", "es-419" };
        private const string resourceFileName = "BluePrism.Setup.resources.dll";
        private readonly string _path;

#if DEBUG
        private const string buildLocation = "Debug";
#else
        private const string buildLocation = "Release";
#endif

        private AssemblyPackager(string outDir)
        {
            _path = Path.Combine(outDir, assemblyFile);
            CreateAssembly(outDir);
        }

        public void Bind(WixSharp.ManagedProject project)
        {
            project.DefaultRefAssemblies.Add(_path);
            project.UIInitialized += Unpackage;
        }

        private static void Unpackage(WixSharp.SetupEventArgs e)
        {
            var current = Path.GetDirectoryName(typeof(AssemblyPackager).Assembly.Location);
            var assemblyPath = Path.Combine(current, assemblyFile);
            var assembly = Assembly.ReflectionOnlyLoadFrom(assemblyPath);
            foreach (var locale in locales)
            {
                var localePath = Path.Combine(current, locale);
                Directory.CreateDirectory(localePath);
                var resourceName = $"{resourceBase}.{locale}";
                var fileName = Path.Combine(localePath, resourceFileName);

                using (var reader = new BinaryReader(assembly.GetManifestResourceStream(resourceName)))
                using (var file = new FileStream(fileName, FileMode.Create))
                    reader.WriteTo(file);
            }
        }

        private static AssemblyPackager _packager;

        public static AssemblyPackager Instance(string outDir)
        {
            if (_packager == null)
                _packager = new AssemblyPackager(outDir);

            return _packager;
        }

        private void CreateAssembly(string outDir)
        {
            var asmName = new AssemblyName(assemblyName);

            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                asmName, AssemblyBuilderAccess.Save, outDir);

            var moduleBuilder = assemblyBuilder.DefineDynamicModule(
                assemblyFile, assemblyFile);

            var files = new List<FileStream>();
            try
            {
                foreach (var locale in locales)
                {
                    var sourcePath = Path.Combine("bin", buildLocation, locale, resourceFileName);
                    var resourceName = $"{resourceBase}.{locale}";
                    var file = new FileStream(sourcePath, FileMode.Open);
                    moduleBuilder.DefineManifestResource(
                        resourceName, file, ResourceAttributes.Public);
                    files.Add(file);
                }

                File.Delete(_path);
                assemblyBuilder.Save(assemblyFile);
            }
            finally
            {
                files?.ForEach(f => f.Dispose());
            }
        }
    }
}
