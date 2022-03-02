using System;
using System.Linq;
using BluePrism.ApplicationManager.CommandHandling;
using NUnit.Framework;

namespace ClientComms.UnitTests.ClientComms.UnitTests.CommandHandling
{

    /// <summary>
    /// Tests to ensure that handler code complies with any constraints
    /// </summary>
    public class HandlerCodeTests
    {
        [Test]
        public void HandlerImplementations_ShouldHaveUniqueCommandIds()
        {

            // Implicitly tests for unique command ids by instantiating HandlerFactory
            var factory = ApplicationFactoryInitialiser.Initialise();
        }

        [Test]
        public void HandlerImplementations_ShouldHaveSingleConstructor()
        {
            var handlers = ApplicationFactoryInitialiser.GetHandlers();
            var invalidHandlerDescriptions = (from handler in handlers
                                              let handlerType = handler.HandlerType
                                              let constructorCount = handlerType.GetConstructors().Length
                                              where constructorCount != 1
                                              select $"{handlerType} ({constructorCount} constructors)").ToList();
            if (invalidHandlerDescriptions.Any())
            {
                string message = @"Handler types were found that have zero or more than one public constructor:
" + string.Join(Environment.NewLine, invalidHandlerDescriptions);
                Assert.Fail(message);
            }
        }
    }
}