Imports AutomateControls

''' Project  : Automate
''' Interface    : IProcessViewingForm
''' 
''' <summary>
''' This interface is designed for forms that display processes (eg frmProcess
''' and frmProcessComparison). It allows for communication from the process
''' viewing control to the parent form (eg to display messages on the form's
''' status bar and to update the form's menus etc).
''' </summary>
Friend Interface IProcessViewingForm : Inherits IEnvironmentColourManager

    ''' <summary>
    ''' The default time in milliseconds that messages displayed on the status bar should 
    ''' remain before being removed.
    ''' </summary>
    ''' <value></value>
    Property DefaultStatusBarMessageDuration() As Integer

    ''' <summary>
    ''' Sets the text on the form's status bar.
    ''' </summary>
    ''' <param name="sMessage">The message to be displayed.</param>
    ''' <param name="iMessageDuration">The number of milliseconds that the message
    ''' is to be displayed for. Set to zero to display indefinitely (or at least
    ''' until it is replaced by another). Must be a multiple of 100 milliseconds;
    ''' if not it will be rounded up to the next multiple.</param>
    Sub SetStatusBarText(ByVal sMessage As String, ByVal iMessageDuration As Integer)

    ''' <summary>
    ''' Allows for messages to be displayed without thinking about the time that the
    ''' message will be displayed for. Uses the DefaultStatusBarMessageDuration
    ''' property to determine the time.
    ''' </summary>
    ''' <param name="sMessage">The message to be displayed.</param>
    Sub SetStatusBarText(ByVal sMessage As String)

    ''' <summary>
    ''' Clears the text on the statusbar.
    ''' </summary>
    Sub ClearStatusBarText()


    ''' <summary>
    ''' Sets the updates the zoomlevel
    ''' </summary>
    ''' <param name="percent">The new zoom level for the active subsheet in a process
    ''' in the form of a percentage</param>
    Sub ZoomUpdate(ByVal percent As Integer)

    ''' <summary>
    ''' Causes the supplied search control to be hidden.
    ''' </summary>
    ''' <param name="SearchControl">The search control to hide.</param>
    ''' <remarks></remarks>
    Sub HideSearchControl(ByVal SearchControl As DiagramSearchToolstrip)

End Interface
