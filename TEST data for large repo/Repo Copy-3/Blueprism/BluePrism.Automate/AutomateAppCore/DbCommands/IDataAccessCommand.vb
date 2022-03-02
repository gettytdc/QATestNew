Imports BluePrism.Data

Public Interface IDataAccessCommand
    Inherits IDisposable
    Function Execute(databaseConnection As IDatabaseConnection) As Object
End Interface
