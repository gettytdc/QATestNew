﻿Imports System.Linq
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.Utilities.Functional
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    ''' <summary>
    ''' Adds the first item with the given name to the list's selected items.
    ''' Required parameters: Those required to uniquely identify the element plus
    ''' 'idname' identifying the item to add.
    ''' </summary>
    ''' <seealso cref="UIAutomationHandlerBase" />
    <CommandId("UIAListAddToSelection")>
    Friend Class ListAddToSelectionHandler : Inherits UIAutomationHandlerBase

        ''' <summary>
        ''' The automation factory
        ''' </summary>
        Private ReadOnly mAutomationFactory As IAutomationFactory

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ListAddToSelectionHandler" /> class.
        ''' </summary>
        ''' <param name="application">The application.</param>
        ''' <param name="identifierHelper">The identifier helper.</param>
        ''' <param name="automationFactory">The automation factory.</param>
        Public Sub New(
                      application As ILocalTargetApp,
                      identifierHelper As IUIAutomationIdentifierHelper,
                      automationFactory As IAutomationFactory)
            MyBase.New(application, identifierHelper)

            mAutomationFactory = automationFactory
        End Sub

        '''<inheritDoc />
        Protected Overrides Function Execute(element As IAutomationElement, context As CommandContext) As Reply

            Dim listItems = GetListItems(element).ToList()

            GetListItem(listItems, context.Query).
                Map(Function(x) x.GetCurrentPattern(Of ISelectionItemPattern))?.
                AddToSelection()

            Return Reply.Ok

        End Function
    End Class

End Namespace