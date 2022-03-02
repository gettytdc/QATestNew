namespace BluePrism.UnitTesting
{
    using System;
    using Autofac.Extras.Moq;
    using BPCoreLib.DependencyInjection;
    using Moq;

    public static class LegacyUnitTestHelper
    {
        private static AutoMock _mock;

        public static void SetupDependencyResolver()
        {
            _mock = AutoMock.GetLoose();
            DependencyResolver.SetContainer(_mock.Container);
        }

        public static Mock<T> GetMock<T>() where T : class =>
            _mock.Mock<T>() 
            ?? throw new InvalidOperationException("SetupDependencyResolver must be called before mocks can be retrieved.");

        public static void UnsetDependencyResolver() =>
            DependencyResolver.SetContainer(null);
    }
}