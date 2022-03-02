Imports BluePrism.BPCoreLib
Imports BluePrism.Server.Domain.Models

''' <summary>
''' Class describing an individual conflict - ie. a collision between incoming data
''' and existing data which requires a choice to be made to determine how to resolve
''' it.
''' </summary>
Public Class Conflict

    ''' <summary>
    ''' The delegate responsible for applying resolutions. Each component has a
    ''' different mechanism for applying resolutions.
    ''' </summary>
    ''' <param name="rel">The release that is being checked</param>
    ''' <param name="resolutions">The resolutions being applied.</param>
    ''' <param name="errors">The error log to which any cross-component errors should
    ''' be logged.</param>
    ''' <remarks><seealso cref="PackageComponent.ResolutionApplier"/></remarks>
    Public Delegate Sub Resolver(ByVal rel As clsRelease, _
     ByVal resolutions As ICollection(Of ConflictResolution), _
     ByVal errors As clsErrorLog)

    ' The component that this conflict relates to
    Private mComponent As PackageComponent

    ' The definition of this conflict
    Private mDefn As ConflictDefinition

    ' The option chosen, null if none has been chosen
    Private mOption As ConflictOption

    ' The map of handlers against their respective conflict options.
    Private mHandlers As IDictionary(Of ConflictOption, ConflictDataHandler)

    ' The resolution set in this conflict, if it has one.
    Private mResolution As ConflictResolution

    ''' <summary>
    ''' Creates a new conflict on the given component, defined by the given
    ''' definition.
    ''' </summary>
    ''' <param name="comp">The component on which this conflict occurred</param>
    ''' <param name="defn">The type of conflict which has occurred.</param>
    Public Sub New(ByVal comp As PackageComponent, ByVal defn As ConflictDefinition)
        mComponent = comp
        mDefn = defn
        mHandlers = New Dictionary(Of ConflictOption, ConflictDataHandler)
    End Sub

    ''' <summary>
    ''' The component which this conflict relates to
    ''' </summary>
    Public Property Component() As PackageComponent
        Get
            Return mComponent
        End Get
        Set(ByVal value As PackageComponent)
            mComponent = value
        End Set
    End Property

    ''' <summary>
    ''' The definition of this conflict.
    ''' </summary>
    Public Property Definition() As ConflictDefinition
        Get
            Return mDefn
        End Get
        Set(ByVal value As ConflictDefinition)
            mDefn = value
        End Set
    End Property

    ''' <summary>
    ''' The resolution of this conflict, if it has one. This should only be set
    ''' when the conflict has been fully resolved. No internal validation of that
    ''' state is performed by the conflict class.
    ''' </summary>
    Public Property Resolution() As ConflictResolution
        Get
            Return mResolution
        End Get
        Set(ByVal value As ConflictResolution)
            mResolution = value
        End Set
    End Property

    ''' <summary>
    ''' Checks if this conflict has been resolved - true if it has a resolution
    ''' set within it. No further validation is performed.
    ''' </summary>
    Public ReadOnly Property IsResolved() As Boolean
        Get
            Return (mResolution IsNot Nothing AndAlso
                mResolution.ConflictOption.Choice <> ConflictOption.UserChoice.Fail)
        End Get
    End Property

    ''' <summary>
    ''' The chosen option within this conflict, or null if no conflict option has
    ''' been chosen (or, at least, set within this conflict object)
    ''' </summary>
    Public Property ChosenOption() As ConflictOption
        Get
            Return mOption
        End Get
        Set(ByVal value As ConflictOption)
            ChooseOption(value)
        End Set
    End Property

    ''' <summary>
    ''' Chooses the given option within this conflict.
    ''' </summary>
    ''' <param name="opt">The option to choose.</param>
    ''' <returns>The data handler which should be used for additional data required
    ''' from the user after choosing the given option.</returns>
    ''' <exception cref="BluePrismException">If the given option is not an option
    ''' defined in the definition held by this conflict.</exception>
    Public Function ChooseOption(ByVal opt As ConflictOption) As ConflictDataHandler
        ' From nothing we came; To nothing we return
        If opt Is Nothing Then mOption = Nothing : Return Nothing

        ' Get the option from our definition and check that they are (reference) equal
        Dim copt As ConflictOption = mDefn(opt.Choice)
        If opt IsNot copt Then Throw New BluePrismException( _
         "Option: {0} doesn't belong to conflict: {1}", opt, Me)

        ' Set the memvar
        mOption = opt

        ' Get the handler for this option. This is cached inside this conflict, since
        ' the component data won't change, thus the need for a different handler will
        ' not come about. First time, however, delegate the decision about the handler
        ' the component.
        Dim handler As ConflictDataHandler = Nothing
        If Not mHandlers.TryGetValue(opt, handler) Then ' Not cached
            handler = mComponent.GetHandlerForOption(Me, opt) ' Get it from the component
            mHandlers(opt) = handler ' And cache it
        End If
        Return handler

    End Function

End Class
