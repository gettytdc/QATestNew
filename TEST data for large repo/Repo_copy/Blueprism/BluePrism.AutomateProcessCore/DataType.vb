Imports System.Runtime.CompilerServices
Imports System.Runtime.Serialization

Imports BluePrism.AutomateProcessCore.My.Resources
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.BPCoreLib

''' <summary>
''' IDs of possible data types. A ToString() on this Enum will return the internal
''' Automate name for the Data Type, e.g. "text".
''' </summary>
<Flags, DataContract([Namespace]:="bp")>
Public Enum DataType As Integer

    ''' <summary>
    ''' No data type is set
    ''' </summary>
    <EnumMember> unknown = 0

    ''' <summary>
    ''' Represents an arbitrary list or collection rows, each containing a set of
    ''' data with varying types. Data Items of this type are typically held within
    ''' a specific <see cref="clsCollectionStage">collection stage</see>.
    ''' </summary>
    <EnumMember> collection = 1

    ''' <summary>
    ''' Represents a date with no time element.
    ''' </summary>
    <EnumMember> [date] = 2

    ''' <summary>
    ''' Represents a date and time.
    ''' </summary>
    <EnumMember> datetime = 4

    ''' <summary>
    ''' Represents a flag value - True or False
    ''' </summary>
    <EnumMember> flag = 8

    ''' <summary>
    ''' Represents a number, possibly with a fractional part.
    ''' </summary>
    <EnumMember> number = 16

    ''' <summary>
    ''' Represents a password. Broadly similar to a
    '''<see cref="DataType.text">text</see> type, its value is obscured when
    ''' displayed in the UI.
    ''' </summary>
    <EnumMember> password = 32

    ''' <summary>
    ''' Represents arbitrary text.
    ''' </summary>
    <EnumMember> text = 64

    ''' <summary>
    ''' Represents a time of day.
    ''' </summary>
    <EnumMember> time = 128

    ''' <summary>
    ''' Represents an amount of time.
    ''' </summary>
    <EnumMember> timespan = 256

    ''' <summary>
    ''' Represents an image - held internally as a <see cref="clsPixRect"/>-encoded
    ''' value.
    ''' </summary>
    <EnumMember> image = 512

    ''' <summary>
    ''' Represents an arbitrary set of bytes - a binary value.
    ''' </summary>
    <EnumMember> binary = 1024

End Enum

Public Module DataTypeExtensions
    <Extension()>
    Public Function ToLocalisedString(ByVal dt As DataType) As String
        Select Case dt
            Case DataType.unknown
                Return DataTypesResources.DataTypes_unknown_Title
            Case DataType.collection
                Return DataTypesResources.DataTypes_collection_Title
            Case DataType.date
                Return DataTypesResources.DataTypes_date_Title
            Case DataType.datetime
                Return DataTypesResources.DataTypes_datetime_Title
            Case DataType.flag
                Return DataTypesResources.DataTypes_flag_Title
            Case DataType.number
                Return DataTypesResources.DataTypes_number_Title
            Case DataType.password
                Return DataTypesResources.DataTypes_password_Title
            Case DataType.text
                Return DataTypesResources.DataTypes_text_Title
            Case DataType.time
                Return DataTypesResources.DataTypes_time_Title
            Case DataType.timespan
                Return DataTypesResources.DataTypes_timespan_Title
            Case DataType.image
                Return DataTypesResources.DataTypes_image_Title
            Case DataType.binary
                Return DataTypesResources.DataTypes_binary_Title
            Case Else
                Throw New InvalidDataTypeException()
        End Select
    End Function
End Module
