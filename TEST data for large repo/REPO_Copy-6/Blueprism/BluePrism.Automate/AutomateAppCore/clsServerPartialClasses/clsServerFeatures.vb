Imports BluePrism.AutomateAppCore.clsServerPartialClasses

Public Partial Class clsServer

    <Obsolete("This method of feature control is no longer used")>
    Friend Function GetFeatureEnabled(feature As Feature) As Boolean Implements IServer.GetFeatureEnabled

        Return False

    End Function

End Class
