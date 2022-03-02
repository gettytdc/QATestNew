Imports BluePrism.Core.ActiveDirectory.DirectoryServices
Imports BluePrism.Core.ActiveDirectory.UserQuery

Namespace clsServerPartialClasses.ActiveDirectory
    Public Class MappedActiveDirectoryUserFinder : Implements IMappedUserFinder

        Private ReadOnly mServer As IServerPrivate

        Public Sub New(serverPrivate As IServerPrivate)
            mServer = serverPrivate
        End Sub

        Public Function AlreadyMappedSids(activeDirectoryUsers As IEnumerable(Of ISearchResult)) As HashSet(Of String) Implements IMappedUserFinder.AlreadyMappedSids
            Return mServer.GetAlreadyMappedActiveDirectoryUsers(activeDirectoryUsers)
        End Function
    End Class

End Namespace
