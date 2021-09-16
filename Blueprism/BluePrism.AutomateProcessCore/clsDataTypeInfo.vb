Imports BluePrism.AutomateProcessCore.My.Resources
Imports BluePrism.BPCoreLib.Collections

''' Project  : AutomateProcessCore
''' Class    : clsDataTypeInfo
''' 
''' <summary>
''' A support class for clsProcessDataTypes.
''' </summary>
Public Class clsDataTypeInfo

    ''' <summary>
    ''' The date/time data types within Blue Prism
    ''' </summary>
    Public Shared ReadOnly DateTimeTypes As ICollection(Of DataType) =
     GetReadOnly.ICollectionFrom(DataType.date, DataType.datetime, DataType.time)

    ''' <summary>
    ''' Creates a new public, statistics-compatible datatype info object with
    ''' the given values.
    ''' </summary>
    ''' <param name="value">The value of the datatype represented by this object.
    ''' </param>
    ''' <param name="friendlyName">The friendly name of this datatype</param>
    ''' <param name="friendlyPlural">The plural version of the friendly name
    ''' of this datatype.</param>
    ''' <param name="friendlyDesc">The friendly description of the datatype.
    ''' </param>
    Public Sub New(
     ByVal value As DataType,
     ByVal friendlyName As String,
     ByVal friendlyPlural As String,
     ByVal friendlyDesc As String)
        Me.New(value, friendlyName, friendlyPlural, True, True, friendlyDesc)
    End Sub

    ''' <summary>
    ''' Creates a new datatype info object with the given values.
    ''' </summary>
    ''' <param name="value">The value of the datatype represented by this object.
    ''' </param>
    ''' <param name="friendlyName">The friendly name of this datatype</param>
    ''' <param name="friendlyPlural">The plural version of the friendly name
    ''' of this datatype.</param>
    ''' <param name="isPublic">whether this datatype is public or not</param>
    ''' <param name="statsCompat">True to indicate statistics compatibility,
    ''' false otherwise.</param>
    ''' <param name="friendlyDesc">The friendly description of the datatype.
    ''' </param>
    Public Sub New(
     ByVal value As DataType,
     ByVal friendlyName As String,
     ByVal friendlyPlural As String,
     ByVal isPublic As Boolean,
     ByVal statsCompat As Boolean,
     ByVal friendlyDesc As String)
        mValue = value
        mFriendlyName = friendlyName
        mFriendlyPlural = friendlyPlural
        mPublic = isPublic
        mFriendlyDescription = friendlyDesc
        mStatsCompat = statsCompat
    End Sub

    ''' <summary>
    ''' The value of the datatype this info object represents.
    ''' </summary>
    Public ReadOnly Property Value() As DataType
        Get
            Return mValue
        End Get
    End Property
    Private mValue As DataType

    ''' <summary>
    ''' The name of the datatype this info object represents. This is equivalent
    ''' to the name of the constant within the DataType enumeration.
    ''' </summary>
    Public ReadOnly Property Name() As String
        Get
            Return mValue.ToString()
        End Get
    End Property

    ''' <summary>
    ''' Gets the neutral friendly name for this datatype.
    ''' </summary>
    ''' <param name="type">The data type</param>
    ''' <returns>The name of the data type for the current culture</returns>
    Public Shared Function GetNeutralFriendlyName(type As DataType, Optional ByVal bPlural As Boolean = False) As String
        If (bPlural) Then
            Return DataTypesResources.ResourceManager.GetString($"DataTypes_{type}_PluralTitle", New Globalization.CultureInfo("en"))
        Else
            Return DataTypesResources.ResourceManager.GetString($"DataTypes_{type}_Title", New Globalization.CultureInfo("en"))
        End If
    End Function

    ''' <summary>
    ''' Gets the localized friendly name For this datatype according To the current culture.
    ''' </summary>
    ''' <param name="type">The data type</param>
    ''' <returns>The name of the data type for the current culture</returns>
    Public Shared Function GetLocalizedFriendlyName(type As DataType, Optional ByVal bPlural As Boolean = False) As String
        If (bPlural) Then
            Return DataTypesResources.ResourceManager.GetString($"DataTypes_{type}_PluralTitle")
        Else
            Return DataTypesResources.ResourceManager.GetString($"DataTypes_{type}_Title")
        End If
    End Function
    ''' <summary>
    ''' The friendly name of the data type this info represents.
    ''' </summary>
    Public ReadOnly Property FriendlyName() As String
        Get
            Return mFriendlyName
        End Get
    End Property
    Private mFriendlyName As String

    ''' <summary>
    ''' The plural version of the friendly name for the underlying datatype.
    ''' </summary>
    Public ReadOnly Property FriendlyPlural() As String
        Get
            Return mFriendlyPlural
        End Get
    End Property
    Private mFriendlyPlural As String

    ''' <summary>
    ''' Flag indicating if this datatype is available to the users.
    ''' </summary>
    Public ReadOnly Property IsPublic() As Boolean
        Get
            Return mPublic
        End Get
    End Property
    Private mPublic As Boolean

    ''' <summary>
    ''' Description of the underlying datatype.
    ''' </summary>
    Public ReadOnly Property FriendlyDescription() As String
        Get
            Return mFriendlyDescription
        End Get
    End Property
    Private mFriendlyDescription As String

    ''' <summary>
    ''' Indicates whether this data type can be used as a statistic.
    ''' Only public data types can be used in this way.
    ''' </summary>
    Public ReadOnly Property IsStatisticCompatible() As Boolean
        Get
            Return mStatsCompat
        End Get
    End Property
    Private mStatsCompat As Boolean

    ''' <summary>
    ''' Gets the friendly name for this datatype, returning the plural form
    ''' if specified.
    ''' </summary>
    ''' <param name="plural">True to return the plural form of the friendly
    ''' name; False to return the singular form.</param>
    ''' <returns>The friendly name for the underlying datatype.</returns>
    Friend Function GetFriendlyName(ByVal plural As Boolean) As String
        If plural Then Return mFriendlyPlural
        Return mFriendlyName
    End Function

    ''' <summary>
    ''' Gets a string representation of this data type info.
    ''' </summary>
    ''' <returns>The friendly name of this object</returns>
    Public Overrides Function ToString() As String
        Return mFriendlyName
    End Function

End Class

