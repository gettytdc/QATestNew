Imports BluePrism.BPCoreLib.Data
Imports System.Runtime.Serialization

''' <summary>
''' Component representing a schedule report or timetable
''' </summary>
<Serializable, DataContract([Namespace]:="bp")>
Public Class ScheduleListComponent : Inherits ScheduleStoredComponent

    ''' <summary>
    ''' Creates a new schedule list component from the given data provider.
    ''' </summary>
    ''' <param name="prov">The provider of the data for this schedule list.</param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal prov As IDataProvider)
        MyBase.New(owner, prov.GetValue("id", 0), prov.GetString("name"))
    End Sub

    ''' <summary>
    ''' Creates a new schedule list component from the given properties.
    ''' </summary>
    ''' <param name="id">The ID of the schedule list required.</param>
    ''' <param name="name">The name of the schedule list required.</param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal id As Integer, ByVal name As String)
        MyBase.New(owner, id, name)
    End Sub

    ''' <summary>
    ''' The type of this component.
    ''' </summary>
    Public Overrides ReadOnly Property Type() As PackageComponentType
        Get
            Return PackageComponentType.ScheduleList
        End Get
    End Property

    ''' <summary>
    ''' Loads the database data for this component.
    ''' </summary>
    ''' <returns>The data associated with this component.</returns>
    Protected Overrides Function LoadData() As Object
        Return Store.GetSchedule(IdAsInteger)
    End Function

End Class