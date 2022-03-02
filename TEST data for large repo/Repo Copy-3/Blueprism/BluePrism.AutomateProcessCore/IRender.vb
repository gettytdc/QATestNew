Imports System.Drawing

Public Enum RenderDevice
    Screen
    File
End Enum

Public Interface IRender

    ''' <summary>
    ''' Implementation should set this property so that the drawing can be tweaked
    ''' depending on the device it is targeted to be rendered on.
    ''' </summary>
    ReadOnly Property RenderingDevice As RenderDevice

    ''' <summary>
    ''' Implementation should draw a rectangle outline with a boundry defined by
    ''' the given RectangleF
    ''' </summary>
    ''' <param name="rectangle">The boundry</param>
    Sub FillRectangle(ByVal rectangle As RectangleF)

    ''' <summary>
    ''' Implementation should fill a rectangle with a boundry defined by the
    ''' given RectangleF
    ''' </summary>
    ''' <param name="rectangle">The boundry</param>
    Sub DrawRectangleF(ByVal rectangle As RectangleF)

    ''' <summary>
    ''' Implementation should fill a path with a boundry defined by the given
    ''' graphics path
    ''' </summary>
    ''' <param name="path">The graphics path</param>
    Sub FillPath(ByVal path As Drawing2D.GraphicsPath)

    ''' <summary>
    ''' Implementation should draw a path outline with a boundy defined by the
    ''' given graphics path
    ''' </summary>
    ''' <param name="path">The graphics path</param>
    Sub DrawPath(ByVal path As Drawing2D.GraphicsPath)

    ''' <summary>
    ''' Implementation should fill a polygon with the boundry defined by the
    ''' array of points
    ''' </summary>
    ''' <param name="points">The array of points</param>
    Sub FillPolygon(ByVal points() As PointF)

    ''' <summary>
    ''' Implementation should draw a polygon outline with the boundry defined by
    ''' the array of points
    ''' </summary>
    ''' <param name="points">The array of points</param>
    Sub DrawPolygon(ByVal points() As PointF)

    ''' <summary>
    ''' Implementation should draw a line start and finish defined by the given
    ''' points
    ''' </summary>
    ''' <param name="start">The start point</param>
    ''' <param name="finish">The finish point</param>
    Sub DrawLine(ByVal start As PointF, ByVal finish As PointF)

    ''' <summary>
    ''' Implementation should fill a circle defined by the given location and
    ''' radius
    ''' </summary>
    ''' <param name="left">The leftmost position of the circle</param>
    ''' <param name="top">The topmost position of the circle</param>
    ''' <param name="radius">The radius</param>
    Sub FillCircle(ByVal left As Single, ByVal top As Single, ByVal radius As Single)

    ''' <summary>
    ''' Implementation should draw a circle outline defined by the given location
    ''' and radius
    ''' </summary>
    ''' <param name="left">The leftmost position of the circle</param>
    ''' <param name="top">The topmost position of the circle</param>
    ''' <param name="radius">The radius</param>
    Sub DrawCircle(ByVal left As Single, ByVal top As Single, ByVal radius As Single)

    ''' <summary>
    ''' Implementation should fill an ellipse defined by the given location and
    ''' boundry rectangle
    ''' </summary>
    ''' <param name="rectangle">The boundry</param>
    Sub FillEllipse(ByVal rectangle As RectangleF)

    ''' <summary>
    ''' Implementation should draw an ellipse outline defined by the given
    ''' location and boundry rectangle
    ''' </summary>
    ''' <param name="rectangle">The boundry</param>
    Sub DrawEllipse(ByVal rectangle As RectangleF)

    ''' <summary>
    ''' Implementation should draw the given text in the given boundry
    ''' </summary>
    ''' <param name="text">The text</param>
    ''' <param name="bounds">The boundry</param>
    Sub DrawText(ByVal text As String, ByVal bounds As RectangleF)

    ''' <summary>
    ''' Implementation should set the current pen to the given color
    ''' </summary>
    ''' <param name="color">The color</param>
    Sub SetBrushColor(color As Color)

    ''' <summary>
    ''' Implementation should set the dash style of the current pen
    ''' </summary>
    ''' <param name="style">The dash style</param>
    Sub SetPenStyle(style As Drawing2D.DashStyle)

End Interface
