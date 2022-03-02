
Imports System.Drawing
Imports System.Data.SqlClient
Imports BluePrism.AutomateAppCore.Auth

Imports BluePrism.BPCoreLib
Imports BluePrism.Data
Imports BluePrism.AutomateAppCore.DataMonitor
Imports BluePrism.Server.Domain.Models

' Partial class which separates the preferences from the rest of the clsServer
' methods just in order to keep the file size down to a sane level and make it
' easier to actually find functions
Partial Public Class clsServer
    ''' <summary>
    ''' The default background colour in a Blue Prism environment as an
    ''' HTML-encoded colour string.
    ''' </summary>
    Private Const DefaultBackColorAsHTML As String = "#0d2a48"

    ''' <summary>
    ''' The default foreground colour in a Blue Prism environment as an
    ''' HTML-encoded colour string.
    ''' </summary>
    Private Const DefaultForeColorAsHTML As String = "#ffffff"

#Region " Preference Satellite Table Names "

    ''' <summary>
    ''' Holds the pref table names for the supported system types.
    ''' </summary>
    Private Shared ReadOnly PrefTableNames As New SatelliteNameHolder()

    ''' <summary>
    ''' Placeholder class for table names used in the server instance.
    ''' </summary>
    Private Class SatelliteNameHolder

        ''' <summary>
        ''' The map of satellite table names for each type.
        ''' </summary>
        Private Shared ReadOnly mNames As IDictionary(Of Type, String) = CreateMap()

        ''' <summary>
        ''' creates the satellite table map for the supported types.
        ''' </summary>
        ''' <returns>A map of supported system types and the satellite pref table
        ''' which holds data for them.</returns>
        Private Shared Function CreateMap() As IDictionary(Of Type, String)
            Dim map As New Dictionary(Of Type, String)
            map(GetType(String)) = "BPAStringPref"
            map(GetType(Integer)) = "BPAIntegerPref"
            map(GetType(Boolean)) = "BPAIntegerPref"
            map(GetType(Color)) = "BPAStringPref"
            map(GetType(Guid)) = "BPAStringPref"
            Return map
        End Function

        ''' <summary>
        ''' Gets the pref table which holds the data for the specified type.
        ''' </summary>
        ''' <param name="t">The system type representing the type of data which
        ''' is held in the prefs in the database.</param>
        Default Public ReadOnly Property Item(t As Type) As String
            Get
                If t.BaseType Is GetType(System.Enum) Then
                    Return mNames(GetType(Integer))
                End If
                Return mNames(t)
            End Get
        End Property

        ''' <summary>
        ''' Gets the pref table which holds the data for the type with the
        ''' specified name.
        ''' </summary>
        ''' <param name="typeName">The name of the system type representing the
        ''' type of data held in the prefs.</param>
        Default Public ReadOnly Property Item(typeName As String) As String
            Get
                Return Me(Type.GetType(typeName))
            End Get
        End Property

    End Class

#End Region

#Region " Getting prefs "

#Region " Specific GetPref(name) "

    ''' <summary>
    ''' Gets the color preferences for the environment colors
    ''' </summary>
    <UnsecuredMethod()>
    Public Sub GetEnvironmentColors(ByRef backColor As Color, ByRef foreColor As Color) Implements IServer.GetEnvironmentColors
        Using con = GetConnection()
            Dim val As String = Nothing
            If TryGetPref(con, PreferenceNames.Env.EnvironmentBackColor, Nothing, val) Then
                backColor = ColorTranslator.FromHtml(val)
            Else
                backColor = ColorTranslator.FromHtml(DefaultBackColorAsHTML)
            End If
            If TryGetPref(con, PreferenceNames.Env.EnvironmentForeColor, Nothing, val) Then
                foreColor = ColorTranslator.FromHtml(val)
            Else
                foreColor = ColorTranslator.FromHtml(DefaultForeColorAsHTML)
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Gets the integer preference with the given name.
    ''' Throws an exception if the given name was not found.
    ''' This first searches the current logged in user, and then the system
    ''' preferences for the given name.
    ''' </summary>
    ''' <param name="name">The name of the required preference.</param>
    ''' <returns>The integer value assigned to the given name in the preferences.
    ''' </returns>
    ''' <exception cref="NoSuchPreferenceException">If no integer preference was
    ''' found with the specified name.</exception>
    <SecuredMethod(True)>
    Public Function GetIntPref(name As String) As Integer Implements IServer.GetIntPref
        CheckPermissions()
        Using con = GetConnection()
            Dim val As Integer = 0
            Dim id = GetLoggedInUserId()
            If TryGetPref(con, name, id, val) OrElse TryGetPref(con, name, Nothing, val) Then Return val
            Throw New NoSuchPreferenceException(My.Resources.clsServer_IntegerPreference0NotFound, name)
        End Using
    End Function

    ''' <summary>
    ''' Gets the boolean preference with the given name.
    ''' Throws an exception if the given name was not found.
    ''' This first searches the current logged in user, and then the system
    ''' preferences for the given name.
    ''' </summary>
    ''' <param name="name">The name of the required preference.</param>
    ''' <returns>The boolean value assigned to the given name in the preferences.
    ''' </returns>
    ''' <exception cref="NoSuchPreferenceException">If no boolean preference was
    ''' found with the specified name.</exception>
    <SecuredMethod(True)>
    Public Function GetBoolPref(name As String) As Boolean Implements IServer.GetBoolPref
        CheckPermissions()
        Using con = GetConnection()
            ' Boolean prefs piggy back onto integer prefs...
            Dim val As Integer = 0
            Dim id = GetLoggedInUserId()
            If TryGetPref(con, name, id, val) OrElse TryGetPref(con, name, Nothing, val) Then Return (val <> 0)
            Throw New NoSuchPreferenceException(My.Resources.clsServer_BooleanPreference0NotFound, name)
        End Using
    End Function

    ''' <summary>
    ''' Gets the guid preference with the given name.
    ''' Throws an exception if the given name was not found.
    ''' This first searches the current logged in user, and then the system
    ''' preferences for the given name.
    ''' </summary>
    ''' <param name="name">The name of the required preference.</param>
    ''' <returns>The boolean value assigned to the given name in the preferences.
    ''' </returns>
    ''' <exception cref="NoSuchPreferenceException">If no guid preference was
    ''' found with the specified name.</exception>
    <SecuredMethod(True)>
    Public Function GetGuidPref(name As String) As Guid Implements IServer.GetGuidPref
        CheckPermissions()
        Using con = GetConnection()
            ' Guids go in and out as strings
            Dim val As String = Nothing
            Dim id = GetLoggedInUserId()
            If TryGetPref(con, name, id, val) OrElse TryGetPref(con, name, Nothing, val) Then _
                Return New Guid(val)
            Throw New NoSuchPreferenceException(My.Resources.clsServer_GuidPreference0NotFound, name)
        End Using
    End Function

    ''' <summary>
    ''' Gets the string preference with the given name.
    ''' Throws an exception if the given name was not found.
    ''' </summary>
    ''' <param name="name">The name of the required preference.</param>
    ''' <returns>The string value assigned to the given name in the preferences.
    ''' </returns>
    ''' <exception cref="NoSuchPreferenceException">If no string preference was
    ''' found with the specified name.</exception>
    <SecuredMethod(True)>
    Public Function GetStringPref(name As String) As String Implements IServer.GetStringPref
        CheckPermissions()
        Using con = GetConnection()
            Dim val As String = Nothing
            Dim id = GetLoggedInUserId()
            If TryGetPref(con, name, id, val) OrElse TryGetPref(con, name, Nothing, val) Then Return val
            Throw New NoSuchPreferenceException(My.Resources.clsServer_StringPreference0NotFound, name)
        End Using
    End Function

#End Region

#Region " Generic GetPref(name, defaultValue) "

    ''' <summary>
    ''' Gets the preference with the given name and of the given type, or the
    ''' specified default value if the preference was not found.
    ''' This will check the user prefs for the current user (if a user is logged
    ''' in), then the system prefs for a pref of the given name and type.
    ''' </summary>
    ''' <typeparam name="T">The type of the value held in this preference. This
    ''' decides which satellite table to retrieve the pref value from.
    ''' </typeparam>
    ''' <param name="name">The name of the preference for which the value is
    ''' required.</param>
    ''' <param name="defaultValue">The value to return if no user pref or
    ''' system pref could be found with the given name.</param>
    ''' <returns>The value representing the required pref or the specified
    ''' default value if no pref could be found.</returns>
    Private Function _GetPref(Of T)(name As String, defaultValue As T) As T
        Using con = GetConnection()
            Dim val As T = Nothing

            ' If we're logged in, check the user pref - otherwise just check the
            ' system pref.
            ' We may not be logged in (eg. the scheduler determines whether to
            ' run or not based on a system pref before any user logs in)
            Dim id = GetLoggedInUserId()
            If (id <> Nothing AndAlso TryGetPref(con, name, id, val)) _
             OrElse TryGetPref(con, name, Nothing, val) Then
                Return val
            End If
        End Using
        Return defaultValue

    End Function

    <SecuredMethod(True)>
    Public Function GetPref(ByVal name As String, ByVal defaultValue As String) As String Implements IServer.GetPref
        CheckPermissions()
        Return _GetPref(name, defaultValue)
    End Function
    <SecuredMethod(True)>
    Public Function GetPref(ByVal name As String, ByVal defaultValue As Integer) As Integer Implements IServer.GetPref
        CheckPermissions()
        Return _GetPref(name, defaultValue)
    End Function
    <SecuredMethod(True)>
    Public Function GetPref(ByVal name As String, ByVal defaultValue As Boolean) As Boolean Implements IServer.GetPref
        CheckPermissions()
        Return _GetPref(name, defaultValue)
    End Function
    <SecuredMethod(True)>
    Public Function GetPref(ByVal name As String, ByVal defaultValue As Color) As Color Implements IServer.GetPref
        CheckPermissions()
        Return _GetPref(name, defaultValue)
    End Function
    <SecuredMethod(True)>
    Public Function GetPref(ByVal name As String, ByVal defaultValue As Guid) As Guid Implements IServer.GetPref
        CheckPermissions()
        Return _GetPref(name, defaultValue)
    End Function

    <UnsecuredMethod>
    Public Function GetConnectionCheckRetrySecondsPref(defaultValue As Integer) As Integer Implements IServer.GetConnectionCheckRetrySecondsPref
        Return _GetPref("ConnectionCheckRetrySeconds", defaultValue)
    End Function

    ''' <summary>
    ''' Gets the preference with the given name and of the given type, or the
    ''' specified default value if the preference was not found.
    ''' This will check the system prefs for a pref of the given name and type.
    ''' </summary>
    ''' <typeparam name="T">The type of the value held in this preference. This
    ''' decides which satellite table to retrieve the pref value from.
    ''' </typeparam>
    ''' <param name="name">The name of the preference for which the value is
    ''' required.</param>
    ''' <param name="defaultValue">The value to return if no user pref or
    ''' system pref could be found with the given name.</param>
    ''' <returns>The value representing the required pref or the specified
    ''' default value if no pref could be found.</returns>
    Private Function GetSystemPref(Of T)(
        con As IDatabaseConnection, name As String, defaultValue As T) As T
        Dim val As T = Nothing
        If TryGetPref(con, name, Nothing, val) Then
            Return val
        End If
        Return defaultValue

    End Function

#End Region

#Region " Implementation of Preference Retrieval "

    ''' <summary>
    ''' Attempts to get the generically typed value assigned to the preference
    ''' with the given name.
    ''' </summary>
    ''' <typeparam name="T">The type of data required / expected. This should be
    ''' <see cref="IConvertible">convertible</see> into directly from the data
    ''' returned (using an <see cref="SqlDataReader"/> object)</typeparam>
    ''' <param name="name">The name of the required preference.</param>
    ''' <param name="userId">The ID of the user for which the preference is
    ''' required. A value of <see cref="Guid.Empty"/> indicates that a system
    ''' preference is required - ie. a preference with no user ID set.</param>
    ''' <param name="val">The reference value into which the value is set from
    ''' the database, if found.</param>
    ''' <returns>True if a preference was found, False if no preference was
    ''' found in the given table with the given name.</returns>
    Private Function TryGetPref(Of T)(
     con As IDatabaseConnection, name As String, userId As Guid, ByRef val As T) As Boolean
        ' Get the table name to retrieve the pref value from
        Dim tableName As String = PrefTableNames(GetType(T))
        If tableName Is Nothing Then
            Throw New ArgumentException(String.Format(
             My.Resources.clsServer_NoSatelliteTableSpecifiedForPreferenceType0, GetType(T)))
        End If

        Dim sql = String.Format(
         " select" &
         "   i.value " &
         " from BPAPref p " &
         "   join {0} i on p.id = i.prefid" &
         " where p.name = @name and p.userid {1}",
         ValidateTableName(con, tableName),
         IIf(userId = Nothing, "is null", "= @userid"))

        Using cmd As New SqlCommand(sql)
            cmd.Parameters.AddWithValue("@name", name)
            If userId <> Nothing Then cmd.Parameters.AddWithValue("@userid", userId)

            Using reader = con.ExecuteReturnDataReader(cmd)
                If Not reader.Read() Then Return False
                ' Convert into our target type... this handles bool..int
                ' and vice versa amongst other things.
                Dim type As Type = GetType(T)
                If type.BaseType Is GetType([Enum]) Then
                    val = DirectCast([Enum].ToObject(type, reader(0)), T)
                ElseIf type Is GetType(Color) Then
                    ' Special Case for colors - we need to convert it separately
                    ' We need to cast it through Object first since 'Color' can't
                    ' be cast into 'T' directly
                    val = DirectCast(
                        CObj(ColorTranslator.FromHtml(reader.GetString(0))), T)
                ElseIf type Is GetType(Guid) Then
                    val = DirectCast(CObj(New Guid(reader.GetString(0))), T)
                Else
                    val = DirectCast(Convert.ChangeType(reader(0), GetType(T)), T)
                End If
                Return True
            End Using
        End Using
    End Function

#End Region

#End Region

#Region " Setting prefs "

#Region " Generic SetSystemPref() and SetUserPref() "

    ''' <summary>
    ''' Sets the system preference with the given name to the given value.
    ''' </summary>
    ''' <param name="name">The name of the preference</param>
    ''' <param name="value">The value of the preference</param>
    Private Sub _SetSystemPref(Of T)(name As String, value As T)
        Using con = GetConnection()
            SetSystemPref(con, name, value)
        End Using
    End Sub

    <SecuredMethod(True)>
    Public Sub SetSystemPref(ByVal name As String, ByVal value As String) Implements IServer.SetSystemPref
        CheckPermissions()
        _SetSystemPref(name, value)
    End Sub
    <SecuredMethod(True)>
    Public Sub SetSystemPref(ByVal name As String, ByVal value As Integer) Implements IServer.SetSystemPref
        CheckPermissions()
        _SetSystemPref(name, value)
    End Sub
    <SecuredMethod(True)>
    Public Sub SetSystemPref(ByVal name As String, ByVal value As Boolean) Implements IServer.SetSystemPref
        CheckPermissions()
        _SetSystemPref(name, value)
    End Sub
    <SecuredMethod(True)>
    Public Sub SetSystemPref(ByVal name As String, ByVal value As Color) Implements IServer.SetSystemPref
        CheckPermissions()
        _SetSystemPref(name, value)
    End Sub
    <SecuredMethod(True)>
    Public Sub SetSystemPref(ByVal name As String, ByVal value As Guid) Implements IServer.SetSystemPref
        CheckPermissions()
        _SetSystemPref(name, value)
    End Sub

    ''' <summary>
    ''' Sets the system preference with the given name to the given value.
    ''' </summary>
    ''' <param name="name">The name of the preference</param>
    ''' <param name="value">The value of the preference</param>
    Private Sub SetSystemPref(Of T)(con As IDatabaseConnection, name As String, value As T)
        SetPref(con, name, Nothing, value)
    End Sub

    ''' <summary>
    ''' Sets the current user's preference with the given name to the given value
    ''' </summary>
    ''' <param name="name">The name of the preference</param>
    ''' <param name="value">The value of the preference</param>
    Private Sub _SetUserPref(Of T)(name As String, value As T)
        SetSpecificUserPref(name, GetLoggedInUserId(), value)
    End Sub

    <SecuredMethod(True)>
    Public Sub SetUserPref(ByVal name As String, ByVal value As String) Implements IServer.SetUserPref
        CheckPermissions()
        _SetUserPref(name, value)
    End Sub
    <SecuredMethod(True)>
    Public Sub SetUserPref(ByVal name As String, ByVal value As Integer) Implements IServer.SetUserPref
        CheckPermissions()
        _SetUserPref(name, value)
    End Sub
    <SecuredMethod(True)>
    Public Sub SetUserPref(ByVal name As String, ByVal value As Boolean) Implements IServer.SetUserPref
        CheckPermissions()
        _SetUserPref(name, value)
    End Sub
    <SecuredMethod(True)>
    Public Sub SetUserPref(ByVal name As String, ByVal value As Color) Implements IServer.SetUserPref
        CheckPermissions()
        _SetUserPref(name, value)
    End Sub
    <SecuredMethod(True)>
    Public Sub SetUserPref(ByVal name As String, ByVal value As Guid) Implements IServer.SetUserPref
        CheckPermissions()
        _SetUserPref(name, value)
    End Sub

    ''' <summary>
    ''' Sets the given user's preference with the given name to the given value
    ''' </summary>
    ''' <param name="name">The name of the preference</param>
    ''' <param name="value">The value of the preference</param>
    ''' <remarks>This was previously an overload of <see cref="SetUserPref"/>,
    ''' but .net remoting has issues with overloaded, generic methods - see bug
    ''' 5680, comment 4</remarks>
    Friend Sub SetSpecificUserPref(Of T)(name As String, userId As Guid, value As T)
        If userId = Nothing Then Throw New ArgumentException(My.Resources.clsServer_AUserPrefRequiresAValidUserID)
        Using con = GetConnection()
            SetPref(con, name, userId, value)
        End Using
    End Sub

#End Region

#Region " Implementation of Preference Setting "

    ''' <summary>
    ''' Sets the preference with the given name and scope to the given value
    ''' </summary>
    ''' <typeparam name="T">The type of preference to set. Typically, this ties
    ''' in with the name of the satellite table to set the value in.
    ''' </typeparam>
    ''' <param name="name">The name of the preference</param>
    ''' <param name="userId">The ID of the user to set the preference for. If
    ''' this is <see cref="Guid.Empty"/> then a system preference will be set.
    ''' </param>
    ''' <param name="value">The value of the preference</param>
    ''' <exception cref="MissingItemException">If no <see cref="PrefTableNames">
    ''' satellite table</see> is defined for the type</exception>
    Private Sub SetPref(Of T)(
     con As IDatabaseConnection, name As String, userId As Guid, value As T)

        ' Get the table name to retrieve the pref value from
        Dim tableName As String = PrefTableNames(GetType(T))
        If tableName Is Nothing Then Throw New MissingItemException(
             My.Resources.clsServer_NoSatelliteTableSpecifiedForPreferenceType0, GetType(T))

        ' Bit of a hack to get color in and out with ease...
        ' We could do with creating a 'convert-in / convert-out' structure that we
        ' could use to encode... well, anything really
        If GetType(T) Is GetType(Color) Then
            SetPref(Of String)(con, name, userId,
                ColorTranslator.ToHtml(DirectCast(CObj(value), Color)))
            Return
        ElseIf GetType(T) Is GetType(Guid) Then
            SetPref(Of String)(con, name, userId, value.ToString())
            Return
        End If

        Using cmd As New SqlCommand()

            ' If a pref already exists with this name and scope, delete it.
            ' - this is the safest way as it ensures that the satellite record
            ' is deleted too, even if it isn't in 'tableName', but a table 
            ' representing a different type.
            ' Once that's done insert the new pref and its satellite record
            cmd.CommandText = String.Format( _
             " if exists (select 1 from BPAPref where name=@name and userid {1}) " & _
             "   delete from BPAPref where name = @name and userid {1}; " & _
 _
             " insert into BPAPref (name, userid) " & _
             "   values (@name, @userId); " & _
 _
             " insert into {0} (prefid, value)" & _
             "   values (scope_identity(), @value)", _
             ValidateTableName(con, tableName),
             IIf(userId = Nothing, "is null", "= @userId") _
            )

            ' Set the base parameters - the 'prefid' param will be set later when
            ' updating the satellite table.
            With cmd.Parameters
                .AddWithValue("@name", name)
                .AddWithValue("@userId", IIf(userId = Nothing, DBNull.Value, userId))
                .AddWithValue("@value", value)
                .AddWithValue("@prefid", DBNull.Value)
            End With

            con.Execute(cmd)
        End Using

        IncrementDataVersion(con, DataNames.Preferences)

    End Sub

#End Region

#End Region

#Region " Delete prefs "

    ''' <summary>
    ''' Deletes the user pref with the given name for the currently logged in user.
    ''' </summary>
    ''' <param name="prefName">The pref name to delete for the current user.</param>
    <SecuredMethod(True)>
    Public Sub DeleteUserPref(prefName As String) Implements IServer.DeleteUserPref
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand(
                "delete from BPAPref where name = @name and userid = @userId")
            With cmd.Parameters
                .AddWithValue("@name", prefName)
                .AddWithValue("@userId", GetLoggedInUserId())
            End With
            con.Execute(cmd)
        End Using
    End Sub

#End Region

End Class
