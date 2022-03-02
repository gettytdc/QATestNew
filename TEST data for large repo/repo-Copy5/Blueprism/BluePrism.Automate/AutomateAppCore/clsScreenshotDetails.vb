Imports System.Runtime.Serialization

''' <summary>
''' Used to represent details of the screenshot.
''' </summary>
<Serializable, DataContract([Namespace]:="bp")>
Public Class clsScreenshotDetails
    ''' <summary>
    ''' A clsPixRect serialised as a string
    ''' </summary>
    <DataMember>
    Public Property Screenshot As String
    ''' <summary>
    ''' The time at which the exception occoured
    ''' </summary>
    <DataMember>
    Public Property Timestamp As DateTimeOffset
    ''' <summary>
    ''' The name of the resource
    ''' </summary>
    <DataMember>
    Public Property ResourceId As Guid
    ''' <summary>
    ''' The id of the exception stage
    ''' </summary>
    <DataMember>
    Public Property StageId As Guid
    ''' <summary>
    ''' The name of the process or object
    ''' </summary>
    <DataMember>
    Public Property ProcessName As String
End Class
