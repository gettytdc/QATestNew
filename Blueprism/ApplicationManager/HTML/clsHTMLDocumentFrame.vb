''' <summary>
''' This class represents a frame or iframe embedded in a browser. 
''' The embedded component is effectivley a complete document itself
''' </summary>
''' <remarks></remarks>
Public Class clsHTMLDocumentFrame
    Inherits clsHTMLDocument

    Private mFrameElement As clsHTMLElement

    ''' <summary>
    ''' The constructor of clsHTMLDocuementFrame is only accessible from this assembly because 
    ''' it should only be created by the parent clsHTMLDocument and is accessed through 
    ''' clsHTMLDocument.Frames
    ''' </summary>
    Friend Sub New(ByVal objDocument As mshtml.IHTMLDocument2, ByVal FrameElement As clsHTMLElement)
        MyBase.New(objDocument, IntPtr.Zero)
        mFrameElement = FrameElement
    End Sub


    ''' <summary>
    ''' This is simply a helper function to get the offset of the frame 
    ''' in the elementfrompoint calculation in clsHTMLDocument.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property FrameBounds() As Drawing.Rectangle
        Get
            Return MyBase.GetAbsoluteBounds(mFrameElement)
        End Get
    End Property

    ''' <summary>
    ''' The element that represents this frame
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property FrameElement() As clsHTMLElement
        Get
            Return mFrameElement
        End Get
    End Property

End Class
