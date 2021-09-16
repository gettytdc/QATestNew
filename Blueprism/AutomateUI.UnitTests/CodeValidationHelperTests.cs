using System;
using System.Collections.Generic;
using AutomateUI.Controls.Widgets.SystemManager.WebApi.Request;
using BluePrism.AutomateProcessCore.Compilation;
using BluePrism.AutomateProcessCore.WebApis;
using FluentAssertions;
using NUnit.Framework;

namespace AutomateUI.UnitTests
{
    [TestFixture]
    public partial class CodeValidationHelperTests
    {
        public CodeValidationHelperTests()
        {
            _displaySuccess = (x) => _isSuccess = true;
            _displayError = (x) => _isError = true;
        }

        private bool _isSuccess;
        private bool _isError;
        private readonly Action<string> _displaySuccess;
        private readonly Action<string> _displayError;
        private ICodeCompiler _compiler;
        private readonly List<MethodDefinition> _methods = new List<MethodDefinition>();

        [SetUp]
        public void SetUp()
        {
            _isSuccess = false;
            _isError = false;
            _compiler = new CodeCompiler(BluePrism.AutomateProcessCore.Compilation.CodeLanguage.CSharp, TestContext.CurrentContext.TestDirectory);
        }

        [Test]
        public void CheckValidation_ValidCommonCode_DisplaysSuccess()
        {
            var commonCode = CreateCommonCode("int x = 5;");
            CodeValidationHelper.Validate(commonCode, _methods.ToArray(), _displaySuccess, _displayError, _compiler);
            _isSuccess.Should().Be(true);
            _isError.Should().Be(false);
        }

        [Test]
        public void CheckValidation_InvalidCommonCode_DisplaysError()
        {
            var commonCode = CreateCommonCode("this is not code");
            CodeValidationHelper.Validate(commonCode, _methods.ToArray(), _displaySuccess, _displayError, _compiler);
            _isSuccess.Should().Be(false);
            _isError.Should().Be(true);
        }

        [Test]
        public void CheckValidation_ValidCode_DisplaysSuccess()
        {
            var commonCode = CreateCommonCode("");
            var methodDefinition = new MethodDefinition("Method1", "Method1", "var x = 4;", Array.Empty<MethodParameterDefinition>());
            CodeValidationHelper.Validate(commonCode, new[] { methodDefinition }, _displaySuccess, _displayError, _compiler);
            _isSuccess.Should().Be(true);
            _isError.Should().Be(false);
        }

        [Test]
        public void CheckValidation_Invalidode_DisplaysError()
        {
            var commonCode = CreateCommonCode("");
            var methodDefinition = new MethodDefinition("Method1", "Method1", "this is not code", Array.Empty<MethodParameterDefinition>());
            CodeValidationHelper.Validate(commonCode, new[] { methodDefinition }, _displaySuccess, _displayError, _compiler);
            _isSuccess.Should().Be(false);
            _isError.Should().Be(true);
        }

        private CodeProperties CreateCommonCode(string code)
        {
            return new CodeProperties(code, BluePrism.AutomateProcessCore.Compilation.CodeLanguage.CSharp, Array.Empty<string>(), Array.Empty<string>());
        }
    }
}
