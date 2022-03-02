#if UNITTESTS
using System;
using System.Linq;
using System.Reflection;
using BluePrism.AutomateProcessCore;
using BluePrism.AutomateProcessCore.Compilation;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AutomateProcessCore.UnitTests.Compilation
{
    public class CodeCompilerTests
        {
            private static readonly AssemblyDefinition SimpleCSharpAssemblyDefinition;

            static CodeCompilerTests() => SimpleCSharpAssemblyDefinition = CreateSimpleCSharpAssemblyDefinition();

            [Test]
            public void Compile_WithEmptyAssembly_ReturnsEmptyResult()
            {
                var content = new AssemblyDefinition("Code", CodeLanguage.CSharp, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<ClassDefinition>());
                var compiler = CreateCompiler();
                var result = compiler.Compile(content, null);
                result.Should().Be(CompiledCodeResult.Empty);
            }

            [Test]
            public void Compile_WithSimpleAssembly_CreatesSourceCode()
            {
                var result = CompileSimpleCSharpAssembly();
                // Golden master style test to avoid regression - it may be necessary to capture
                // new source code value in debugger and update the constant if the code structure
                // or formatting changes
                result.SourceCode.Should().Be(SimpleCSharpAssemblySourceCode);
            }

            [Test]
            public void Compile_WithIdenticalAssemblyDefinition_CreatesSameSourceCode()
            {
                var result1 = CompileSimpleCSharpAssembly();
                var result2 = CompileSimpleCSharpAssembly();
                result1.SourceCode.Should().Be(result2.SourceCode);
            }

            [Test]
            public void Compile_WithSimpleAssembly_CreatesClasses()
            {
                var assembly = GetCompiledSimpleAssembly();
                var expectedTypes = SimpleCSharpAssemblyDefinition.Classes.Select(c => c.Identifier);
                var types = assembly.GetExportedTypes().Select(t => t.Name);
                types.Should().Equal(expectedTypes);
            }

            [Test]
            public void Compile_WhenSuccessful_CachesResult()
            {
                var cacheMock = new Mock<IObjectCache>();
                var compiler = CreateCompiler();
                var result = compiler.Compile(SimpleCSharpAssemblyDefinition, cacheMock.Object);
                cacheMock.Verify(c => c.Add(It.IsAny<string>(), result));
            }

            [Test]
            public void Compile_WhenFailed_DoesNotCacheResult()
            {
                var cacheMock = new Mock<IObjectCache>();
                var compiler = CreateCompiler();
                var assemblyDefinition = CreateCSharpAssemblyDefinitionWithErrors();
                var result = compiler.Compile(assemblyDefinition, cacheMock.Object);
                cacheMock.Verify(c => c.Add(It.IsAny<string>(), It.IsAny<object>()), Times.Never());
            }

            [Test]
            public void Compile_WhenFailed_IncludesErrorsInResult()
            {
                var compiler = CreateCompiler();
                var assemblyDefinition = CreateCSharpAssemblyDefinitionWithErrors();
                var result = compiler.Compile(assemblyDefinition, null);
                result.Assembly.Should().BeNull();
                result.Errors.HasErrors.Should().BeTrue();
            }

            [Test]
            public void Compile_ChecksCacheBasedOnSourceCode()
            {
                var cacheMock = new Mock<IObjectCache>();
                var compiler = CreateCompiler();
                var result = compiler.Compile(SimpleCSharpAssemblyDefinition, cacheMock.Object);
                var expectedSourceCode = result.SourceCode;
                cacheMock.Verify(c => c.Get<CompiledCodeResult>(expectedSourceCode));
            }

            [Test]
            public void Compile_WhenCachedResultExists_ReturnsCachedResult()
            {
                var result1 = CompileSimpleCSharpAssembly();
                var cacheMock = new Mock<IObjectCache>();
                cacheMock.Setup(c => c.Get<CompiledCodeResult>(result1.SourceCode)).Returns(result1);
                var compiler2 = CreateCompiler();
                var result2 = compiler2.Compile(SimpleCSharpAssemblyDefinition, cacheMock.Object);
                result2.Should().Be(result1);
            }

            [Test]
            public void Compile_WithSimpleAssembly_CreatesClassesAndMethods()
            {
                var assembly = GetCompiledSimpleAssembly();
                var assemblyDefinition = SimpleCSharpAssemblyDefinition;
                foreach (var classDefinition in assemblyDefinition.Classes)
                {
                    var type = assembly.GetType(classDefinition.Identifier);
                    type.Should().NotBeNull();
                    AssertValidClass(classDefinition, type);
                }
            }

            public void AssertValidClass(ClassDefinition classDefinition, Type type)
            {
                foreach (var methodDefinition in classDefinition.Methods)
                {
                    var method = type.GetMethod(methodDefinition.Identifier);
                    method.Should().NotBeNull();
                    AssertValidMethod(methodDefinition, method);
                }
            }

            private static void AssertValidMethod(MethodDefinition methodDefinition, MethodInfo methodInfo)
            {
                foreach (var parameterDefinition in methodDefinition.Parameters)
                {
                    var parameter = methodInfo.GetParameters().SingleOrDefault(p => (p.Name ?? string.Empty) == (parameterDefinition.Identifier ?? string.Empty));
                    parameter.Should().NotBeNull($"parameter {parameterDefinition.Identifier} should be created");
                    var expectedType = clsProcessDataTypes.GetFrameworkEquivalentFromDataType(parameterDefinition.DataType);
                    if (parameterDefinition.IsOutput)
                    {
                        parameter.ParameterType.IsByRef.Should().BeTrue($"type of parameter {parameter.Name} should be by ref");
                        parameter.ParameterType.GetElementType().Should().Be(expectedType, $"parameter {parameter.Name} should have type {expectedType}");
                        parameter.IsOut.Should().Be(true, $"parameter {parameter.Name} should be out");
                    }
                    else
                    {
                        parameter.ParameterType.Should().Be(expectedType, $"parameter {parameter.Name} should have type {expectedType}");
                        parameter.IsOut.Should().Be(false, $"parameter {parameter.Name} should not be out");
                    }
                }
            }

            private static CodeCompiler CreateCompiler() => new CodeCompiler(CodeLanguage.CSharp, TestContext.CurrentContext.TestDirectory);

            private static CompiledCodeResult CompileSimpleCSharpAssembly()
            {
                var compiler = CreateCompiler();
                var result = compiler.Compile(SimpleCSharpAssemblyDefinition, null);
                result.Should().NotBeNull();
                return result;
            }

            private static Assembly GetCompiledSimpleAssembly()
            {
                var result = CompileSimpleCSharpAssembly();
                result.Errors.Should().BeEmpty();
                result.Assembly.Should().NotBeNull();
                return result.Assembly;
            }

            private static AssemblyDefinition CreateSimpleCSharpAssemblyDefinition()
            {
                var class1 = CreateSimpleCSharpClass("Class 1");
                var class2 = CreateSimpleCSharpClass("Class 2");
                var namespaces = new[] { "System", "System.Data" };
                var references = new[] { "System.dll", "System.Data.dll" };
                var classes = new[] { class1, class2 };
                return new AssemblyDefinition("Code", CodeLanguage.CSharp, namespaces, references, classes);
            }

            private static AssemblyDefinition CreateCSharpAssemblyDefinitionWithErrors()
            {
                var class1 = new ClassDefinition("Class 1", Enumerable.Empty<MethodDefinition>(), "shared.vb", "invalid syntax GetXmlNamespace*(&)");
                var namespaces = new[] { "System", "System.Data" };
                var references = new[] { "System.dll", "System.Data.dll" };
                return new AssemblyDefinition("Code", CodeLanguage.CSharp, namespaces, references, new[] { class1 });
            }

            private static ClassDefinition CreateSimpleCSharpClass(string name)
            {
                var method1 = CreateCSharpMethod($"{name} Method 1");
                var method2 = CreateCSharpMethod($"{name} Method 2");
                return new ClassDefinition(name, new[] { method1, method2 }, string.Empty, string.Empty);
            }

            private static MethodDefinition CreateCSharpMethod(string name)
            {
                var parameters = new[] { new MethodParameterDefinition($"{name} Input Param 1", DataType.text, false), new MethodParameterDefinition($"{name} Input Param 2", DataType.flag, false), new MethodParameterDefinition($"{name} Output Param 1", DataType.text, true), new MethodParameterDefinition($"{name} Output Param 2", DataType.text, true) };
                var identifier = CodeCompiler.GetIdentifier(name);
                var code = $@"{identifier}_Output_Param_1 = ""Hello from {name}: "" + {identifier}_Input_Param_1;
{identifier}_Output_Param_2 = ""Hello from {name}: "" + {identifier}_Input_Param_2.ToString();";
                return new MethodDefinition(name, "myfile", code, parameters);
            }

            private const string SimpleCSharpAssemblySourceCode = @"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Data;



public class Class_1 {
    
    
    #line 1 ""myfile""
    public void Class_1_Method_1(string Class_1_Method_1_Input_Param_1, bool Class_1_Method_1_Input_Param_2, out string Class_1_Method_1_Output_Param_1, out string Class_1_Method_1_Output_Param_2) {
Class_1_Method_1_Output_Param_1 = ""Hello from Class 1 Method 1: "" + Class_1_Method_1_Input_Param_1;
Class_1_Method_1_Output_Param_2 = ""Hello from Class 1 Method 1: "" + Class_1_Method_1_Input_Param_2.ToString();
    }
    
    #line default
    #line hidden
    
    
    #line 1 ""myfile""
    public void Class_1_Method_2(string Class_1_Method_2_Input_Param_1, bool Class_1_Method_2_Input_Param_2, out string Class_1_Method_2_Output_Param_1, out string Class_1_Method_2_Output_Param_2) {
Class_1_Method_2_Output_Param_1 = ""Hello from Class 1 Method 2: "" + Class_1_Method_2_Input_Param_1;
Class_1_Method_2_Output_Param_2 = ""Hello from Class 1 Method 2: "" + Class_1_Method_2_Input_Param_2.ToString();
    }
    
    #line default
    #line hidden
}

public class Class_2 {
    
    
    #line 1 ""myfile""
    public void Class_2_Method_1(string Class_2_Method_1_Input_Param_1, bool Class_2_Method_1_Input_Param_2, out string Class_2_Method_1_Output_Param_1, out string Class_2_Method_1_Output_Param_2) {
Class_2_Method_1_Output_Param_1 = ""Hello from Class 2 Method 1: "" + Class_2_Method_1_Input_Param_1;
Class_2_Method_1_Output_Param_2 = ""Hello from Class 2 Method 1: "" + Class_2_Method_1_Input_Param_2.ToString();
    }
    
    #line default
    #line hidden
    
    
    #line 1 ""myfile""
    public void Class_2_Method_2(string Class_2_Method_2_Input_Param_1, bool Class_2_Method_2_Input_Param_2, out string Class_2_Method_2_Output_Param_1, out string Class_2_Method_2_Output_Param_2) {
Class_2_Method_2_Output_Param_1 = ""Hello from Class 2 Method 2: "" + Class_2_Method_2_Input_Param_1;
Class_2_Method_2_Output_Param_2 = ""Hello from Class 2 Method 2: "" + Class_2_Method_2_Input_Param_2.ToString();
    }
    
    #line default
    #line hidden
}
";
    }
}
#endif
