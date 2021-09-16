Imports System.IO
Imports System.Xml
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Core.Xml
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Core.Utility

''' <summary>
''' User-specific configuration object.
''' </summary>
Public Class UserConfig : Inherits BaseConfig

    ' The last connection entered for this user
    Private mSelectedConnectionName As String

    ' The last locale entered for this user
    Private mSelectedLocale As String

    ' The connection mapping of last user logged in keyed against the connection that
    ' they logged into.
    Private WithEvents mConnectionMapping As clsEventFiringDictionary(Of String, String)

    Public Sub New(location As IConfigLocator)
        MyBase.New(location)
    End Sub

    ''' <summary>
    ''' Gets the name of the last connection that the current windows user logged
    ''' in with, or null if no such connection is recorded.
    ''' </summary>
    ''' <remarks>Note that there is no guarantee that the connection name returned
    ''' by this property actually exists in the machine settings - those could all
    ''' have been changed since the last time that this user config was loaded.
    ''' </remarks>
    Public Property SelectedConnectionName() As String
        Get
            Return mSelectedConnectionName
        End Get
        Set(ByVal value As String)
            If mSelectedConnectionName = value Then Return
            mSelectedConnectionName = value
            TrySave()
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the last user name for the currently set 'last connection'.
    ''' Returns null if there is no last connection, or no last user on the current
    ''' last connection. Throws an exception when setting without a last connection
    ''' name currently set, since the property is meaningless without one.
    ''' </summary>
    ''' <exception cref="InvalidOperationException">If an attempt is made to set the
    ''' last user name when no last connection name is currently set.</exception>
    Public Property LastUserName() As String
        Get
            Return GetLastUsernameFor(Nothing)
        End Get
        Set(ByVal value As String)
            If mSelectedConnectionName Is Nothing Then Throw New InvalidOperationException(
             "Cannot set the last user when no last connection is set")
            ConnectionMapping(mSelectedConnectionName) = value
        End Set
    End Property

    ''' <summary>
    ''' Sets the last connection and username to the given values.
    ''' </summary>
    ''' <param name="connection">The name to set as the last connection</param>
    ''' <param name="username">The username to set as the last user.</param>
    Public Sub SetLast(ByVal connection As String, ByVal username As String)

        Dim currMappedUser As String = Nothing
        ' We need to save if either the connection differs -or-
        ' if the connection mapping for that connection doesn't exist -or-
        ' the current mapped user differs from the incoming username
        Dim shouldSave As Boolean = connection <> mSelectedConnectionName OrElse
         Not ConnectionMapping.TryGetValue(connection, currMappedUser) OrElse
         currMappedUser <> username

        mSelectedConnectionName = connection
        ' Use the inner dictionary to effectively disable the event, ensuring that
        ' the save() isn't called as part of updating the collection
        If username IsNot Nothing Then mConnectionMapping.InnerDictionary(connection) = username

        ' Save all in one go (if appropriate)
        If shouldSave Then Save()

    End Sub

    ''' <summary>
    ''' Last-used locale, or Nothing for not overridden.
    ''' </summary>
    Public Property SelectedLocale() As String
        Get
            Return mSelectedLocale
        End Get
        Set(ByVal value As String)
            If mSelectedLocale = value Then Return
            value = If(CultureHelper.IsLatinAmericanSpanish(value), CultureHelper.LatinAmericanSpanish, value)
            mSelectedLocale = If(IsFrenchLanguage(value), "fr-FR", value)
            If Not IsPseudoLocalizationLanguage(value) Then TrySave(Nothing)
        End Set
    End Property

    Private Function IsFrenchLanguage(value As String) As Boolean
        Return Not String.IsNullOrEmpty(value) AndAlso value.Length >= 2 AndAlso value.Substring(0, 2) = "fr"
    End Function

    Private Function IsPseudoLocalizationLanguage(value As String) As Boolean
        Return String.Equals(value, "gsw-li", StringComparison.OrdinalIgnoreCase)
    End Function

    ''' <summary>
    ''' Last-used branding override title, or Nothing for not overridden.
    ''' </summary>
    Public Property LastBrandingTitle() As String
        Get
            Return mLastBrandingTitle
        End Get
        Set(ByVal value As String)
            If mLastBrandingTitle = value Then Return
            mLastBrandingTitle = value
            TrySave(Nothing)
        End Set
    End Property
    Private mLastBrandingTitle As String

    ''' <summary>
    ''' Last-used branding override icon, or Nothing for not overridden.
    ''' </summary>
    Public Property LastBrandingIcon() As String
        Get
            Return mLastBrandingIcon
        End Get
        Set(ByVal value As String)
            If mLastBrandingIcon = value Then Return
            mLastBrandingIcon = value
            TrySave(Nothing)
        End Set
    End Property
    Public mLastBrandingIcon As String

    ''' <summary>
    ''' Last-used branding override large logo, or Nothing for not overridden.
    ''' </summary>
    Public Property LastBrandingLargeLogo() As String
        Get
            Return mLastBrandingLargeLogo
        End Get
        Set(ByVal value As String)
            If mLastBrandingLargeLogo = value Then Return
            mLastBrandingLargeLogo = value
            TrySave(Nothing)
        End Set
    End Property
    Public mLastBrandingLargeLogo As String

    ''' <summary>
    ''' Gets the last user for the given connection name, or null if no such last
    ''' user exists, or if the connection name was not found in this user config.
    ''' </summary>
    ''' <param name="connectionName">The name of the connection for which the last
    ''' user is required. Null will get the last user for the last logged in
    ''' connection by the current windows user.</param>
    ''' <returns>The last username which was registered in this user config for the
    ''' connection with the given name, or null if there is no user registered with
    ''' the specified connection, or the specified connection is not registered in
    ''' this user config.</returns>
    Public Function GetLastUsernameFor(ByVal connectionName As String) As String
        ' If they asked for the default last user and we have a 'last connection'
        ' available, get the user for that connection
        If connectionName Is Nothing Then connectionName = mSelectedConnectionName

        ' If we still don't have a connection name, return null
        If connectionName Is Nothing Then Return Nothing

        Dim username As String = Nothing
        ConnectionMapping.TryGetValue(connectionName, username)
        Return username

    End Function

    ''' <summary>
    ''' The map of last logged in usernames (the values) against the connection
    ''' names that those usernames belong to (the keys).
    ''' </summary>
    Public ReadOnly Property ConnectionMapping() As IDictionary(Of String, String)
        Get
            If mConnectionMapping Is Nothing Then _
             mConnectionMapping = New clsEventFiringDictionary(Of String, String)
            Return mConnectionMapping
        End Get
    End Property

    ''' <summary>
    ''' Handles a connection mapping being removed - this ensures that if the
    ''' removed connection mapping was the last connection name, then the last
    ''' connection name is reset.
    ''' </summary>
    ''' <param name="coll">The collection on which the connection is being removed.
    ''' </param>
    ''' <param name="item">The entry being removed - the key represents the
    ''' connection name; the value represents the last logged in username on that
    ''' connection.</param>
    Private Sub HandleConnectionRemoved(
     ByVal coll As ICollection(Of KeyValuePair(Of String, String)),
     ByVal item As KeyValuePair(Of String, String)) Handles mConnectionMapping.ItemRemoved
        If item.Key = mSelectedConnectionName Then mSelectedConnectionName = Nothing
        TrySave()
    End Sub

    ''' <summary>
    ''' Handles the connection map being cleared.
    ''' This ensures that the last connection name is cleared at the same time.
    ''' </summary>
    ''' <param name="coll">The collection which is being cleared.</param>
    Private Sub HandleConnectionMapCleared(
     ByVal coll As ICollection(Of KeyValuePair(Of String, String)),
     ByVal clearedPairs As ICollection(Of KeyValuePair(Of String, String))) _
     Handles mConnectionMapping.CollectionCleared
        mSelectedConnectionName = Nothing
        TrySave()
    End Sub

    ''' <summary>
    ''' Handles the connection map being added to
    ''' </summary>
    ''' <param name="coll">The collection to which an entry was added</param>
    ''' <param name="item">The item that was added</param>
    Private Sub HandleConnectionAdded(
     ByVal coll As ICollection(Of KeyValuePair(Of String, String)),
     ByVal item As KeyValuePair(Of String, String)) Handles mConnectionMapping.ItemAdded
        TrySave()
    End Sub

    ''' <summary>
    ''' Handles a connection mapping being set - this autosaves the config.
    ''' </summary>
    ''' <param name="map">The dictionary in which the connection is set.</param>
    ''' <param name="entry">The mapping entry that was set</param>
    ''' <param name="oldVal">The previous value of the mapping entry, or null if it
    ''' had no previous value.</param>
    Private Sub HandleConnectionSet(ByVal map As IDictionary(Of String, String),
     ByVal entry As KeyValuePair(Of String, String), ByVal oldVal As String) _
     Handles mConnectionMapping.ItemSet
        If entry.Value <> oldVal Then TrySave()
    End Sub

    ''' <summary>
    ''' Saves this config object to the given stream writer.
    ''' </summary>
    ''' <param name="tw">The writer to which this config should be saved.</param>
    Protected Overrides Sub Save(ByVal tw As TextWriter)

        Dim doc As New XmlDocument()
        Dim root As XmlNode = doc.AppendChild(doc.CreateElement("user-config"))

        Dim mapElement As XmlElement = doc.CreateElement("connection-map")
        For Each entry As KeyValuePair(Of String, String) In ConnectionMapping
            Dim entryElement As XmlElement = doc.CreateElement("connection")
            entryElement.SetAttribute("name", entry.Key)
            entryElement.SetAttribute("user", entry.Value)
            mapElement.AppendChild(entryElement)
        Next
        root.AppendChild(mapElement)

        If mSelectedConnectionName <> "" Then
            Dim connElement As XmlElement = doc.CreateElement("last-connection")
            connElement.AppendChild(doc.CreateTextNode(mSelectedConnectionName))
            root.AppendChild(connElement)
        End If

        If mSelectedLocale <> "" Then
            Dim connElement As XmlElement = doc.CreateElement("last-locale")
            connElement.AppendChild(doc.CreateTextNode(mSelectedLocale))
            root.AppendChild(connElement)
        End If

        If mLastBrandingLargeLogo IsNot Nothing Then
            Dim connElement As XmlElement = doc.CreateElement("last-branding-large-logo")
            connElement.AppendChild(doc.CreateTextNode(mLastBrandingLargeLogo))
            root.AppendChild(connElement)
        End If

        If mLastBrandingTitle IsNot Nothing Then
            Dim connElement As XmlElement = doc.CreateElement("last-branding-title")
            connElement.AppendChild(doc.CreateTextNode(mLastBrandingTitle))
            root.AppendChild(connElement)
        End If

        doc.Save(tw)

    End Sub

    ''' <summary>
    ''' Loads this config object's properties from the given stream reader
    ''' </summary>
    ''' <param name="reader">The reader whence to draw the config properties.</param>
    Protected Overrides Sub Load(ByVal reader As TextReader)

        Dim doc As New ReadableXmlDocument()
        doc.Load(reader)

        If doc.DocumentElement.Name <> "user-config" Then Throw New NoSuchElementException(
         "The user config file's root element should be 'user-config' - found '{0}' instead",
         doc.DocumentElement.Name)

        ' It's a valid XML document, and it has the right root node - so initialise
        ' this class. When setting use the variable / inner dictionary to ensure that
        ' a file save attempt isn't made while setting the properties.
        mSelectedConnectionName = Nothing
        mSelectedLocale = Nothing
        mLastBrandingLargeLogo = Nothing
        mLastBrandingTitle = Nothing
        Dim mapWithoutEvents As IDictionary(Of String, String) =
         DirectCast(ConnectionMapping, clsEventFiringDictionary(Of String, String)).InnerDictionary
        mapWithoutEvents.Clear() ' Avoid firing events by using the wrapped map

        For Each el As XmlElement In doc.SelectNodes("/user-config/connection-map/connection")
            mapWithoutEvents(el.GetAttribute("name")) = el.GetAttribute("user")
        Next

        ' There should only be one or zero 'last-connection' nodes
        Dim lastConnNode As XmlNode = doc.SelectSingleNode("/user-config/last-connection")
        If lastConnNode IsNot Nothing Then mSelectedConnectionName = lastConnNode.InnerText

        ' There should only be one or zero 'last-locale' nodes
        Dim lastLocaleNode As XmlNode = doc.SelectSingleNode("/user-config/last-locale")
        If lastLocaleNode IsNot Nothing Then
            mSelectedLocale = lastLocaleNode.InnerText
            If Not String.IsNullOrWhiteSpace(lastLocaleNode.InnerText) Then
                If lastLocaleNode.InnerText.Length >= 2 AndAlso lastLocaleNode.InnerText.Substring(0, 2) = "fr" Then
                    mSelectedLocale = "fr-FR"
                End If
                If CultureHelper.IsLatinAmericanSpanish(lastLocaleNode.InnerText) Then
                    mSelectedLocale = CultureHelper.LatinAmericanSpanish
                End If
            End If
        End If

        Dim n As XmlNode = doc.SelectSingleNode("/user-config/last-branding-large-logo")
        If n IsNot Nothing Then mLastBrandingLargeLogo = n.InnerText
        n = doc.SelectSingleNode("/user-config/last-branding-title")
        If n IsNot Nothing Then mLastBrandingTitle = n.InnerText

    End Sub


    Protected Overrides ReadOnly Property FileName As String
        Get
            Return "User.config"
        End Get
    End Property

    Protected Overrides ReadOnly Property ConfigFile As FileInfo
        Get
            Return New FileInfo(Path.Combine(mLocation.UserConfigDirectory.FullName, FileName))
        End Get
    End Property

End Class
