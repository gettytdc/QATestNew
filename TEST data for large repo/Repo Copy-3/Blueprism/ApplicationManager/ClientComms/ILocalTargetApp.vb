Imports BluePrism.ApplicationManager.ClientCommsI

''' <summary>
''' Defines functionality available in target application
''' </summary>
Friend Interface ILocalTargetApp : Inherits ITargetApp

    ''' <summary>
    ''' The application model targeted by this application
    ''' </summary>
    ReadOnly Property Model() As clsUIModel

    ''' <summary>
    ''' The UIAutomation ID helper for the target app
    ''' </summary>
    ReadOnly Property UIAutomationIdHelper As IUIAutomationIdentifierHelper
End Interface