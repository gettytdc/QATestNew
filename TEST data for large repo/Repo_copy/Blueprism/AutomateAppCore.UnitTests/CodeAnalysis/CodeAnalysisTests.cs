using BluePrism.AutomateAppCore;
using Mono.Cecil.Cil;
using NUnit.Framework;
using System.Linq;
using System;

namespace AutomateAppCore.UnitTests.CodeAnalysis
{
    [TestFixture]
    public class CodeAnalysisTests
    {
        [Test]
        public void EnsureNoReferencesTogSvInServer()
        {
            var assembly = Mono.Cecil.AssemblyDefinition.ReadAssembly(AppDomain.CurrentDomain.BaseDirectory + @"..\..\..\bin\AutomateAppCore.dll");

            var serverType = assembly.Modules.SelectMany(x => x.GetTypes()).Single(x => x.Name == nameof(clsServer));

            var gSvMethod = assembly.Modules.SelectMany(x => x.GetTypes()).Single(x => x.Name == nameof(app)).Methods.Single(x => x.Name == "get_gSv");

            var references = serverType.Methods.Select(x => new
            {
                x.Body.Instructions,
                x.Name
            }).Where(x => x.Instructions.Any(y => y.OpCode == OpCodes.Call && y.Operand == gSvMethod)).Select(x => x.Name).ToArray();

            Assert.AreEqual(new string[] { }, references, $"gSv referenced in clsServer in methods: {string.Join(", ", references)}");
        }
    }
}
