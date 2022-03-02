Imports System.Collections.ObjectModel
Imports System.Linq
Imports System.Runtime.Serialization
Imports BluePrism.AutomateProcessCore.My.Resources

<DataContract([Namespace]:="bp"), Serializable>
Public Class WebConnectionSettings : Inherits WebConnectionBaseSettings

    <DataMember>
    Private mUriSpecificSettings As ReadOnlyCollection(Of UriWebConnectionSettings)

    Public Sub New(connectionLimit As Integer, maxIdleTime As Integer, connectionLeaseTimeout As Integer?, uriSettings As IEnumerable(Of UriWebConnectionSettings))

        MyBase.New(connectionLimit, maxIdleTime, connectionLeaseTimeout)

        ValidateUriSettings(uriSettings)

        mUriSpecificSettings = uriSettings.ToList().AsReadOnly
    End Sub

    Public ReadOnly Property UriSpecificSettings As ReadOnlyCollection(Of UriWebConnectionSettings)
        Get
            Return mUriSpecificSettings
        End Get
    End Property


    Private Shared Sub ValidateUriSettings(uriSettings As IEnumerable(Of UriWebConnectionSettings))
        Dim groupedByUri = uriSettings.GroupBy(Function(s) s.BaseUri)

        Dim groupWithDuplicate = groupedByUri.FirstOrDefault(Function(g) g.Count > 1)

        If groupWithDuplicate IsNot Nothing Then _
            Throw New ArgumentException(String.Format(WebConnectionSettingsResources.Error_DuplicateUri_Template, groupWithDuplicate.Key))
    End Sub

    ''' <summary>
    ''' Method which checks the collection of Uri specific settings to see if any 
    ''' already exist which apply for the Uri passed in, and return it if so. 
    ''' If not will return Nothing.
    ''' </summary>
    ''' <param name="requestUri">The uri to check for an applicable setting</param>
    ''' <returns>The applicable UriWebConnectionSettings or Nothing if none exist</returns>
    Public Function GetExistingUriSettings(requestUri As Uri) As UriWebConnectionSettings
        Dim uriSettings As UriWebConnectionSettings = Nothing

        Dim baseUri = requestUri.GetLeftPart(System.UriPartial.Authority)

        uriSettings = UriSpecificSettings.
            FirstOrDefault(Function(g) g.BaseUri.ToString.TrimEnd("/"c) = baseUri)

        Return uriSettings
    End Function


    Public Overrides Function Equals(settings As Object) As Boolean
        Dim webSettings = TryCast(settings, WebConnectionSettings)
        If webSettings Is Nothing Then Return False

        Return ConnectionLimit = webSettings.ConnectionLimit AndAlso
               MaxIdleTime = webSettings.MaxIdleTime AndAlso
               Nullable.Equals(ConnectionLeaseTimeout, webSettings.ConnectionLeaseTimeout) AndAlso
               New HashSet(Of UriWebConnectionSettings)(webSettings.UriSpecificSettings).SetEquals(UriSpecificSettings)
    End Function
End Class
