Imports BluePrism.AutomateAppCore.Resources

Public interface IResourceRunnerFactory
    Function Create(options As IResourceRunnerStartUpOptions) As ResourceRunnerComponents
end interface