
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Blueprism.APIVersionTool
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length < 2)
            {
                Console.WriteLine("Please supply path to AutomateAppCore.dll, and target path.");
                return;
            }

            var location = args[0];

            if(!File.Exists(location))
            {
                Console.WriteLine($"{location} Was not found!");
                return;
            }

            var targetPath = args[1];

            targetPath = targetPath.TrimEnd('\\');

            if (!Directory.Exists(targetPath))
            {
                Console.WriteLine($"ERROR: Directory '{targetPath}' was not found!");
                return;
            }

            var assem = System.Reflection.Assembly.LoadFrom(location);

            var t = assem.GetType("BluePrism.AutomateAppCore.IServer");
            if (t == null)
            {
                Console.WriteLine("IServer not found!");
                return;
            }
            var version = assem.GetName().Version;
            var i = new InterfaceDescriptionGenerator();
            var api = i.GetDescription(t);

            File.WriteAllText(Path.Combine(targetPath, "APIDescription.txt"), api);

            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(api);
                var apiVersion = $"{version}-a{Convert.ToBase64String(sha.ComputeHash(bytes))}";
                File.WriteAllText(Path.Combine(targetPath, "APIVersion.txt"), apiVersion);
            }
        }
    }
}
