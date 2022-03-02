Imports System.Reflection
Imports System.IO
Imports BluePrism.AutomateAppCore
Imports BluePrism.BPCoreLib
Imports BluePrism.Core.Utility

Public Class clsUserInterfaceUtils

    ''' <summary>
    ''' Helper method to set either the provided <paramref name="editableControl">
    ''' editable control</paramref> or the <paramref name="readonlyControl">
    ''' read-only control</paramref> to be visible depending on the state of the
    ''' <paramref name="ro"/> flag.
    ''' </summary>
    ''' <param name="ro">True to show the read-only control; False to show the
    ''' editable control.</param>
    ''' <param name="editableControl">The control to show if we are not setting
    ''' this to be read-only. Also acts as the source of the text to show in the
    ''' read-only control</param>
    ''' <param name="readonlyControl">The control to show if we are setting this
    ''' to be read-only. Has its Text property set from the corresponding Text
    ''' property on the editable control before it is shown / hidden.</param>
    Public Shared Sub ShowReadOnlyControl(ByVal ro As Boolean, _
     ByVal editableControl As Control, ByVal readonlyControl As Control)

        readonlyControl.Text = editableControl.Text
        editableControl.Visible = Not ro
        readonlyControl.Visible = ro

    End Sub

    ''' <summary>
    ''' Gets the ancestor of this control which is of type T, will return nothing if 
    ''' the ancestor cannot be found
    ''' </summary>
    ''' <typeparam name="T">The type of ancestor to find</typeparam>
    ''' <param name="ctl">The control for which to find the ancestor for</param>
    ''' <returns>The first ancestor found of the matching type, or null if no such
    ''' ancestor was found in the given controls ancestry tree.</returns>
    Public Shared Function GetAncestor(Of T As Control)(ByVal ctl As Control) As T
        Dim parent As Control = ctl.Parent
        While (parent IsNot Nothing)
            Dim ancestor As T = TryCast(parent, T)
            If Not ancestor Is Nothing Then
                Return ancestor
            End If
            parent = parent.Parent
        End While
        Return Nothing
    End Function

#Region "InvokeRequired helpers"

    ''' <summary>
    ''' Very generic delegate which sets a value on a control.
    ''' </summary>
    ''' <param name="ctl">The control to set a value on.</param>
    Private Delegate Sub ControlSetter(ByVal ctl As Control, ByVal value As Object)

    ''' <summary>
    ''' Generic delegate to get a value from a control
    ''' </summary>
    ''' <param name="ctl">The control from which the value should be retrieved.
    ''' </param>
    ''' <returns>The value from the control</returns>
    Private Delegate Function ControlGetter(ByVal ctl As Control) As Object

    ''' <summary>
    ''' Generic delegate to perform an operation on a control
    ''' </summary>
    ''' <param name="ctl">The control on which the operation should be performed.
    ''' </param>
    Private Delegate Sub ControlOperation(ByVal ctl As Control)

    ''' <summary>
    '''  Generic delegate with which to get a property.
    ''' </summary>
    ''' <typeparam name="T">The type of the property to get.</typeparam>
    ''' <returns>The value of the property</returns>
    Private Delegate Function GetPropertyDelegate(Of T)() As T

    ''' <summary>
    ''' Generic delegate to set a property.
    ''' </summary>
    ''' <typeparam name="T">The type of the property to set</typeparam>
    ''' <param name="value">The value to which the property should be set.
    ''' </param>
    Private Delegate Sub SetPropertyDelegate(Of T)(ByVal value As T)

    ''' <summary>
    ''' Gets the named property of the control and returns the value, ensuring
    ''' that the property is retrieved on the required UI thread as necessary.
    ''' </summary>
    ''' <typeparam name="T">The type of property required.</typeparam>
    ''' <param name="ctl">The control on which the property is required.</param>
    ''' <param name="name">The name of the required property</param>
    ''' <returns>The value of the given property</returns>
    Public Shared Function GetProperty(Of T)(ByVal ctl As Control, ByVal name As String) As T

        Dim prop As PropertyInfo = ctl.GetType().GetProperty(name)
        Dim mi As MethodInfo = prop.GetGetMethod()

        If ctl.InvokeRequired Then
            ' Create a delegate to get the property
            Dim getProp As GetPropertyDelegate(Of T) = DirectCast( _
             [Delegate].CreateDelegate(GetType(GetPropertyDelegate(Of T)), ctl, mi),  _
             GetPropertyDelegate(Of T))

            Return DirectCast(ctl.Invoke(getProp), T)
        Else
            Return DirectCast(mi.Invoke(ctl, Nothing), T)
        End If

    End Function

    ''' <summary>
    ''' Sets the named property of the control to the given value, ensuring
    ''' that the property is set on the required UI thread.
    ''' </summary>
    ''' <typeparam name="T">The type of property that the name represents.
    ''' </typeparam>
    ''' <param name="ctl">The control on which the property should be set.
    ''' </param>
    ''' <param name="name">The name of the property to set.</param>
    ''' <param name="value">The value to set the property to</param>
    Public Shared Sub SetProperty(Of T)(ByVal ctl As Control, ByVal name As String, ByVal value As T)

        Dim prop As PropertyInfo = ctl.GetType().GetProperty(name)
        Dim mi As MethodInfo = prop.GetSetMethod()

        If ctl.InvokeRequired Then
            ' Create a delegate to get the property
            Dim setProp As SetPropertyDelegate(Of T) = DirectCast( _
             [Delegate].CreateDelegate(GetType(SetPropertyDelegate(Of T)), ctl, mi),  _
             SetPropertyDelegate(Of T))

            ctl.Invoke(setProp, value)
        Else
            mi.Invoke(ctl, New Object() {value})
        End If

    End Sub

    ''' <summary>
    ''' Gets the text property of the given control, ensuring that the property
    ''' is accessed from the thread that created the control's handle.
    ''' </summary>
    ''' <param name="ctl">The control for which the text property is required.
    ''' </param>
    ''' <returns>The text value on the control.</returns>
    Public Shared Function GetText(ByVal ctl As Control) As String
        Return GetProperty(Of String)(ctl, "Text")
    End Function

    ''' <summary>
    ''' Gets the selected item from the given combo box or listview, ensuring
    ''' that the property is retrieved on the correct thread.
    ''' If more than one selection is made in the listview, the first selection
    ''' is returned. If the given control is not a combo box or a list view, or
    ''' it is a listview with no selected items, this will return null.
    ''' </summary>
    ''' <param name="ctl">The combo box from which the selected item is
    ''' required.</param>
    ''' <returns>The currently selected item in the combo box.</returns>
    Public Shared Function GetSelectedItem(ByVal ctl As Control) As Object
        If ctl.InvokeRequired Then
            Return ctl.Invoke(New ControlGetter(AddressOf GetSelectedItem), ctl)
        Else
            Dim combo As ComboBox = TryCast(ctl, ComboBox)
            If combo IsNot Nothing Then Return combo.SelectedItem

            Dim listview As ListView = TryCast(ctl, ListView)
            If listview IsNot Nothing AndAlso listview.SelectedItems.Count > 0 Then
                Return listview.SelectedItems(0)
            End If

            Return Nothing
        End If
    End Function

    ''' <summary>
    ''' Sets the focus on the supplied control, if it is visible.
    ''' Note that if it is not visible, the request will be silently ignored.
    ''' </summary>
    ''' <param name="ctl">The control on which the focus should be set.</param>
    Public Shared Sub SetFocus(ByVal ctl As Control)
        If ctl Is Nothing OrElse Not ctl.Visible Then Return
        If ctl.InvokeRequired Then
            ctl.Invoke(New ControlOperation(AddressOf SetFocus), ctl)
        Else
            ctl.Focus()
        End If
    End Sub

    ''' <summary>
    ''' Sets the focus on the given control after any other current UI actions
    ''' have been performed in the UI thread.
    ''' This works around a problem where setting the focus in certain events
    ''' causes the focus to be grabbed straight back again. TreeViews do this
    ''' quite a bit - see :-
    ''' http://social.msdn.microsoft.com/forums/en-US/winforms/thread/5ed8754a-1822-42f5-93b3-a1b196e180a1/
    ''' It's used in ctlScheduleManager and in ctlApplicationExplorer at the
    ''' time of writing
    ''' </summary>
    ''' <param name="ctl">The control to set focus on - if it is null or not
    ''' visible, this call has no effect. Otherwise, a focus request is made
    ''' asynchronously, ensuring it occurs after current actions pending in the
    ''' UI thread affected.</param>
    Public Shared Sub SetFocusDelayed(ByVal ctl As Control)
        If ctl Is Nothing OrElse Not ctl.Visible OrElse Not ctl.IsHandleCreated Then Return
        ctl.BeginInvoke(New ControlOperation(AddressOf SetFocus), ctl)
    End Sub

#End Region


    ''' <summary>
    ''' Shows a generic message to the user, suitable for when
    ''' they have requsted an action from the gui which is disallowed
    ''' by the license.
    ''' </summary>
    ''' <param name="ErrorInfo">Specific information about why the operation
    ''' is disallowed</param>
    ''' <remarks>See also GetOperationDisallowedMessage, which retrieves the
    ''' same message instead of showing it to the user. </remarks>
    Public Shared Sub ShowOperationDisallowedMessage(Optional ByVal ErrorInfo As String = "")
        UserMessage.Show(Licensing.GetOperationDisallowedMessage(), -1,
                         "helpLicensing.htm")
    End Sub

    ''' <summary>
    ''' Takes each column supplied and autosizes the width such that it
    ''' accommodates both the width of the columns header text and it accommodates
    ''' the width of all text it contains. This can be made subject to a maximum size
    ''' if desired.
    ''' 
    ''' Automatically takes into account whether you will be sorting the columns
    ''' or not. The framework does not do this for you if you use .width = -2. Tip:
    ''' to get better resizing, add your column sorter before you call this method.
    ''' </summary>
    ''' <param name="listView">The ListView for wich columns are to be autosized.</param>
    ''' <param name="MaxWidth">Optional. The maximum width to accept in any column. 
    ''' If not supplied Integer.MaxValue will be the used as the maximum</param>
    ''' <param name="allRemainingCol">If specified, this column (0-n) gets all the
    ''' remaining width of the ListView after all the other columns have been
    ''' appropriately sized. (But never less than it ought to get!)</param>
    Public Shared Sub AutoAdjustListViewColumnWidths(ByVal listView As ListView, Optional ByVal MaxWidth As Integer = Integer.MaxValue, Optional ByVal allRemainingCol As Integer = -1)

        If listView.Columns.Count = 0 Then Return

        'when the columns are sortable, the text is moved over to accommodate the
        'triangular arrow. Here we estimate the amount by which we need to compensate
        Dim AddedWidthOfSortingArrowsOnColumnHeader As Integer = 0
        If Not listView.ListViewItemSorter Is Nothing Then
            AddedWidthOfSortingArrowsOnColumnHeader = 25
        End If

        Dim widthLeft As Integer = listView.Width
        For Each ch As ColumnHeader In listView.Columns

            Dim miSetColumnWidth As System.Reflection.MethodInfo = GetType(ListView).GetMethod("SetColumnWidth", Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic, Nothing, New Type() {GetType(Integer), GetType(ColumnHeaderAutoResizeStyle)}, Nothing)
            If miSetColumnWidth IsNot Nothing Then
                If listView.Items.Count > 0 AndAlso listView.Items(0) IsNot Nothing Then
                    'Temporarily set the column width to be autosized to the width of the column content
                    miSetColumnWidth.Invoke(listView, New Object() {ch.Index, ColumnHeaderAutoResizeStyle.ColumnContent})

                    '...and store that autocalculated width
                    Dim ColumnWidth As Integer = ch.Width + AddedWidthOfSortingArrowsOnColumnHeader

                    'Now temporarily set the column width to be autosized to the width of the column header content
                    miSetColumnWidth.Invoke(listView, New Object() {ch.Index, ColumnHeaderAutoResizeStyle.HeaderSize})

                    '...and store that autocalculated width
                    Dim HeaderWidth As Integer = ch.Width

                    'The new width should be whichever of those two values is bigger
                    Dim NewWidth As Integer = Math.Max(HeaderWidth, ColumnWidth)

                    '...but must not exceed MaxWidth
                    ch.Width = Math.Min(NewWidth, MaxWidth)
                    widthLeft -= ch.Width
                End If
            End If

        Next

        If allRemainingCol <> -1 AndAlso widthLeft > 0 Then
            Dim ch As ColumnHeader = listView.Columns(allRemainingCol)
            ch.Width += widthLeft
        End If

    End Sub

    ''' <summary>
    ''' Shows an html document to the user, using the system default application for
    ''' the .html extension.
    ''' </summary>
    ''' <param name="HTML">The full html string of the document, including all
    ''' document type declarations, headers, etc.</param>
    ''' <param name="FileName">The name of the file used to store the document on
    ''' disk, whilst in a temporary directory.</param>
    ''' <param name="URLSuffix">The named element within the document to be included
    ''' as part of the url. Eg http://MyURL.htm#Suffix
    ''' The # symbol will be included automatically; do not add it manually.</param>
    ''' <returns>Returns True on success, False in the event of an error.</returns>
    ''' <remarks>The standard BPA css stylesheet will also be written to the same
    ''' directory, and may be referenced with a relative file path from within your
    ''' document.</remarks>
    Public Shared Function ShowHTMLDocument(ByVal HTML As String, ByVal FileName As String, Optional ByVal URLSuffix As String = "") As Boolean
        Try

            Dim destdir As String = clsFileSystem.TempDirectory

            'Choose a unique filename
            Dim htmlFile = BPUtil.FindUnique(
                Path.Combine(destdir, FileName & "-{0}.html"),
                Function(f) File.Exists(f))

            'Create directory if necessary
            If Not Directory.Exists(destdir) Then Directory.CreateDirectory(destdir)

            'write out style sheet
            Using sw As New StreamWriter(Path.Combine(destdir, "AutomateHelp.css"), False)
                sw.Write(My.Resources.AutomateHelp_CSS)
                sw.Flush()
            End Using

            'Write document to disk
            Using sw As New StreamWriter(htmlFile, False)
                sw.Write(HTML)
                sw.Flush()
            End Using

            'Create and show internet explorer object with supplied document
            Dim url As String = String.Format(
                "file:///{0}#{1}", htmlFile.Replace("\"c, "/"), URLSuffix)
            ExternalBrowser.OpenUrl(url)
            Return True

        Catch ex As Exception
            UserMessage.Err(ex,
             String.Format(My.Resources.ErrorWhilstWritingDocumentToTemporaryDirectory0, ex.Message))
            Return False
        End Try
    End Function

#Region "Graphics Utilities"

    ''' Project  : Automate
    ''' Class    : clsUtility.GraphicsUtils
    ''' 
    ''' <summary>
    ''' A graphics utility class.
    ''' </summary>
    Public Class GraphicsUtils

#Region "Public Class PseudoColour"

        ''' Project  : Automate
        ''' Class    : clsUtility.GraphicsUtils.PseudoColor
        ''' 
        ''' <summary>
        ''' This class facilitates arithmetic operations on colours by allowing RGB 
        ''' components to take on negative (and otherwise out of range) values.
        ''' </summary>
        Public Class PseudoColour

            'we make these doubles so that we can divide them etc.
            Public R As Double
            Public G As Double
            Public B As Double
            Public A As Double

            Public Sub New(ByVal A As Double, ByVal R As Double, ByVal G As Double, ByVal B As Double)
                Me.A = A
                Me.R = R
                Me.G = G
                Me.B = B
            End Sub

            Public Sub New(ByVal R As Double, ByVal G As Double, ByVal B As Double)
                Me.New(255, R, G, B)
            End Sub

            ''' <summary>
            ''' Creates a color object from the class instance. Reduces RGB components mod 256
            ''' and takes rounded absolute value before creating color..
            ''' </summary>
            ''' <returns>Return Color.FromArgb(Math.Floor(Math.Abs(A Mod 256)), Math.Floor(Math.Abs(R Mod 256)), Math.Floor(Math.Abs(G Mod 256)), Math.Floor(Math.Abs(B Mod 256)))</returns>
            Public Function ToColor() As Color
                Return Color.FromArgb(CInt(Math.Floor(Math.Abs(A Mod 256))), CInt(Math.Floor(Math.Abs(R Mod 256))), CInt(Math.Floor(Math.Abs(G Mod 256))), CInt(Math.Floor(Math.Abs(B Mod 256))))
            End Function

            ''' <summary>
            ''' copies the ARGB values from the provided color into a new pseudocolor object.
            ''' </summary>
            ''' <param name="c"></param>
            ''' <returns>Returns a pseudocolour object with RGB values matching that of the
            ''' supplied color oject.</returns>
            Public Shared Function FromColor(ByVal c As Color) As PseudoColour
                Return New PseudoColour(c.A, c.R, c.G, c.B)
            End Function

            ''' <summary>
            ''' Adds the supplied pseudocolour object to the current instance by summing the
            ''' RGB components one at a time.
            ''' </summary>
            ''' <param name="p">The pseudocolour to add.</param>
            ''' <param name="AddAlpha">When set to true, the alpha components will be added.
            ''' Otherwise the alpha component will remain unchanged.</param>
            Public Sub Add(ByVal p As PseudoColour, ByVal AddAlpha As Boolean)
                If AddAlpha Then Me.A += p.A
                Me.R += p.R
                Me.G += p.G
                Me.B += p.B
            End Sub


            Public Sub Add(ByVal c As Color, ByVal AddAlpha As Boolean)
                If AddAlpha Then Me.A += c.A
                Me.R += c.R
                Me.G += c.G
                Me.B += c.B
            End Sub

            Public Shared Function Add(ByVal p1 As PseudoColour, ByVal p2 As PseudoColour, ByVal AddAlpha As Boolean) As PseudoColour
                Dim TempAlpha As Double = 255
                If AddAlpha Then TempAlpha = p1.A + p2.A
                Return New PseudoColour(TempAlpha, p1.R + p2.R, p1.G + p2.G, p1.B + p2.B)
            End Function

            Public Shared Function Add(ByVal c1 As Color, ByVal c2 As Color, ByVal AddAlpha As Boolean) As PseudoColour
                Dim TempAlpha As Double = 255
                If AddAlpha Then TempAlpha = c1.A + c2.A
                Return New PseudoColour(TempAlpha, c1.R + c2.R, c1.G + c2.G, c1.B + c2.B)
            End Function

            Public Shared Function Add(ByVal c As Color, ByVal p As PseudoColour, ByVal AddAlpha As Boolean) As PseudoColour
                Dim TempAlpha As Double = 255
                If AddAlpha Then TempAlpha = c.A + p.A
                Return New PseudoColour(TempAlpha, c.R + p.R, c.G + p.G, c.B + p.B)
            End Function


            Public Sub Subtract(ByVal c As Color, ByVal SubtractAlpha As Boolean)
                If SubtractAlpha Then Me.A -= c.A
                Me.R -= c.R
                Me.G -= c.G
                Me.B -= c.B
            End Sub

            Public Sub Subtract(ByVal p As PseudoColour, ByVal SubtractAlpha As Boolean)
                If SubtractAlpha Then Me.A -= p.A
                Me.R -= p.R
                Me.G -= p.G
                Me.B -= p.B
            End Sub

            Public Shared Function Subtract(ByVal p1 As PseudoColour, ByVal p2 As PseudoColour, ByVal SubtractAlpha As Boolean) As PseudoColour
                Dim tempalpha As Double = 255
                If SubtractAlpha Then tempalpha = p1.A - p2.A
                Return New PseudoColour(tempalpha, p1.R - p2.R, p1.G - p2.G, p1.B - p2.B)
            End Function

            Public Shared Function Subtract(ByVal p1 As Color, ByVal p2 As Color, ByVal SubtractAlpha As Boolean) As PseudoColour
                Dim tempalpha As Double = 255
                If SubtractAlpha Then tempalpha = p1.A - p2.A
                Return New PseudoColour(tempalpha, p1.R - p2.R, p1.G - p2.G, p1.B - p2.B)
            End Function

            ''' <summary>
            ''' Divides the (A)RGB components of the class by the supplied double. Does not
            ''' catch division by zero exceptions, so the caller should make that check.
            ''' </summary>
            ''' <param name="d">The double to divide by.</param>
            ''' <param name="DivideAlpha">Set to true if the alpha component is also to be 
            ''' divided as well as the RGB components.</param>
            Public Sub Divide(ByVal d As Double, ByVal DivideAlpha As Boolean)
                Me.Multiply(1.0 / d, DivideAlpha)
            End Sub


            ''' <summary>
            ''' Multiplies the (A)RGB components of the class by the supplied double.
            ''' </summary>
            ''' <param name="d">The number to multiply the components by.</param>
            ''' <param name="MultiplyAlpha">Set to true if the alpha component is also to be 
            ''' multiplied as well as the RGB components.</param>
            Public Sub Multiply(ByVal d As Double, ByVal MultiplyAlpha As Boolean)
                Me.R *= d
                Me.G *= d
                Me.B *= d
                If MultiplyAlpha Then Me.A *= d
            End Sub

            ''' <summary>
            ''' Returns a new pseudocolour object obtained by multiplying the current instance
            ''' by the supplied one. Multiplication is performed componentwise on the RGB values.
            ''' </summary>
            ''' <param name="d">The value to multiply by</param>
            ''' <param name="MultiplyAlpha">Set to true if the alpha value is also to be
            ''' multiplied. Otherwise the alpha value will remain unchanged.</param>
            ''' <returns></returns>
            Public Function MultipliedBy(ByVal d As Double, ByVal MultiplyAlpha As Boolean) As PseudoColour
                If MultiplyAlpha Then
                    Return New PseudoColour(Me.A * d, Me.R * d, Me.G * d, Me.B * B)
                Else
                    Return New PseudoColour(Me.A, Me.R * d, Me.G * d, Me.B * d)
                End If
            End Function

        End Class
#End Region

#Region "Class HSVColours"

        ''' Project  : Automate
        ''' Class    : clsUtility.GraphicsUtils.ColorConverter
        ''' 
        ''' <summary>
        ''' Designed for converting colors from one form to another eg HSV to RGB etc
        ''' </summary>
        Public Class ColorConverter

            ''' Project  : Automate
            ''' Struct   : clsUtility.GraphicsUtils.ColorConverter.RGB
            ''' 
            ''' <summary>
            ''' Represents a color as an RGB.
            ''' </summary>
            Public Structure RGB
                ''' <summary>
                ''' Red component of color. Ranges from 0 to 255
                ''' </summary>
                Dim Red As Integer
                ''' <summary>
                ''' Green component of color. Ranges from 0 to 255
                ''' </summary>
                Dim Green As Integer
                ''' <summary>
                ''' Blue component of color. Ranges from 0 to 255
                ''' </summary>
                Dim Blue As Integer

                Public Sub New(
                 ByVal R As Integer, ByVal G As Integer, ByVal B As Integer)
                    Red = R
                    Green = G
                    Blue = B
                End Sub

                ''' <summary>
                ''' Gets string representation.
                ''' </summary>
                ''' <returns>String.</returns>
                Public Overrides Function ToString() As String
                    Return My.Resources.RGB & Red.ToString & " / " & Green.ToString & " / " & Blue.ToString
                End Function

                ''' <summary>
                ''' Makes a System.Drawing.Color object from this RGB.
                ''' </summary>
                ''' <returns></returns>
                Public Function ToColor() As Color
                    Return Color.FromArgb(Red, Green, Blue)
                End Function

                ''' <summary>
                ''' Makes an RGB from a .NET color.
                ''' </summary>
                ''' <param name="C">The color.</param>
                ''' <returns>.</returns>
                Public Shared Function FromColor(ByVal C As Color) As RGB
                    Return New RGB(C.R, C.G, C.B)
                End Function

            End Structure

            ''' Project  : Automate
            ''' Struct   : clsUtility.GraphicsUtils.ColorConverter.HSV
            ''' 
            ''' <summary>
            ''' Color representation as HSV.
            ''' </summary>
            Public Structure HSV
                ''' <summary>
                ''' Hue. Can think of this as distance round color wheel in degrees. Values are
                ''' 0 to 360.
                ''' </summary>
                Dim Hue As Integer
                ''' <summary>
                ''' Saturation. From 0 to 255.
                ''' </summary>
                Dim Saturation As Integer
                ''' <summary>
                ''' Value. From 0 to 255.
                ''' </summary>
                Dim Value As Integer

                Public Sub New( _
                 ByVal H As Integer, ByVal S As Integer, ByVal V As Integer)
                    Hue = H
                    Saturation = S
                    Value = V
                End Sub

                ''' <summary>
                ''' Gets string representation.
                ''' </summary>
                ''' <returns>String.</returns>
                Public Overrides Function ToString() As String
                    Return "H/S/V: " & Hue.ToString & " / " & Saturation.ToString & " / " & Value.ToString
                End Function

                ''' <summary>
                ''' Makes a System.Drawing.Color object from this HSV.
                ''' </summary>
                ''' <returns></returns>
                Public Function ToColor() As Color
                    Dim RGB As RGB = HSVtoRGB(Me)
                    Return RGB.ToColor
                End Function

                ''' <summary>
                ''' Determines if HSV is null.
                ''' </summary>
                ''' <returns>True if H = S = V = 0; false otherwise.</returns>
                Public Function IsNull() As Boolean
                    Return Me.Hue = 0 AndAlso Me.Saturation = 0 AndAlso Me.Value = 0
                End Function

            End Structure

            ''' <summary>
            ''' Converts an HSV to an RGB.
            ''' </summary>
            ''' <param name="H">Hue from 0..360</param>
            ''' <param name="S">Saturation from 0..255</param>
            ''' <param name="V">Value from 0..255</param>
            ''' <returns>RGB equivalent of the HSV.</returns>
            Public Shared Function HSVtoRGB(ByVal H As Integer, ByVal S As Integer, ByVal V As Integer) As RGB
                Return HSVtoRGB(New HSV(H, S, V))
            End Function

            ''' <summary>
            ''' Converts an HSV to an RGB.
            ''' </summary>
            ''' <param name="HSV">The HSV to convert.</param>
            ''' <returns>RGB equivalent of the HSV.</returns>
            Public Shared Function HSVtoRGB(ByVal HSV As HSV) As RGB

                'Working variables representing HSV values.
                'h is from 0 to 360 degrees; s,v from 0.0 to 1.0
                Dim h As Decimal
                Dim s As Decimal
                Dim v As Decimal

                'Working RGB values
                Dim r As Decimal
                Dim g As Decimal
                Dim b As Decimal

                ' Scale Hue to be between 0 and 360. Saturation
                ' and Value scale to be between 0 and 1.
                h = HSV.Hue Mod 360
                s = HSV.Saturation / 255D
                v = HSV.Value / 255D

                If s = 0 Then
                    'Special case when no saturation exists.
                    'Corresponds to being at centre of colour
                    'wheel so angle (v) is irrelevant
                    r = v
                    g = v
                    b = v
                Else
                    Dim p As Decimal
                    Dim q As Decimal
                    Dim t As Decimal

                    'Number representing the colour Segment we are in from 0,1,..,5
                    'for the six colour segments
                    Dim NumberOfSegment As Integer
                    'The fraction within the current colour segment from 0.0 to 1.0
                    Dim FractionWithinSegment As Decimal


                    'which colour Segment are we in?
                    NumberOfSegment = CInt(Math.Floor(h / 60)) Mod 6

                    'how far as a fraction are we into the segment?
                    FractionWithinSegment = h / 60 - NumberOfSegment

                    'the three axes of the color are translated as follows
                    p = v * (1 - s)
                    q = v * (1 - (s * FractionWithinSegment))
                    t = v * (1 - (s * (1 - FractionWithinSegment)))

                    'the axes correspond to different RGB values depending
                    'on which colour segment we are in.
                    Select Case NumberOfSegment
                        Case 0
                            r = v
                            g = t
                            b = p
                        Case 1
                            r = q
                            g = v
                            b = p
                        Case 2
                            r = p
                            g = v
                            b = t
                        Case 3
                            r = p
                            g = q
                            b = v
                        Case 4
                            r = t
                            g = p
                            b = v
                        Case 5
                            r = v
                            g = p
                            b = q
                    End Select
                End If

                'scale values back to 0..255 and return
                Return New RGB(CInt(r * 255), CInt(g * 255), CInt(b * 255))
            End Function


            ''' <summary>
            ''' Converts a colour to an HSV.
            ''' </summary>
            ''' <param name="c">The colour convert.</param>
            ''' <returns>HSV equivalent of the RGB.</returns>
            Public Shared Function RGBtoHSV(ByVal c As Color) As HSV
                Return RGBtoHSV(New RGB(c.R, c.G, c.B))
            End Function

            ''' <summary>
            ''' Converts an RGB to an HSV.
            ''' </summary>
            ''' <param name="RGB">The RGB to convert.</param>
            ''' <returns>HSV equivalent of the RGB.</returns>
            Public Shared Function RGBtoHSV(ByVal RGB As RGB) As HSV

                'intermediate calculation values
                Dim min As Decimal
                Dim max As Decimal
                Dim delta As Decimal

                'working values for RGB input from 0.0. to 1.0
                Dim r As Decimal = RGB.Red / 255D
                Dim g As Decimal = RGB.Green / 255D
                Dim b As Decimal = RGB.Blue / 255D

                'working values for HSV output
                Dim h As Decimal
                Dim s As Decimal
                Dim v As Decimal

                min = Math.Min(Math.Min(r, g), b)
                max = Math.Max(Math.Max(r, g), b)
                delta = max - min

                If max = 0 Or delta = 0 Then
                    ' R, G, and B must be 0, or all the same.
                    ' In this case, S is 0, and H is undefined.
                    ' Using H = 0 is as good as any...
                    s = 0
                    h = 0
                Else
                    s = delta / max
                    If g = max Then
                        'Cyan/Yellow
                        h = 2 + (b - r) / delta
                    ElseIf r = max Then
                        ' Yellow/Magenta
                        h = (g - b) / delta
                    Else
                        'Magenta/Cyan
                        h = 4 + (r - g) / delta
                    End If

                End If

                'always so no matter what
                v = max

                ' Scale h to range 0..360. 
                h *= 60
                If h < 0 Then
                    h += 360
                End If

                'Scale values to range 0..255, except H stays as (0..360)
                Return New HSV(CInt(h), CInt(s * 255), CInt(v * 255))
            End Function
        End Class

#End Region


        ''' <summary>
        ''' Reflects in a line a shape lying in a plane.
        ''' </summary>
        ''' <param name="PointA">The first of two points specifying the line of reflection.
        ''' </param>
        ''' <param name="PointB">The second of two points specifying the line of
        ''' reflection.</param>
        ''' <param name="Polygon">The vertices of the shape to be reflected.</param>
        Public Shared Sub InvertShapeInLine(ByVal PointA As Point, ByVal PointB As Point, ByRef Polygon() As Point)

            '##################################################################
            '                                                    PointB
            '            P.                                   - x  --line of reflection
            '              .                         -
            '               .               -
            '                .     -
            '              -  .
            '   x -            .
            '   PointA          .Q  --P is reflected in line to get Q.
            '                    
            '
            'Let n be the normal vector perpendicular to the line AB.
            'Then (visually obvious) q = p - 2*(n.p)n (where n.p is the dot product of the two vectors)
            'provided the line AB passes through the origin.
            '
            'If AB does not pass through the origin then simple cooordinate shift gives
            'q = p - 2*(n.p)n + 2(n.a)n, where a is the shift vector between coordinates.
            '##################################################################

            'first calculate normal vector:
            Dim n_1 As Double = 1
            Dim n_2 As Double
            If Not (PointB.X - PointA.X) = 0 Then
                n_2 = -(PointB.Y - PointA.Y) / (PointB.X - PointA.X)
            Else
                n_2 = 0
            End If
            Dim NormalisingFactor As Double = Math.Sqrt(n_1 * n_1 + n_2 * n_2)
            n_1 /= NormalisingFactor
            n_2 /= NormalisingFactor

            'Calculate shift vector
            Dim a_1 As Double = PointA.X
            Dim a_2 As Double = PointA.Y

            'calculate dot product n.a
            Dim na As Double = n_1 * a_1 + n_2 * a_2

            'then for each point in the shape perform the reflection:
            For i As Integer = 0 To Polygon.Length - 1
                'Calculate (n.p):
                Dim np As Double = n_1 * Polygon(i).X + n_2 * Polygon(i).Y
                'reflect:
                Polygon(i).X += CInt(-2 * np * n_1 + 2 * na * n_1)
                Polygon(i).Y += CInt(-2 * np * n_2 + 2 * na * n_2)
            Next

        End Sub

        ''' <summary>
        ''' Measures the space required to draw the specified text using the specified
        ''' font.
        ''' </summary>
        ''' <param name="Text">The text to be drawn.</param>
        ''' <param name="Font">The font to be used when drawing the text.</param>
        ''' <param name="sf">The string format</param>
        ''' <return>Variable carrying the computed size.</return>
        Public Shared Function MeasureStringSize(ByVal Text As String, ByVal Font As Font, ByVal sf As StringFormat) As SizeF
            Using b As New Bitmap(1, 1, Imaging.PixelFormat.Format32bppArgb)
                Using g As Graphics = Graphics.FromImage(b)
                    Return g.MeasureString(Text, Font, g.ClipBounds.Size, sf)
                End Using
            End Using
        End Function

        ''' <summary>
        ''' Draws text vertically from top to bottom at the specified location.
        ''' </summary>
        ''' <param name="objgraphics">The graphics object that is to be used to draw the text.</param>
        ''' <param name="StringFont">The font that is to be used to draw the text.</param>
        ''' <param name="Text">The text to be drawn.</param>
        ''' <param name="objbrush">The brush that is to be used to draw the text.</param>
        ''' <param name="x">The x coordinate of the upper left corner of the text.</param>
        ''' <param name="y">The y coordinate of the upper left corner of the text.</param>
        ''' <param name="UseAntialiasing">Indicates whether antialiasing should be used to draw the string.</param>
        Public Shared Sub DrawVerticalText(ByVal objgraphics As Graphics, ByVal StringFont As Font, ByVal Text As String, ByVal objbrush As Brush, ByVal x As Single, ByVal y As Single, ByVal UseAntialiasing As Boolean)
            Dim SF As New StringFormat
            SF.FormatFlags = StringFormatFlags.DirectionVertical
            If UseAntialiasing Then objgraphics.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAlias
            objgraphics.DrawString(Text, StringFont, objbrush, x, y, SF)
        End Sub


        ''' <summary>
        ''' Imitates the 3D borders drawn on the standard controls but that don't seem to
        ''' be provided for ordinary user controls.
        ''' </summary>
        ''' <param name="objgraphics">The graphics object to be used in drawing the lines.
        ''' </param>
        ''' <param name="objControl"></param>
        Public Shared Sub Draw3DBorderOnUserControl(ByVal objgraphics As Graphics, ByVal objControl As UserControl)

            'the border is composed of two lines around the edge. On the upper right
            'half we have a dark pair of lines and on the lower left we have a lighter
            'pair.

            'First draw the outer line from the top left corner clockwise: semidark, verylight, verylight, semidark.
            objgraphics.DrawLine(New Pen(System.Drawing.SystemColors.ControlDark), 0, 0, 0, objControl.Height - 1)
            objgraphics.DrawLine(New Pen(System.Drawing.SystemColors.ControlLightLight), 0, objControl.Height - 1, objControl.Width - 1, objControl.Height - 1)
            objgraphics.DrawLine(New Pen(System.Drawing.SystemColors.ControlLightLight), objControl.Width - 1, objControl.Height - 1, objControl.Width - 1, 0)
            objgraphics.DrawLine(New Pen(System.Drawing.SystemColors.ControlDark), objControl.Width - 1, 0, 0, 0)

            'Next draw the inner line from the top left corner clockwise: verydark, semilight, semilight, verydark.
            objgraphics.DrawLine(New Pen(System.Drawing.SystemColors.ControlDarkDark), 1, 1, 1, objControl.Height - 2)
            objgraphics.DrawLine(New Pen(System.Drawing.SystemColors.ControlLight), 1, objControl.Height - 2, objControl.Width - 2, objControl.Height - 2)
            objgraphics.DrawLine(New Pen(System.Drawing.SystemColors.ControlLight), objControl.Width - 2, objControl.Height - 2, objControl.Width - 2, 1)
            objgraphics.DrawLine(New Pen(System.Drawing.SystemColors.ControlDarkDark), objControl.Width - 2, 1, 1, 1)

        End Sub


        ''' <summary>
        ''' Draws a tooltip box at the specified location at the specified point.
        ''' If any of the arguments is a null reference then simply exits.
        ''' </summary>
        ''' <param name="g">The graphics object to use when drawing the tooltip.</param>
        ''' <param name="Text">The text to be displayed.</param>
        ''' <param name="Location">The location at which to draw the tooltip.</param>
        Public Shared Sub DrawToolTip(ByVal g As Graphics, ByVal Text As String, ByVal Location As Point)
            If Not ((g Is Nothing) OrElse (Text Is Nothing) OrElse Location.Equals(Point.Empty)) Then

                Dim font1 As New System.Drawing.Font(clsFont.FamilyName, 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
                Dim size1 As SizeF
                size1.Width = 800
                size1.Height = 1000
                If Text.Length > 10000 Then
                    Text = Text.Substring(0, 10000)
                End If
                Dim size2 As SizeF = g.MeasureString(Text, font1, size1)
                Dim rect1 As RectangleF
                Dim X As Integer
                Dim Y As Integer
                Dim w As Integer
                Dim h As Integer

                X = Location.X
                Y = Location.Y
                w = CInt(size2.Width)
                h = CInt(size2.Height)

                If X + w > g.ClipBounds.Width Then
                    X = CInt(g.ClipBounds.Width - w - 10)
                End If

                If Y + h > g.ClipBounds.Height Then
                    Y = CInt(g.ClipBounds.Height - h - 10 - 22)
                End If

                rect1.X = X + 3
                rect1.Y = Y + 3
                rect1.Width = w + 3
                rect1.Height = h + 3

                Dim brush1 As New SolidBrush(System.Drawing.Color.White)
                Dim brush2 As New SolidBrush(System.Drawing.Color.Gray)
                Dim pen1 As New Pen(System.Drawing.Color.Black)

                g.FillRectangle(brush2, X + 3, Y + 3, w + 3, h + 3)
                g.DrawRectangle(pen1, X, Y, w + 3, h + 3)
                g.FillRectangle(brush1, X + 1, Y + 1, w + 2, h + 2)
                g.DrawString(Text, font1, New SolidBrush(Color.Black), rect1)

            End If
        End Sub


        ''' <summary>
        ''' Draws a two-way gradient in a rectangle: taking any one pixel-wide row of the
        ''' rectangle there will be a horizontal gradient; taking any one pixel-wide
        ''' column of the rectangle there will be a vertical gradient.
        ''' </summary>
        ''' <param name="objgraphics">The graphics object to use.</param>
        ''' <param name="R">The rectangle in which to draw the gradient.</param>
        ''' <param name="C1">The colour to appeear in the top left corner.</param>
        ''' <param name="C2">The colour to appear in the top right corner.</param>
        ''' <param name="c3">The colour to appeear in the bottom left corner.</param>
        ''' <param name="c4">The colour to appear in the bottom right corner.</param>
        Public Shared Sub FillRectangleWithTwoWayGradient(ByVal objgraphics As Graphics, ByVal R As Rectangle, ByVal C1 As Color, ByVal C2 As Color, ByVal C3 As Color, ByVal C4 As Color)
            Dim iLeftHandIncrement As PseudoColour
            Dim iRightHandIncrement As PseudoColour
            Dim objBrush As Brush
            Dim rTemp As New Rectangle
            If R.Width >= R.Height Then
                iLeftHandIncrement = PseudoColour.FromColor(C3)
                iLeftHandIncrement.Subtract(C1, False)
                iLeftHandIncrement.Divide(R.Height, False)
                iRightHandIncrement = PseudoColour.FromColor(C4)
                iRightHandIncrement.Subtract(C2, False)
                iRightHandIncrement.Divide(R.Height, False)
                For i As Integer = 0 To R.Height
                    rTemp = New Rectangle(R.X, R.Y + i, R.Width, 1)
                    objBrush = New Drawing2D.LinearGradientBrush(rTemp, PseudoColour.Add(C1, iLeftHandIncrement.MultipliedBy(i, False), False).ToColor, PseudoColour.Add(C2, iRightHandIncrement.MultipliedBy(i, False), False).ToColor, Drawing2D.LinearGradientMode.Horizontal)
                    objgraphics.FillRectangle(objBrush, rTemp)
                Next
            Else
                iLeftHandIncrement = PseudoColour.FromColor(C2)
                iLeftHandIncrement.Subtract(C1, False)
                iLeftHandIncrement.Divide(R.Width, False)
                iRightHandIncrement = PseudoColour.FromColor(C4)
                iRightHandIncrement.Subtract(C3, False)
                iRightHandIncrement.Divide(R.Width, False)
                For i As Integer = 0 To R.Width
                    rTemp = New Rectangle(R.X + i, R.Y, R.Height, 1)
                    objBrush = New Drawing2D.LinearGradientBrush(rTemp, PseudoColour.Add(C1, iLeftHandIncrement.MultipliedBy(i, False), False).ToColor, PseudoColour.Add(C3, iRightHandIncrement.MultipliedBy(i, False), False).ToColor, Drawing2D.LinearGradientMode.Horizontal)
                    objgraphics.FillRectangle(objBrush, rTemp)
                Next
            End If
        End Sub

        Public Shared Sub FillRectangleWithGradient(ByVal objgraphics As Graphics, ByVal R As Rectangle, ByVal C1 As Color, ByVal C2 As Color)
            Dim p1 As Point, p2 As Point
            Dim objBrush As Brush

            p1.X = R.X
            p1.Y = R.Y
            p2.X = R.Width
            p2.Y = 0

            objBrush = New System.Drawing.Drawing2D.LinearGradientBrush(p1, p2, C1, C2)
            objgraphics.FillRectangle(objBrush, R)
        End Sub

        ''' <summary>
        ''' Inverts a color
        ''' </summary>
        ''' <param name="c">The color to invert</param>
        ''' <returns>A new color that is the inverse of the given color</returns>
        ''' <remarks></remarks>
        Public Shared Function InvertColor(ByVal c As Drawing.Color) As Drawing.Color
            Dim r As Integer = 255 - c.R
            Dim g As Integer = 255 - c.G
            Dim b As Integer = 255 - c.B
            Return Color.FromArgb(c.A, r, g, b)
        End Function
    End Class


#End Region


End Class
