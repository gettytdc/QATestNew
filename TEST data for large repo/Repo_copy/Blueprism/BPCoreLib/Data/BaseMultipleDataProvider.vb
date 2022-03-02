Imports BluePrism.Server.Domain.Models

Namespace Data

    ''' <summary>
    ''' Base class for a multiple data provider. This keeps track of whether there
    ''' is any data left in the provider or not and handles the raising of any
    ''' exceptions if there were not.
    ''' </summary>
    Public MustInherit Class BaseMultipleDataProvider
        Inherits BaseDataProvider : Implements IMultipleDataProvider

        ' The state of this data provider
        Private mState As DataTraversalState

        ''' <summary>
        ''' Creates a new multiple data provider, starting in a traversal state of
        ''' <see cref="DataTraversalState.BeforeStart"/>
        ''' </summary>
        Protected Sub New()
            Me.New(DataTraversalState.BeforeStart)
        End Sub

        ''' <summary>
        ''' Creates a new multiple data provider, starting in the given initial state
        ''' </summary>
        ''' <param name="initialState">The state to use initially for the traverser
        ''' of data in this object.</param>
        Protected Sub New(ByVal initialState As DataTraversalState)
            mState = initialState
        End Sub

        ''' <summary>
        ''' The current state of this data provider.
        ''' </summary>
        Protected Property State() As DataTraversalState
            Get
                Return mState
            End Get
            Set(ByVal value As DataTraversalState)
                mState = DataTraversalState.BeforeStart
            End Set
        End Property

        ''' <summary>
        ''' Moves to the next row of data in the provider, returning a boolean value
        ''' indicating whether there is more data to be provided.
        ''' </summary>
        ''' <returns>True if the provider was moved and there is more data available,
        ''' false if there is no more data available.</returns>
        Public Overridable Function MoveNext() As Boolean _
         Implements IMultipleDataProvider.MoveNext
            If mState = DataTraversalState.AfterEnd Then Return False
            If InnerMoveNext() Then mState = DataTraversalState.InData : Return True
            mState = DataTraversalState.AfterEnd
            Return False
        End Function

        ''' <summary>
        ''' Gets the data item with the given name or 'Nothing' if that item
        ''' did not exist within this provider.
        ''' </summary>
        ''' <param name="name">The name of the data item required.</param>
        Default Public Overrides ReadOnly Property Item(ByVal name As String) _
         As Object
            Get
                If mState = DataTraversalState.BeforeStart Then Throw New _
                 InvalidStateException(My.Resources.BaseMultipleDataProvider_ProviderIsBeforeTheStartOfTheData)
                If mState = DataTraversalState.AfterEnd Then Throw New _
                 InvalidStateException(My.Resources.BaseMultipleDataProvider_ProviderIsAfterTheEndOfTheData)
                Return Normalise(GetItem(name))
            End Get
        End Property

        ''' <summary>
        ''' Attempts to move to the next row of data in this provider.
        ''' </summary>
        ''' <returns>True if the provider has moved to a row containing data; False
        ''' if there is no more data to traverse.</returns>
        Protected MustOverride Function InnerMoveNext() As Boolean

        ''' <summary>
        ''' Gets the item value corresponding to the given name.
        ''' </summary>
        ''' <param name="name">The name of the item for which the value is required.
        ''' </param>
        ''' <returns>The object corresponding to the given name, or null if there was
        ''' no item with the given name in this provider</returns>
        ''' <remarks>The state of the provider is checked before this method is
        ''' called, so that subclasses do not need to check if the provider is
        ''' currently before or after the data. This method should only be called
        ''' when the provider has previously reported that data is available by use
        ''' of the <see cref="InnerMoveNext"/> method.</remarks>
        Protected MustOverride Function GetItem(ByVal name As String) As Object

    End Class

End Namespace
