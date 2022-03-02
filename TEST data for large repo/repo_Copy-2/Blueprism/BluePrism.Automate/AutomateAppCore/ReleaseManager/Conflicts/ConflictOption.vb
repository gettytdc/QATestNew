

Imports BluePrism.BPCoreLib.Collections

''' <summary>
''' Class describing a conflict option.
''' </summary>
Public Class ConflictOption

    ''' <summary>
    ''' The broad-brush outcome of the user's chosen conflict option. This is a
    ''' vaguer version of the PackageComponent.ModificationType with the loosened
    ''' requirement that each option within a definition must have a unique outcome
    ''' choice associated with it - the ModificationType must be unique within a
    ''' component.
    ''' </summary>
    Public Enum UserChoice

        ''' <summary>
        ''' Skip the component which is causing the conflict
        ''' </summary>
        Skip

        ''' <summary>
        ''' Overwrite the existing component
        ''' </summary>
        Overwrite

        ''' <summary>
        ''' Give the incoming component a new ID
        ''' </summary>
        NewId

        ''' <summary>
        ''' Give the incoming component a new name
        ''' </summary>
        Rename

        ''' <summary>
        ''' Rename the component already on the system, and let the incoming
        ''' component retain its name.
        ''' </summary>
        RenameExisting

        ''' <summary>
        ''' Provide extra info for the import operation
        ''' </summary>
        ExtraInfo

        ''' <summary>
        ''' Allow any component-specific post processing to be carried out once
        ''' the component is successfully imported
        ''' </summary>
        PerformPostProcess

        ''' <summary>
        ''' Omit any component-specific post processing after the component is
        ''' successfully imported
        ''' </summary>
        OmitPostProcess

        ''' <summary>
        ''' Save the component contents to file (only supported by data sources)
        ''' </summary>
        SaveToFile

        ''' <summary>
        ''' Indicates that the release import should not go ahead (only applies
        ''' to non-interactive imports performed using the AutomateC command)
        ''' </summary>
        Fail

    End Enum

#Region " Member Variables "

    ' The resolution type for this option - should be unique within the definition
    ' that this option relates to.
    Private mChoice As UserChoice

    ' The text describing this option.
    Private mText As String

    ' The handlers for this option, mapped against their IDs
    Private mHandlers As IDictionary(Of String, ConflictDataHandler)

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates new conflict option with the given properties
    ''' </summary>
    ''' <param name="res">The resolution choice of this option.</param>
    ''' <param name="txt">The text describing this option.</param>
    Public Sub New(ByVal res As UserChoice, ByVal txt As String)
        Me.New(res, txt, Nothing)
    End Sub

    ''' <summary>
    ''' Creates new conflict option with the given properties
    ''' </summary>
    ''' <param name="res">The resolution choice of this option.</param>
    ''' <param name="txt">The text describing this option.</param>
    ''' <param name="handlers">The data handlers used to retrieve extra information,
    ''' should this option be chosen.</param>
    Public Sub New(ByVal res As UserChoice, ByVal txt As String,
     ByVal ParamArray handlers() As ConflictDataHandler)
        mChoice = res
        mText = txt
        mHandlers = New clsOrderedDictionary(Of String, ConflictDataHandler)
        If handlers IsNot Nothing Then
            For Each handler As ConflictDataHandler In handlers
                mHandlers(handler.Id) = handler
            Next
        End If
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The choice of modification that this option represents unique within its
    ''' conflict definition - ie. each definition should have a single option
    ''' representing a single resolution
    ''' </summary>
    Public ReadOnly Property Choice() As UserChoice
        Get
            Return mChoice
        End Get
    End Property

    ''' <summary>
    ''' The text describing this option.
    ''' </summary>
    Public Property Text() As String
        Get
            Return mText
        End Get
        Set(value As String)
            mText = value
        End Set
    End Property

    ''' <summary>
    ''' The data handlers registered with this option, mapped against their IDs
    ''' </summary>
    Public ReadOnly Property Handlers() As IDictionary(Of String, ConflictDataHandler)
        Get
            Return mHandlers
        End Get
    End Property

#End Region

#Region " Handler Methods "

    ''' <summary>
    ''' Creates a new handler for this option - by default, just a clone of the
    ''' first defined handler on this option.
    ''' </summary>
    ''' <returns>A new data handler - the first defined data handler set in this
    ''' conflict option.</returns>
    Public Function CreateHandler() As ConflictDataHandler
        Return CreateHandler(Nothing)
    End Function

    ''' <summary>
    ''' Creates a new handler, based on the handler with the given ID, defined within
    ''' this conflict option. If no id is given, a new handler based on the first
    ''' handler defined in this option is returned.
    ''' If this option has no defined handler, null is returned.
    ''' </summary>
    ''' <param name="handlerId">The ID of the handler for which a new clone is
    ''' required, or null to return the first handler defined.</param>
    ''' <returns>A new copy of the required handler, or null if no handler with the
    ''' given ID was found in this option, or it has no defined handlers.</returns>
    Public Function CreateHandler(ByVal handlerId As String) As ConflictDataHandler
        If mHandlers.Count = 0 Then Return Nothing
        Dim handler As ConflictDataHandler = Nothing
        If handlerId Is Nothing Then
            ' Then we just want the first one (if there is one
            handler = CollectionUtil.First(Of ConflictDataHandler)(mHandlers.Values)
        Else
            ' Try and get it from the map - if it's not there it remains at null
            mHandlers.TryGetValue(handlerId, handler)
        End If
        Return handler.Clone()
    End Function

#End Region

End Class
