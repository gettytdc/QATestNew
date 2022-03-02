Option Strict On

Imports System.ComponentModel
Imports System.Runtime.InteropServices
Imports System.Collections.Generic
Imports System.Drawing
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.BPCoreLib
Imports BluePrism.Core.Utility.DrawingExtensionMethods

<TypeConverter(GetType(ExpandableObjectConverter))> _
Public MustInherit Class clsUIEntity

    ''' <summary>
    ''' A short identifier to describe the entity type. This is used in
    ''' diagnostics output.
    ''' </summary>
    Public MustOverride ReadOnly Property EntityTypeID() As String


    Public Property Name() As String
        Get
            Return msName
        End Get
        Set(ByVal Value As String)
            msName = Value
        End Set
    End Property
    Private msName As String

    Public Property Parent() As clsUIWindow
        Get
            Return mobjParent
        End Get
        Set(ByVal Value As clsUIWindow)
            mobjParent = Value
        End Set
    End Property
    Private mobjParent As clsUIWindow

    <clsUIWindow.WindowIdentifierAttribute(clsQuery.IdentifierTypes.Ordinal)> _
    Public Property Ordinal() As Integer
        Get
            Return miOrdinal
        End Get
        Set(ByVal Value As Integer)
            miOrdinal = Value
        End Set
    End Property
    Private miOrdinal As Integer

    Public ReadOnly Property Children() As Collection
        Get
            Return mcolChildren
        End Get
    End Property
    Private mcolChildren As New Collection

    <TypeConverter(GetType(HexConverter))>
    Public Property Handle() As IntPtr
        Get
            Return mHandle
        End Get
        Set(ByVal value As IntPtr)
            mHandle = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the handle for the UI entity as an IntPtr. Int32 doesn't cut it in 64
    ''' bit systems, so we could do with changing it, but it's a little too ingrained
    ''' to do it on a whim (and a lot of the relevant modWin32 methods incorrectly
    ''' take Int32s where they should take IntPtrs), so I'm dropping this here to act
    ''' as a band-aid for now, so that new changes can have something which will just
    ''' work when it is fixed.
    ''' </summary>
    ''' <remarks>TODO: Fix Int32 handles in clsUIEntity and modWin32</remarks>
    Public Property Hwnd() As IntPtr
        Get
            Return mHandle
        End Get
        Set(ByVal value As IntPtr)
            mHandle = value
        End Set
    End Property
    Private mHandle As IntPtr

    ''' <summary>
    ''' Adds a child to this entity.
    ''' </summary>
    ''' <param name="e">The child (another clsUIEntity)</param>
    ''' <returns>An ordinal value which indicates the number of children
    ''' already present. The actual value is not guaranteed, only a relative
    ''' order.</returns>
    Public Function AddChild(ByVal e As clsUIEntity) As Integer
        mcolChildren.Add(e)
        Return mcolChildren.Count
    End Function

    Public Sub RemoveChild(ByVal e As clsUIEntity)
        For i As Integer = 1 To mcolChildren.Count
            If mcolChildren.Item(i) Is e Then
                mcolChildren.Remove(i)
                Exit For
            End If
        Next
    End Sub

    Friend Overridable Function GetBounds() As RECT

    End Function


End Class

Public Class clsUIWindow : Inherits clsUIEntity

    Public Overrides ReadOnly Property EntityTypeID() As String
        Get
            Return "WINDOW"
        End Get
    End Property


    ''' <summary>
    ''' "True" if the window is active, "False" otherwise.
    ''' </summary>
    <WindowIdentifierAttribute(clsQuery.IdentifierTypes.Active)> _
    Public ReadOnly Property Active() As String
        Get
            Dim info As New WINDOWINFO
            info.cbSize = Convert.ToUInt32(Marshal.SizeOf(info))
            GetWindowInfo(Handle, info)
            If info.dwWindowStatus = WS_ACTIVECAPTION Then
                Return "True"
            Else
                Return "False"
            End If
        End Get
    End Property

    <WindowIdentifierAttribute(clsQuery.IdentifierTypes.Enabled)> _
    Public ReadOnly Property Enabled() As String
        Get
            Dim info As New WINDOWINFO
            info.cbSize = Convert.ToUInt32(Marshal.SizeOf(info))
            GetWindowInfo(Handle, info)
            If (info.dwStyle And WindowStyles.WS_DISABLED) = 0 Then
                Return "True"
            Else
                Return "False"
            End If
        End Get
    End Property

    <WindowIdentifierAttribute(clsQuery.IdentifierTypes.Visible)> _
    Public ReadOnly Property Visible() As String
        Get
            Dim info As New WINDOWINFO
            info.cbSize = Convert.ToUInt32(Marshal.SizeOf(info))
            GetWindowInfo(Handle, info)
            If (info.dwStyle And WindowStyles.WS_VISIBLE) <> 0 Then
                Return "True"
            Else
                Return "False"
            End If
        End Get
    End Property

    ''' <summary>
    ''' ScreenVisible looks at not only the window's visibility, but that of all its
    ''' ancestors. If any are not visible, then the window is not "screen visible".
    ''' </summary>
    <WindowIdentifierAttribute(clsQuery.IdentifierTypes.ScreenVisible)> _
    Public ReadOnly Property ScreenVisible() As String
        Get
            Dim w As clsUIWindow = Me
            While True
                If w.Visible = "False" Then Return "False"
                w = TryCast(w.Parent, clsUIWindow)
                'If there is no parent, or the parent is our 'fake' Desktop window
                '(which has a handle of 0) then we've reached the top of the tree...
                If w Is Nothing OrElse w.Handle = IntPtr.Zero Then Return "True"
            End While
            'The VB compiler is not capable of seeing that the following line of
            'code cannot be reached...
            Return "Unicorn"
        End Get
    End Property


    ''' <summary>
    ''' True if the window is a checkbox or radio button, and the state is checked.
    ''' Otherwise False. False includes indeterminate button states.
    ''' </summary>
    <WindowIdentifierAttribute(clsQuery.IdentifierTypes.Checked)> _
    Public ReadOnly Property Checked() As Boolean
        Get

            If mClassName.StartsWith("WindowsForms10.BUTTON") Then
                '.NET buttons do not respond properly to the BM_GETCHECK message, but they
                'do use what is normally the WS_MAXIMIZEBOX part of the style to store
                'the checked state!
                Dim checkedbystyle As Boolean
                checkedbystyle = (GetWindowLong(Hwnd, GWL.GWL_STYLE) And &H10000) <> 0
                clsConfig.LogWin32("Win32: Alternate check state " & checkedbystyle.ToString())
                Return checkedbystyle
            Else
                'The normal way of doing it!
                Dim checkstate As Integer
                checkstate = SendMessage(Handle, WindowMessages.BM_GETCHECK, 0, 0).ToInt32()
                clsConfig.LogWin32("Win32: Button check state " & checkstate.ToString())
                Return (checkstate And ButtonCheckStates.BST_CHECKED) <> 0
            End If
        End Get
    End Property

    ''' <summary>
    ''' Determines whether a window is a top-level window
    ''' </summary>
    Public ReadOnly Property IsTopLevel() As Boolean
        Get
            Dim ancestor As IntPtr = GetAncestor(Handle, GA_ROOT)
            Return ancestor = Handle
        End Get
    End Property


    <WindowIdentifierAttribute(clsQuery.IdentifierTypes.Title)> _
    Public Property Title() As String
        Get
            Return msTitle
        End Get
        Set(ByVal Value As String)
            msTitle = Value
        End Set
    End Property
    Private msTitle As String

    <WindowIdentifierAttribute(clsQuery.IdentifierTypes.ClassName)> _
    Public Property ClassName() As String
        Get
            If mClassName Is Nothing Then Return ""
            Return mClassName
        End Get
        Set(ByVal Value As String)
            mClassName = FilterClassName(Value)
        End Set
    End Property
    Private mClassName As String

    ''' <summary>
    ''' The 'Style' of the window.
    ''' See https://msdn.microsoft.com/en-us/library/windows/desktop/ms632600(v=vs.85).aspx
    ''' for details of window styles
    ''' </summary>
    <WindowIdentifier(clsQuery.IdentifierTypes.WindowStyle)>
    Public Property Style As Long

    ''' <summary>
    ''' Checks if the classname of this window contains any of the specified
    ''' substrings, ignoring case.
    ''' </summary>
    ''' <param name="strs">The strings to check the classname of this window for.
    ''' </param>
    ''' <returns>True if any of the given substrings were found in the classname of
    ''' this window, disregarding case.</returns>
    Public Function ClassContains(ParamArray strs() As String) As Boolean
        Dim ignoreCase = StringComparison.InvariantCultureIgnoreCase
        For Each str As String In strs
            If ClassName.IndexOf(str, ignoreCase) <> -1 Then Return True
        Next
        Return False
    End Function

    ''' <summary>
    ''' Filters .NET classnames so that they only have the first two parts
    ''' of the classname e.g. WindowsForms10.EDIT.blah.9032944.3434 becomes
    ''' WindowsForms10.EDIT
    ''' </summary>
    ''' <param name="sClassName">The classname</param>
    ''' <returns>The filtered classname</returns>
    Private Function FilterClassName(ByVal sClassName As String) As String
        Try
            If sClassName.StartsWith("WindowsForms10") Then
                Dim parts() As String = Split(sClassName, ".")
                Return parts(0) & "." & parts(1)
            Else
                Return sClassName
            End If
        Catch
            Return sClassName
        End Try
    End Function

    ''' <summary>
    ''' The .NET classname if available e.g. "System.Windows.Forms.LinkLabel",
    ''' otherwise an empty string
    ''' </summary>
    <WindowIdentifierAttribute(clsQuery.IdentifierTypes.TypeName)> _
    Public ReadOnly Property TypeName() As String
        Get
            Try
                ' If we're running as a 64 bit process, then ManagedSpyLib is not
                ' available to us, so we just return an empty string for that.
                If IntPtr.Size = 8 Then Return ""
                ' This can sometimes return null (bug 8761), convert to empty string
                Dim cp As clsLocalTargetApp.ControlProxy = clsLocalTargetApp.ControlProxy.FromHandle(Hwnd)
                Dim nm As String = cp.GetClassName()
                If nm Is Nothing Then Return "" Else Return nm
            Catch
                Return ""
            End Try
        End Get
    End Property

    <WindowIdentifierAttribute(clsQuery.IdentifierTypes.WindowText)> _
    Public ReadOnly Property WindowText() As String
        Get
            If mClassName IsNot Nothing AndAlso mClassName = "StatusBar20WndClass" Then
                'This particular VB6 ActiveX status bar seems to be broken and does not
                'respond to the WM_GETTEXTLENGTH message properly. I have had it claiming
                'it has 0x100000000 characters. It's better to catch it here than to attempt
                'to allocate that much memory, just in case it succeeds! See bug #
                Return "<StatusBar20>"
            End If

            Dim iLen As Integer, buffer As String
            Try
                If SendMessageTimeout(Handle, WindowMessages.WM_GETTEXTLENGTH, 0, 0, SendMessageTimeoutFlags.SMTO_NOTIMEOUTIFNOTHUNG Or SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 1000, iLen) <> IntPtr.Zero Then
                    buffer = New String(" "c, iLen + 1)
                    SendMessageString(Handle, WindowMessages.WM_GETTEXT, iLen + 1, buffer)
                    Dim rawText = buffer.Left(iLen)
                    Return RemoveNullCharactersFrom(rawText)
                End If
            Catch
            End Try
            Return "<UNKNOWN>"
        End Get
    End Property

    ''' <summary>
    ''' When utilitising the WM_GETTEXT method of the User32 Windows API, if you decode the returned bytes
    ''' into a different charset (In our case Unicode) you can have null characters appear in the result,
    ''' so this method will remove them if they are present. 
    ''' </summary>
    ''' <param name="input">The string to remove null characters from.</param>
    ''' <returns>The sanitized string.</returns>
    Private Function RemoveNullCharactersFrom(input As String) As String
        Dim processedString = input
        Dim emptySpaceCharacter = Chr(32)

        If Array.Exists(processedString.ToCharArray(), Function(x) x = vbNullChar) Then
            processedString = processedString.Replace(vbNullChar, emptySpaceCharacter)
            processedString = processedString.TrimEnd(emptySpaceCharacter)
        End If

        Return processedString
    End Function

    <WindowIdentifierAttribute(clsQuery.IdentifierTypes.AncestorsText)> _
    Public ReadOnly Property AncestorsText() As String
        Get
            Dim sb As New StringBuilder()
            Dim aw As clsUIWindow = Parent
            While aw IsNot Nothing
                sb.Append(aw.WindowText & " ")
                aw = aw.Parent
            End While
            Return sb.ToString()
        End Get
    End Property

    <WindowIdentifierAttribute(clsQuery.IdentifierTypes.CtrlID)> _
   Public Property CtrlID() As Int32
        Get
            Return miCtrlID
        End Get
        Set(ByVal Value As Int32)
            miCtrlID = Value
        End Set
    End Property
    Private miCtrlID As Int32

    <WindowIdentifierAttribute(clsQuery.IdentifierTypes.AncestorCount)> _
    Public ReadOnly Property AncestorCount() As Integer
        Get
            Dim count As Integer
            Dim aw As clsUIWindow = Parent
            While aw IsNot Nothing
                count += 1
                aw = aw.Parent
            End While
            Return count
        End Get
    End Property

    <Category("Location")> _
    Public ReadOnly Property ScreenBounds() As System.Drawing.Rectangle
        Get
            Dim r As RECT
            GetWindowRect(Handle, r)
            Return r
        End Get
    End Property

    ''' <summary>
    ''' Obsolete: This dosen't really do what was intended, left for backwards
    ''' compatibility, use <cref>RelativeClientBounds</cref> instead.
    ''' </summary>
    <Obsolete>
    Public ReadOnly Property ClientBounds() As System.Drawing.Rectangle
        Get
            Dim r As RECT
            GetWindowRect(Handle, r)

            'Subtract the screen location of the parent
            Dim p As POINTAPI
            p.x = r.Left
            p.y = r.Top
            If Not Parent Is Nothing Then
                ScreenToClient(CType(Parent, clsUIWindow).Handle, p)
            End If
            r.Left -= p.x
            r.Right -= p.x
            r.Top -= p.y
            r.Bottom -= p.y

            Return r
        End Get
    End Property

    ''' <summary>
    ''' Get the bounds relative to the parent window
    ''' </summary>
    Public ReadOnly Property RelativeClientBounds() As System.Drawing.Rectangle
        Get
            Dim r = ScreenBounds
            If Parent Is Nothing Then Return r
            With Parent.ScreenBounds.Location
                r.Offset(-.X, -.Y)
            End With
            Return r
        End Get
    End Property


    <Category("Location")> _
    <WindowIdentifierAttribute(clsQuery.IdentifierTypes.X)> _
    Public ReadOnly Property X() As Integer
        Get
            Dim r As RECT, p As POINTAPI
            GetWindowRect(Handle, r)
            p.x = r.Left
            p.y = r.Top
            If Not Parent Is Nothing Then
                ScreenToClient(CType(Parent, clsUIWindow).Handle, p)
            End If
            Return p.x
        End Get
    End Property


    <Category("Location")> _
    <WindowIdentifierAttribute(clsQuery.IdentifierTypes.Y)> _
    Public ReadOnly Property Y() As Integer
        Get
            Dim r As RECT, p As POINTAPI
            GetWindowRect(Handle, r)
            p.x = r.Left
            p.y = r.Top
            If Not Parent Is Nothing Then
                ScreenToClient(CType(Parent, clsUIWindow).Handle, p)
            End If
            Return p.y
        End Get
    End Property

    <Category("Size")> _
    <WindowIdentifierAttribute(clsQuery.IdentifierTypes.Width)> _
     Public ReadOnly Property Width() As Integer
        Get
            Dim r As RECT
            GetWindowRect(Handle, r)
            Return r.Right - r.Left + 1
        End Get
    End Property

    <Category("Size")> _
    <WindowIdentifierAttribute(clsQuery.IdentifierTypes.Height)> _
    Public ReadOnly Property Height() As Integer
        Get
            Dim r As RECT
            GetWindowRect(Handle, r)
            Return r.Bottom - r.Top + 1
        End Get
    End Property

    ''' <summary>
    ''' Get the offset of the client area for this window.
    ''' </summary>
    ''' <returns>A Point containing the offset.</returns>
    Friend Function GetClientOffset() As Point
        Dim r As RECT
        GetClientRect(Handle, r)
        Dim pa As POINTAPI
        pa.x = r.Left
        pa.y = r.Top
        ClientToScreen(Handle, pa)
        GetWindowRect(Handle, r)
        Dim p As New Point(pa.x - r.Left, pa.y - r.Top)
        Return p
    End Function

    Friend Overrides Function GetBounds() As RECT
        Dim r As RECT, r2 As RECT
        Dim p As POINTAPI
        ClientToScreen(Me.Handle, p)
        GetClientRect(Handle, r)
        r2.Left = p.x
        r2.Top = p.y
        r2.Right = p.x + r.Right
        r2.Bottom = p.y + r.Bottom
        Return r2
    End Function

    <WindowIdentifierAttribute(clsQuery.IdentifierTypes.ChildCount)> _
    Public ReadOnly Property ChildCount() As Integer
        Get
            If Me.Children IsNot Nothing Then
                Return Me.Children.Count
            Else
                Return 0
            End If
        End Get
    End Property

    ''' <summary>
    ''' Get the window identifiers for the given window, in the format returned from
    ''' a spy query.
    ''' </summary>
    ''' <returns>The formatted identifier information.</returns>
    Public Function GetIdentifiers() As String
        Return AppendIdentifiers(New StringBuilder(1024)).ToString()
    End Function

    Public Function AppendIdentifiers(ByVal sb As StringBuilder) As StringBuilder
        ' The identifiers relating to the target window itself
        sb _
         .Append("+WindowText=").Append(clsQuery.EncodeValue(WindowText)) _
         .Append(" +ClassName=").Append(clsQuery.EncodeValue(ClassName)) _
         .Append(" CtrlID=").Append(CtrlID) _
         .Append(" X=").Append(X) _
         .Append(" Y=").Append(Y) _
         .Append(" Width=").Append(Width) _
         .Append(" Height=").Append(Height) _
         .Append(" +Visible=").Append(Visible) _
         .Append(" +ScreenVisible=").Append(ScreenVisible) _
         .Append(" +Enabled=").Append(Enabled) _
         .Append(" Active=").Append(Active) _
         .Append(" Ordinal=").Append(Ordinal) _
         .Append(" ChildCount=").Append(Children.Count) _
         .Append(" AncestorsText=").Append(clsQuery.EncodeValue(AncestorsText)) _
         .Append(" AncestorCount=").Append(AncestorCount) _
         .Append(" TypeName=").Append(clsQuery.EncodeValue(TypeName)) _
         .Append(" ScreenBounds=").Append(GetBounds()) _
         .Append(" Style=").Append(Style)

        ' Add identifiers relating to the parent window, if relevant...
        Dim p As clsUIWindow = TryCast(Parent, clsUIWindow)
        If p IsNot Nothing AndAlso p.Handle <> IntPtr.Zero Then sb _
         .Append(" pWindowText=").Append(clsQuery.EncodeValue(p.WindowText)) _
         .Append(" pClassName=").Append(clsQuery.EncodeValue(p.ClassName)) _
         .Append(" pCtrlID=").Append(p.CtrlID) _
         .Append(" pX=").Append(p.X) _
         .Append(" pY=").Append(p.Y) _
         .Append(" pWidth=").Append(p.Width) _
         .Append(" pHeight=").Append(p.Height) _
         .Append(" pVisible=").Append(p.Visible) _
         .Append(" pScreenVisible=").Append(p.ScreenVisible) _
         .Append(" pEnabled=").Append(p.Enabled) _
         .Append(" pActive=").Append(p.Active) _
         .Append(" pOrdinal=").Append(p.Ordinal) _
         .Append(" pChildCount=").Append(p.Children.Count) _
         .Append(" pStyle=").Append(Style)

        Return sb

    End Function

    ''' <summary>
    ''' Holds a list of properties that can be indexed quickly for a given Identifier type.
    ''' </summary>
    Private Shared mobjProperties As Dictionary(Of clsQuery.IdentifierTypes, Reflection.PropertyInfo)

    ''' <summary>
    ''' Returns a list of properties that can be indexed quickly for a given Identifier type.
    ''' </summary>
    ''' <returns></returns>
    Friend Shared Function GetProperties() As Dictionary(Of clsQuery.IdentifierTypes, Reflection.PropertyInfo)
        If mobjProperties Is Nothing Then
            mobjProperties = New Dictionary(Of clsQuery.IdentifierTypes, Reflection.PropertyInfo)
            For Each pi As System.Reflection.PropertyInfo In GetType(clsUIWindow).GetProperties(Reflection.BindingFlags.Instance Or Reflection.BindingFlags.Public Or Reflection.BindingFlags.IgnoreCase)
                Dim Attributes As Object() = pi.GetCustomAttributes(GetType(clsUIWindow.WindowIdentifierAttribute), False)
                If Attributes IsNot Nothing AndAlso Attributes.Length > 0 Then
                    mobjProperties.Add(CType(Attributes(0), clsUIWindow.WindowIdentifierAttribute).Identifier, pi)
                End If
            Next
        End If

        Return mobjProperties
    End Function
    ''' <summary>
    ''' Class used in property attributes to match up properties of this class
    ''' against parameters in clsQuery.
    ''' </summary>
    <CLSCompliant(False)> _
    Public Class WindowIdentifierAttribute
        Inherits System.Attribute
        Public Identifier As clsQuery.IdentifierTypes
        Public Sub New(ByVal Identifier As clsQuery.IdentifierTypes)
            Me.Identifier = Identifier
        End Sub
    End Class
End Class

Public Class clsUIText : Inherits clsUIEntity

    Public Overrides ReadOnly Property EntityTypeID() As String
        Get
            Return "TEXT"
        End Get
    End Property

    ''' <summary>
    ''' Overloaded property to hide it from the editor.
    ''' </summary>
    <Browsable(False), EditorBrowsable(EditorBrowsableState.Never)>
    Public Overloads Property Handle() As IntPtr
        Get
            Return MyBase.Handle
        End Get
        Set(ByVal value As IntPtr)
            MyBase.Handle = value
        End Set
    End Property

    ''' <summary>
    ''' Overloaded property to hide it from the editor.
    ''' </summary>
    <Browsable(False), EditorBrowsable(EditorBrowsableState.Never)> _
    Public Overloads Property Hwnd() As IntPtr
        Get
            Return MyBase.Hwnd
        End Get
        Set(ByVal value As IntPtr)
            MyBase.Hwnd = value
        End Set
    End Property

    Public Property Text() As String
        Get
            Return msText
        End Get
        Set(ByVal Value As String)
            msText = Value
        End Set
    End Property
    Private msText As String

    Public Property DC() As clsUIDC
        Get
            Return mobjDC
        End Get
        Set(ByVal Value As clsUIDC)
            mobjDC = Value
        End Set
    End Property
    Private mobjDC As clsUIDC

    <Category("Location")> _
    Public Property X() As Integer
        Get
            Return miX
        End Get
        Set(ByVal Value As Integer)
            miX = Value
        End Set
    End Property
    Private miX As Integer

    <Category("Location")> _
    Public Property Y() As Integer
        Get
            Return miY
        End Get
        Set(ByVal Value As Integer)
            miY = Value
        End Set
    End Property
    Private miY As Integer

    <Category("Location")> _
    Public Property Location() As Point
        Get
            Return New Point(miX, miY)
        End Get
        Set(ByVal value As Point)
            miX = value.X
            miY = value.Y
        End Set
    End Property

    <Category("Size")> _
    Public Property Width() As Integer
        Get
            Return miWidth
        End Get
        Set(ByVal Value As Integer)
            miWidth = Value
        End Set
    End Property
    Private miWidth As Integer

    <Category("Size")> _
    Public Property Height() As Integer
        Get
            Return miHeight
        End Get
        Set(ByVal Value As Integer)
            miHeight = Value
        End Set
    End Property
    Private miHeight As Integer

    <Category("Size")> _
    Public Property Size() As Size
        Get
            Return New Size(miWidth, miHeight)
        End Get
        Set(ByVal value As Size)
            miWidth = value.Width
            miHeight = value.Height
        End Set
    End Property

    Friend Overrides Function GetBounds() As RECT
        Dim r As New RECT(miX, miX + miWidth, miY, miY + miHeight)

        If Parent IsNot Nothing Then
            Dim pr As RECT = Parent.GetBounds()
            pr.Top -= 10 ' Why 10? What does this represent?
            r.Offset(pr.Location)
        End If
        Return r
    End Function

    ''' <summary>
    ''' Gets the center of the text
    ''' </summary>
    ReadOnly Property Center As Point
        Get
            Dim r As New Rectangle(Location, Size)
            Return r.Center
        End Get
    End Property

End Class

Public Class clsUIDC : Inherits clsUIEntity

    Public Overrides ReadOnly Property EntityTypeID() As String
        Get
            Return "DC"
        End Get
    End Property

    ''' <summary>
    ''' True if this DC is a client DC, otherwise False meaning it refers to the
    ''' entire window.
    ''' </summary>
    Public Property ClientDC() As Boolean

    ''' <summary>
    ''' References to all the text found within the device context. This is
    ''' primarily used to translate the text to the correct location when the
    ''' BitBlt operation has been accounted for during hooking
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property ChildText As New List(Of clsUIText)

    ''' <summary>
    ''' Alignment flags compatible with windows text alignment values.
    ''' </summary>
    <Flags>
    Private Enum Alignments

        TA_LEFT = 0      '0x00000000
        TA_RIGHT = 2     '0x00000010
        TA_CENTER = 6    '0x00000110

        TA_TOP = 0       '0x00000000
        TA_BOTTOM = 8    '0x00001000
        TA_BASELINE = 24 '0x00011000

        TA_RTLREADING = 256
        TA_NOUPDATECP = 0
        TA_UPDATECP = 1
        TA_MASK = TA_BASELINE + TA_CENTER + TA_UPDATECP + TA_RTLREADING

    End Enum

    ''' <summary>
    ''' Sets the horizontal and vertial alignments of the device context
    ''' based on the windows text aligment constants. The vertical and
    ''' horizontal components are extracted by masking bits.
    ''' </summary>
    Public Property Alignment As Integer = 0

    Public Sub AdjustAlignment(Text As clsUIText, descent As Integer)

        Const VerticalMask = &B111000 '0x00111000
        Const HorizontalMask = &B111  '0x00000111
        Select Case Alignment And VerticalMask
            Case Alignments.TA_TOP
                'Do Nothing
            Case Alignments.TA_BOTTOM
                Text.Y -= Text.Height
            Case Alignments.TA_BASELINE
                Text.Y -= (Text.Height - descent)
        End Select
        Select Case Alignment And HorizontalMask
            Case Alignments.TA_LEFT
                'Do Nothing
            Case Alignments.TA_RIGHT
                Text.X -= Text.Width
            Case Alignments.TA_CENTER
                Text.X -= (Text.Width \ 2)
        End Select
    End Sub

End Class

Friend Class HexConverter
    Inherits Int32Converter

    Public Overloads Overrides Function ConvertTo(ByVal context As System.ComponentModel.ITypeDescriptorContext, ByVal culture As System.Globalization.CultureInfo, ByVal value As Object, ByVal destinationType As System.Type) As Object
        Return CType(value, Int32).ToString("X8")
    End Function
End Class

