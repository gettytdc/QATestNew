Imports System.Xml
Imports System.Drawing
Imports BluePrism.Core.Xml

''' <summary>
''' Possible process modes.
'''
''' Note that these must all have descriptions defined in clsAMI.
''' </summary>
Public Enum ProcessMode
    Internal        'Internal, within same process as caller
    ExtSameArch     'External, same architecture as caller
    ExtNativeArch   'External, native architecture
    Ext32bit        'External, 32 bit
    Ext64bit        'External, 64 bit
    Citrix32        'External, Citrix 32 bit
    Citrix64        'External, Citrix 64 bit
End Enum

Public MustInherit Class clsTargetApp
    Implements IDisposable

    ' True when we are connected to the target application; False otherwise
    Private mConnected As Boolean

    ''' <summary>
    ''' Flag indicating whether this object is connected to the target application.
    ''' </summary>
    Public Property Connected() As Boolean
        Get
            Return mConnected
        End Get
        Protected Set(ByVal value As Boolean)
            mConnected = value
        End Set
    End Property

    ''' <summary>
    ''' Get a clsTargetApp instance.
    ''' </summary>
    ''' <param name="mode">The required process mode.</param>
    ''' <returns>The new instance, which may be a clsLocalTargetApp or a
    ''' clsExternalTargetApp according to the mode requested. An external version
    ''' will already be running and connected.</returns>
    Public Shared Function GetTargetApp(mode As ProcessMode) As clsTargetApp

        Select Case mode
            Case ProcessMode.Internal
                Return New clsLocalTargetApp()
            Case ProcessMode.Citrix32, ProcessMode.Citrix64
                Return New clsExternalTargetAppCitrix(mode)
            Case Else
                Return New clsExternalTargetAppLocal(mode)
        End Select

    End Function


    ''' <summary>
    ''' Get the PID of the target application currently connected.
    ''' </summary>
    Public MustOverride ReadOnly Property PID() As Integer

    ''' <summary>
    ''' Event raised when a line of information is received from the target
    ''' application. This is handled internally, and only exposed here for
    ''' debug purposes - i.e. it is required for HookTestClient to be able to
    ''' display/log incoming data. It should not be handled in a production
    ''' application, and it will never be raised by anything except a Local
    ''' target application.
    ''' </summary>
    ''' <param name="line">The line received</param>
    Public Event LineReceived(ByVal line As String)
    'Used by derived classes to raise the event...
    Protected Sub OnLineReceived(ByVal line As String)
        RaiseEvent LineReceived(line)
    End Sub

    ''' <summary>
    ''' Event raised when the target application is disconnected.
    ''' </summary>
    ''' <remarks>A disconnected application is no longer available for interaction,
    ''' unless some further action is taken (such as attaching to the instance left
    ''' running).
    ''' 
    ''' Note that this event must be handled in a thread-safe manner: multiple
    ''' threads can raise this event.</remarks>
    Public Event Disconnected()
    'Used by derived classes to raise the event...
    Public Sub OnDisconnected()
        mConnected = False
        RaiseEvent Disconnected()
    End Sub

    ''' <summary>
    ''' Event raised when the target application is disconnected.
    ''' </summary>
    ''' <remarks>A disconnected application is no longer available for interaction,
    ''' unless some further action is taken (such as attaching to the instance left
    ''' running).
    ''' 
    ''' Note that this event must be handled in a thread-safe manner: multiple
    ''' threads can raise this event.</remarks>
    Public Event ExpectDisconnect()
    'Used by derived classes to raise the event...
    Protected Sub OnExpectDisconnect()
        RaiseEvent ExpectDisconnect()
    End Sub

    ''' <summary>
    ''' Process a query.
    ''' </summary>
    ''' <param name="sQuery">The query to process.</param>
    ''' <returns>The result of the query.</returns>
    ''' <remarks>Low-level logging is performed at this stage if requested.</remarks>
    Public MustOverride Function ProcessQuery(ByVal sQuery As String) As String

    ''' <summary>
    ''' Process a query.
    ''' </summary>
    ''' <param name="sQuery">The query to process.</param>
    ''' <param name="timeout">Length of time before the query times out.</param>
    ''' <returns>The result of the query.</returns>
    ''' <remarks>Low-level logging is performed at this stage if requested.</remarks>
    Public MustOverride Function ProcessQuery(ByVal sQuery As String,
                                              timeout As TimeSpan) As String

    ' Flag keeping track of whether this object has been disposed or not
    Private mIsDisposed As Boolean

    ''' <summary>
    ''' Flag indicating if this target app object has been disposed or not.
    ''' </summary>
    Protected ReadOnly Property IsDisposed() As Boolean
        Get
            Return mIsDisposed
        End Get
    End Property

    ''' <summary>
    ''' Disposes of this target application
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    ''' <summary>
    ''' Disposes of this target application, explicitly or otherwise as specified
    ''' </summary>
    ''' <param name="disposingExplicitly">True to indicate that this method is being called
    ''' explicitly, ie. by managed code; False to indicate that it is being called
    ''' as part of an object finalizer.</param>
    Protected Overridable Sub Dispose(ByVal disposingExplicitly As Boolean)
        If mIsDisposed Then Return
        mIsDisposed = True
    End Sub

    ''' <summary>
    ''' Finalizes this object, ensuring that it is disposed of correctly.
    ''' </summary>
    Protected Overrides Sub Finalize()
        Dispose(False)
    End Sub


    ''' <summary>
    ''' Takes the given collection XML and extracts the rectangle data from it
    ''' </summary>
    ''' <param name="xml">The XML describing the collection holding the rectangle.
    ''' </param>
    ''' <returns>A rectangle containing the value found in the given XML</returns>
    Public Shared Function CreateRectangleFromCollectionXML(ByVal xml As String) _
     As Rectangle
        Dim doc As New ReadableXmlDocument(xml)
        Dim r As New Rectangle()
        For Each fld As XmlElement In doc.GetElementsByTagName("field")
            Dim value As Integer = 0
            If Not Integer.TryParse(fld.GetAttribute("value"), value) Then _
             Continue For

            Select Case fld.GetAttribute("name")
                Case "Left" : r.X = value
                Case "Top" : r.Y = value
                Case "Width" : r.Width = value
                Case "Height" : r.Height = value
            End Select
        Next

        Return r

    End Function


    ''' <summary>
    ''' Creates an xml string representing an automate collection, containing
    ''' information about the bounds of the supplied rectangle.
    ''' </summary>
    ''' <param name="r">The rectangle of interest.</param>
    ''' <returns>Returns Automate collection xml containing info about the bounds
    ''' of the supplied rectangle. Contains fields left, top, right, bottom, width,
    ''' height.</returns>
    Public Shared Function CreateCollectionXMLFromRectangle(ByVal r As Rectangle) As String
        Dim doc As New XmlDocument()
        Dim root As XmlElement = doc.CreateElement("collection")
        Dim row As XmlElement = doc.CreateElement("row")

        row.AppendChild(CreateCollectionFieldXML(doc, r.Left, "Left"))
        row.AppendChild(CreateCollectionFieldXML(doc, r.Top, "Top"))
        row.AppendChild(CreateCollectionFieldXML(doc, r.Bottom, "Bottom"))
        row.AppendChild(CreateCollectionFieldXML(doc, r.Right, "Right"))
        row.AppendChild(CreateCollectionFieldXML(doc, r.Width, "Width"))
        row.AppendChild(CreateCollectionFieldXML(doc, r.Height, "Height"))

        root.AppendChild(row)
        doc.AppendChild(root)
        Return doc.OuterXml
    End Function

    ''' <summary>
    ''' Create the XML element for an integer collection field.
    ''' </summary>
    ''' <param name="doc">The parent XmlDocument</param>
    ''' <param name="val">The value of the field</param>
    ''' <param name="name">The name of the column</param>
    ''' <returns>The new XmlElement</returns>
    Protected Shared Function CreateCollectionFieldXML(ByVal doc As XmlDocument, _
     ByVal val As Integer, ByVal name As String) As XmlElement
        Return CreateCollectionFieldXML(doc, val.ToString(), "number", name)
    End Function

    ''' <summary>
    ''' Create the XML element for a collection field.
    ''' </summary>
    ''' <param name="doc">The parent XmlDocument</param>
    ''' <param name="val">The value of the field</param>
    ''' <param name="dataType">The Automate Data Type of the field, e.g. "text"</param>
    ''' <param name="name">The name of the column</param>
    ''' <returns>The new XmlElement</returns>
    Protected Shared Function CreateCollectionFieldXML(ByVal doc As XmlDocument, _
     ByVal val As String, ByVal dataType As String, ByVal name As String) As XmlElement
        Dim el As XmlElement = doc.CreateElement("field")
        el.SetAttribute("type", dataType)
        el.SetAttribute("value", val)
        el.SetAttribute("name", name)
        Return el
    End Function

End Class
