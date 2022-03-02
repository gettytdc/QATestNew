using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace AutomateControls
{
    /// <summary>
    /// Summary description for UIMethods.
    /// </summary>
    public class UIMethods
    {

        [StructLayout(LayoutKind.Sequential)]
            private struct TRACKMOUSEEVENT 
        {
            internal int        size;
            internal TMEFlags   dwFlags;
            internal IntPtr     hWnd;
            internal int        dwHoverTime;
        }

        [Flags]
            private enum TMEFlags 
        {
            TME_HOVER       = 0x00000001,
            TME_LEAVE       = 0x00000002,
            TME_QUERY       = unchecked((int)0x40000000),
            TME_CANCEL      = unchecked((int)0x80000000)
        }

        [StructLayout(LayoutKind.Sequential)]
            internal struct RECT 
        {
            internal int        left;
            internal int        top;
            internal int        right;
            internal int        bottom;
            public  override string ToString() 
            {
                return String.Format("RECT left={0}, top={1}, right={2}, bottom={3}, width={4}, height={5}", left, top, right, bottom, right-left, bottom-top);
            }

        }

        [Flags]
            private enum ScrollWindowExFlags 
        {
            SW_NONE             = 0x0000,
            SW_SCROLLCHILDREN       = 0x0001,
            SW_INVALIDATE           = 0x0002,
            SW_ERASE            = 0x0004,
            SW_SMOOTHSCROLL         = 0x0010
        }

        internal static void ScrollWindow(IntPtr hwnd, Rectangle rectangle, int XAmount, int YAmount, bool with_children) 
        {
            RECT    rect;

            rect = new RECT();
            rect.left = rectangle.X;
            rect.top = rectangle.Y;
            rect.right = rectangle.Right;
            rect.bottom = rectangle.Bottom;

            Win32ScrollWindowEx(hwnd, XAmount, YAmount, ref rect, ref rect, IntPtr.Zero, IntPtr.Zero, ScrollWindowExFlags.SW_INVALIDATE | ScrollWindowExFlags.SW_ERASE | (with_children ? ScrollWindowExFlags.SW_SCROLLCHILDREN : ScrollWindowExFlags.SW_NONE));
            Win32UpdateWindow(hwnd);
        }

        internal static void ScrollWindow(IntPtr hwnd, int XAmount, int YAmount, bool with_children) 
        {
            Win32ScrollWindowEx(hwnd, XAmount, YAmount, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ScrollWindowExFlags.SW_INVALIDATE | ScrollWindowExFlags.SW_ERASE | (with_children ? ScrollWindowExFlags.SW_SCROLLCHILDREN : ScrollWindowExFlags.SW_NONE));
        }


        [DllImport ("user32.dll", EntryPoint="ScrollWindowEx", CallingConvention=CallingConvention.StdCall)]
        private extern static bool Win32ScrollWindowEx(IntPtr hwnd, int dx, int dy, ref RECT prcScroll, ref RECT prcClip, IntPtr hrgnUpdate, out RECT prcUpdate, ScrollWindowExFlags flags);

        [DllImport ("user32.dll", EntryPoint="ScrollWindowEx", CallingConvention=CallingConvention.StdCall)]
        private extern static bool Win32ScrollWindowEx(IntPtr hwnd, int dx, int dy, ref RECT prcScroll, ref RECT prcClip, IntPtr hrgnUpdate, IntPtr prcUpdate, ScrollWindowExFlags flags);

        [DllImport ("user32.dll", EntryPoint="ScrollWindowEx", CallingConvention=CallingConvention.StdCall)]
        private extern static bool Win32ScrollWindowEx(IntPtr hwnd, int dx, int dy, ref RECT prcScroll, IntPtr prcClip, IntPtr hrgnUpdate, IntPtr prcUpdate, ScrollWindowExFlags flags);

        [DllImport ("user32.dll", EntryPoint="ScrollWindowEx", CallingConvention=CallingConvention.StdCall)]
        private extern static bool Win32ScrollWindowEx(IntPtr hwnd, int dx, int dy, IntPtr prcScroll, IntPtr prcClip, IntPtr hrgnUpdate, IntPtr prcUpdate, ScrollWindowExFlags flags);

        [DllImport ("user32.dll", EntryPoint="UpdateWindow", CallingConvention=CallingConvention.StdCall)]
        private extern static IntPtr Win32UpdateWindow(IntPtr hWnd);


        [DllImport ("user32.dll", EntryPoint="GetWindowLong", CallingConvention=CallingConvention.StdCall)]
        private extern static uint Win32GetWindowLong(IntPtr hwnd, WindowLong index);

        [DllImport ("user32.dll", EntryPoint="SetWindowLong", CallingConvention=CallingConvention.StdCall)]
        private extern static uint Win32SetWindowLong(IntPtr hwnd, WindowLong index, uint value);

        [DllImport ("user32.dll", EntryPoint="SetWindowPos", CallingConvention=CallingConvention.StdCall)]
        internal extern static bool Win32SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, SetWindowPosFlags Flags);

        [DllImport ("user32.dll", EntryPoint="SetWindowPos", CallingConvention=CallingConvention.StdCall)]
        internal extern static bool Win32SetWindowPos(IntPtr hWnd, SetWindowPosZOrder pos, int x, int y, int cx, int cy, SetWindowPosFlags Flags);

        [DllImport ("user32.dll", EntryPoint="TrackMouseEvent", CallingConvention=CallingConvention.StdCall)]
        private extern static bool Win32TrackMouseEvent(ref TRACKMOUSEEVENT tme);

    
        [DllImport("UxTheme.dll", CharSet=CharSet.Auto, SetLastError=true)]
        private static extern IntPtr OpenThemeData(IntPtr hWnd, [MarshalAs(UnmanagedType.VBByRefStr)] ref string classList);
 
        [DllImport("UxTheme.dll", CharSet=CharSet.Auto, SetLastError=true)]
        private static extern void CloseThemeData(IntPtr hTheme);
 
        [DllImport("UxTheme.dll", CharSet=CharSet.Auto, SetLastError=true)]
        private static extern void DrawThemeBackground(IntPtr hTheme, IntPtr hDC, int partId, int stateId, ref RECT pRect, ref RECT pClipRect);

            
        internal enum SetWindowPosZOrder 
        {
            HWND_TOP            = 0,
            HWND_BOTTOM         = 1,
            HWND_TOPMOST            = -1,
            HWND_NOTOPMOST          = -2
        }
        
        [Flags]
            private enum WindowLong 
        {
            GWL_WNDPROC             = -4,
            GWL_HINSTANCE           = -6,
            GWL_HWNDPARENT              = -8,
            GWL_STYLE                   = -16,
            GWL_EXSTYLE                 = -20,
            GWL_USERDATA            = -21,
            GWL_ID              = -12
        }

        [Flags]
            internal enum SetWindowPosFlags 
        {
            SWP_ASYNCWINDOWPOS      = 0x4000, 
            SWP_DEFERERASE          = 0x2000,
            SWP_DRAWFRAME           = 0x0020,
            SWP_FRAMECHANGED        = 0x0020,
            SWP_HIDEWINDOW          = 0x0080,
            SWP_NOACTIVATE          = 0x0010,
            SWP_NOCOPYBITS          = 0x0100,
            SWP_NOMOVE          = 0x0002,
            SWP_NOOWNERZORDER       = 0x0200,
            SWP_NOREDRAW            = 0x0008,
            SWP_NOREPOSITION        = 0x0200,
            SWP_NOENDSCHANGING      = 0x0400,
            SWP_NOSIZE          = 0x0001,
            SWP_NOZORDER            = 0x0004,
            SWP_SHOWWINDOW          = 0x0040
        }


        internal enum Msg 
        {
            WM_NULL                   = 0x0000,
            WM_CREATE                 = 0x0001,
            WM_DESTROY                = 0x0002,
            WM_MOVE                   = 0x0003,
            WM_SIZE                   = 0x0005,
            WM_ACTIVATE               = 0x0006,
            WM_SETFOCUS               = 0x0007,
            WM_KILLFOCUS              = 0x0008,
            //              public const uint WM_SETVISIBLE           = 0x0009;
            WM_ENABLE                 = 0x000A,
            WM_SETREDRAW              = 0x000B,
            WM_SETTEXT                = 0x000C,
            WM_GETTEXT                = 0x000D,
            WM_GETTEXTLENGTH          = 0x000E,
            WM_PAINT                  = 0x000F,
            WM_CLOSE                  = 0x0010,
            WM_QUERYENDSESSION        = 0x0011,
            WM_QUIT                   = 0x0012,
            WM_QUERYOPEN              = 0x0013,
            WM_ERASEBKGND             = 0x0014,
            WM_SYSCOLORCHANGE         = 0x0015,
            WM_ENDSESSION             = 0x0016,
            //              public const uint WM_SYSTEMERROR          = 0x0017;
            WM_SHOWWINDOW             = 0x0018,
            WM_CTLCOLOR               = 0x0019,
            WM_WININICHANGE           = 0x001A,
            WM_SETTINGCHANGE          = 0x001A,
            WM_DEVMODECHANGE          = 0x001B,
            WM_ACTIVATEAPP            = 0x001C,
            WM_FONTCHANGE             = 0x001D,
            WM_TIMECHANGE             = 0x001E,
            WM_CANCELMODE             = 0x001F,
            WM_SETCURSOR              = 0x0020,
            WM_MOUSEACTIVATE          = 0x0021,
            WM_CHILDACTIVATE          = 0x0022,
            WM_QUEUESYNC              = 0x0023,
            WM_GETMINMAXINFO          = 0x0024,
            WM_PAINTICON              = 0x0026,
            WM_ICONERASEBKGND         = 0x0027,
            WM_NEXTDLGCTL             = 0x0028,
            //              public const uint WM_ALTTABACTIVE         = 0x0029;
            WM_SPOOLERSTATUS          = 0x002A,
            WM_DRAWITEM               = 0x002B,
            WM_MEASUREITEM            = 0x002C,
            WM_DELETEITEM             = 0x002D,
            WM_VKEYTOITEM             = 0x002E,
            WM_CHARTOITEM             = 0x002F,
            WM_SETFONT                = 0x0030,
            WM_GETFONT                = 0x0031,
            WM_SETHOTKEY              = 0x0032,
            WM_GETHOTKEY              = 0x0033,
            //              public const uint WM_FILESYSCHANGE        = 0x0034;
            //              public const uint WM_ISACTIVEICON         = 0x0035;
            //              public const uint WM_QUERYPARKICON        = 0x0036;
            WM_QUERYDRAGICON          = 0x0037,
            WM_COMPAREITEM            = 0x0039,
            //              public const uint WM_TESTING              = 0x003a;
            //              public const uint WM_OTHERWINDOWCREATED = 0x003c;
            WM_GETOBJECT              = 0x003D,
            //                      public const uint WM_ACTIVATESHELLWINDOW        = 0x003e;
            WM_COMPACTING             = 0x0041,
            WM_COMMNOTIFY             = 0x0044 ,
            WM_WINDOWPOSCHANGING      = 0x0046,
            WM_WINDOWPOSCHANGED       = 0x0047,
            WM_POWER                  = 0x0048,
            WM_COPYDATA               = 0x004A,
            WM_CANCELJOURNAL          = 0x004B,
            WM_NOTIFY                 = 0x004E,
            WM_INPUTLANGCHANGEREQUEST = 0x0050,
            WM_INPUTLANGCHANGE        = 0x0051,
            WM_TCARD                  = 0x0052,
            WM_HELP                   = 0x0053,
            WM_USERCHANGED            = 0x0054,
            WM_NOTIFYFORMAT           = 0x0055,
            WM_CONTEXTMENU            = 0x007B,
            WM_STYLECHANGING          = 0x007C,
            WM_STYLECHANGED           = 0x007D,
            WM_DISPLAYCHANGE          = 0x007E,
            WM_GETICON                = 0x007F,

            // Non-Client messages
            WM_SETICON                = 0x0080,
            WM_NCCREATE               = 0x0081,
            WM_NCDESTROY              = 0x0082,
            WM_NCCALCSIZE             = 0x0083,
            WM_NCHITTEST              = 0x0084,
            WM_NCPAINT                = 0x0085,
            WM_NCACTIVATE             = 0x0086,
            WM_GETDLGCODE             = 0x0087,
            WM_SYNCPAINT              = 0x0088,
            //              public const uint WM_SYNCTASK       = 0x0089;
            WM_NCMOUSEMOVE            = 0x00A0,
            WM_NCLBUTTONDOWN          = 0x00A1,
            WM_NCLBUTTONUP            = 0x00A2,
            WM_NCLBUTTONDBLCLK        = 0x00A3,
            WM_NCRBUTTONDOWN          = 0x00A4,
            WM_NCRBUTTONUP            = 0x00A5,
            WM_NCRBUTTONDBLCLK        = 0x00A6,
            WM_NCMBUTTONDOWN          = 0x00A7,
            WM_NCMBUTTONUP            = 0x00A8,
            WM_NCMBUTTONDBLCLK        = 0x00A9,
            //              public const uint WM_NCXBUTTONDOWN    = 0x00ab;
            //              public const uint WM_NCXBUTTONUP      = 0x00ac;
            //              public const uint WM_NCXBUTTONDBLCLK  = 0x00ad;
            WM_KEYDOWN                = 0x0100,
            WM_KEYFIRST               = 0x0100,
            WM_KEYUP                  = 0x0101,
            WM_CHAR                   = 0x0102,
            WM_DEADCHAR               = 0x0103,
            WM_SYSKEYDOWN             = 0x0104,
            WM_SYSKEYUP               = 0x0105,
            WM_SYSCHAR                = 0x0106,
            WM_SYSDEADCHAR            = 0x0107,
            WM_KEYLAST                = 0x0108,
            WM_IME_STARTCOMPOSITION   = 0x010D,
            WM_IME_ENDCOMPOSITION     = 0x010E,
            WM_IME_COMPOSITION        = 0x010F,
            WM_IME_KEYLAST            = 0x010F,
            WM_INITDIALOG             = 0x0110,
            WM_COMMAND                = 0x0111,
            WM_SYSCOMMAND             = 0x0112,
            WM_TIMER                  = 0x0113,
            WM_HSCROLL                = 0x0114,
            WM_VSCROLL                = 0x0115,
            WM_INITMENU               = 0x0116,
            WM_INITMENUPOPUP          = 0x0117,
            //              public const uint WM_SYSTIMER       = 0x0118;
            WM_MENUSELECT             = 0x011F,
            WM_MENUCHAR               = 0x0120,
            WM_ENTERIDLE              = 0x0121,
            WM_MENURBUTTONUP          = 0x0122,
            WM_MENUDRAG               = 0x0123,
            WM_MENUGETOBJECT          = 0x0124,
            WM_UNINITMENUPOPUP        = 0x0125,
            WM_MENUCOMMAND            = 0x0126,
            //              public const uint WM_CHANGEUISTATE    = 0x0127;
            //              public const uint WM_UPDATEUISTATE    = 0x0128;
            //              public const uint WM_QUERYUISTATE     = 0x0129;

            //              public const uint WM_LBTRACKPOINT     = 0x0131;
            WM_CTLCOLORMSGBOX         = 0x0132,
            WM_CTLCOLOREDIT           = 0x0133,
            WM_CTLCOLORLISTBOX        = 0x0134,
            WM_CTLCOLORBTN            = 0x0135,
            WM_CTLCOLORDLG            = 0x0136,
            WM_CTLCOLORSCROLLBAR      = 0x0137,
            WM_CTLCOLORSTATIC         = 0x0138,
            WM_MOUSEMOVE              = 0x0200,
            WM_MOUSEFIRST                     = 0x0200,
            WM_LBUTTONDOWN            = 0x0201,
            WM_LBUTTONUP              = 0x0202,
            WM_LBUTTONDBLCLK          = 0x0203,
            WM_RBUTTONDOWN            = 0x0204,
            WM_RBUTTONUP              = 0x0205,
            WM_RBUTTONDBLCLK          = 0x0206,
            WM_MBUTTONDOWN            = 0x0207,
            WM_MBUTTONUP              = 0x0208,
            WM_MBUTTONDBLCLK          = 0x0209,
            WM_MOUSEWHEEL             = 0x020A,
            WM_MOUSELAST             = 0x020D,
            //              public const uint WM_XBUTTONDOWN      = 0x020B;
            //              public const uint WM_XBUTTONUP        = 0x020C;
            //              public const uint WM_XBUTTONDBLCLK    = 0x020D;
            WM_PARENTNOTIFY           = 0x0210,
            WM_ENTERMENULOOP          = 0x0211,
            WM_EXITMENULOOP           = 0x0212,
            WM_NEXTMENU               = 0x0213,
            WM_SIZING                 = 0x0214,
            WM_CAPTURECHANGED         = 0x0215,
            WM_MOVING                 = 0x0216,
            //              public const uint WM_POWERBROADCAST   = 0x0218;
            WM_DEVICECHANGE           = 0x0219,
            WM_MDICREATE              = 0x0220,
            WM_MDIDESTROY             = 0x0221,
            WM_MDIACTIVATE            = 0x0222,
            WM_MDIRESTORE             = 0x0223,
            WM_MDINEXT                = 0x0224,
            WM_MDIMAXIMIZE            = 0x0225,
            WM_MDITILE                = 0x0226,
            WM_MDICASCADE             = 0x0227,
            WM_MDIICONARRANGE         = 0x0228,
            WM_MDIGETACTIVE           = 0x0229,
            /* D&D messages */
            //              public const uint WM_DROPOBJECT     = 0x022A;
            //              public const uint WM_QUERYDROPOBJECT  = 0x022B;
            //              public const uint WM_BEGINDRAG      = 0x022C;
            //              public const uint WM_DRAGLOOP       = 0x022D;
            //              public const uint WM_DRAGSELECT     = 0x022E;
            //              public const uint WM_DRAGMOVE       = 0x022F;
            WM_MDISETMENU             = 0x0230,
            WM_ENTERSIZEMOVE          = 0x0231,
            WM_EXITSIZEMOVE           = 0x0232,
            WM_DROPFILES              = 0x0233,
            WM_MDIREFRESHMENU         = 0x0234,
            WM_IME_SETCONTEXT         = 0x0281,
            WM_IME_NOTIFY             = 0x0282,
            WM_IME_CONTROL            = 0x0283,
            WM_IME_COMPOSITIONFULL    = 0x0284,
            WM_IME_SELECT             = 0x0285,
            WM_IME_CHAR               = 0x0286,
            WM_IME_REQUEST            = 0x0288,
            WM_IME_KEYDOWN            = 0x0290,
            WM_IME_KEYUP              = 0x0291,
            WM_MOUSEHOVER             = 0x02A1,
            WM_MOUSELEAVE             = 0x02A3,
            WM_CUT                    = 0x0300,
            WM_COPY                   = 0x0301,
            WM_PASTE                  = 0x0302,
            WM_CLEAR                  = 0x0303,
            WM_UNDO                   = 0x0304,
            WM_RENDERFORMAT           = 0x0305,
            WM_RENDERALLFORMATS       = 0x0306,
            WM_DESTROYCLIPBOARD       = 0x0307,
            WM_DRAWCLIPBOARD          = 0x0308,
            WM_PAINTCLIPBOARD         = 0x0309,
            WM_VSCROLLCLIPBOARD       = 0x030A,
            WM_SIZECLIPBOARD          = 0x030B,
            WM_ASKCBFORMATNAME        = 0x030C,
            WM_CHANGECBCHAIN          = 0x030D,
            WM_HSCROLLCLIPBOARD       = 0x030E,
            WM_QUERYNEWPALETTE        = 0x030F,
            WM_PALETTEISCHANGING      = 0x0310,
            WM_PALETTECHANGED         = 0x0311,
            WM_HOTKEY                 = 0x0312,
            WM_PRINT                  = 0x0317,
            WM_PRINTCLIENT            = 0x0318,
            WM_HANDHELDFIRST          = 0x0358,
            WM_HANDHELDLAST           = 0x035F,
            WM_AFXFIRST               = 0x0360,
            WM_AFXLAST                = 0x037F,
            WM_PENWINFIRST            = 0x0380,
            WM_PENWINLAST             = 0x038F,
            WM_APP                    = 0x8000,
            WM_USER                   = 0x0400,

            // Our "private" ones
            WM_MOUSE_ENTER            = 0x0401,
            WM_MOUSE_LEAVE            = 0x0402,
            WM_ASYNC_MESSAGE          = 0x0403,
            WM_REFLECT                = WM_USER + 0x1c00,
            WM_CLOSE_INTERNAL     = WM_USER + 0x1c01
        }

        [Flags]
            internal enum WindowExStyles : int 
        {
            // Extended Styles
            WS_EX_DLGMODALFRAME = 0x00000001,
            WS_EX_DRAGDETECT    = 0x00000002,
            WS_EX_NOPARENTNOTIFY    = 0x00000004,
            WS_EX_TOPMOST       = 0x00000008,
            WS_EX_ACCEPTFILES   = 0x00000010,
            WS_EX_TRANSPARENT   = 0x00000020,

            WS_EX_MDICHILD      = 0x00000040,
            WS_EX_TOOLWINDOW    = 0x00000080,
            WS_EX_WINDOWEDGE    = 0x00000100,
            WS_EX_CLIENTEDGE    = 0x00000200,
            WS_EX_CONTEXTHELP   = 0x00000400,

            WS_EX_RIGHT     = 0x00001000,
            WS_EX_LEFT      = 0x00000000,
            WS_EX_RTLREADING    = 0x00002000,
            WS_EX_LTRREADING    = 0x00000000,
            WS_EX_LEFTSCROLLBAR = 0x00004000,
            WS_EX_LAYERED       = 0x00080000,
            WS_EX_RIGHTSCROLLBAR    = 0x00000000,

            WS_EX_CONTROLPARENT = 0x00010000,
            WS_EX_STATICEDGE    = 0x00020000,
            WS_EX_APPWINDOW     = 0x00040000,
            WS_EX_NOINHERITLAYOUT   = 0x00100000,
            WS_EX_LAYOUTRTL     = 0x00400000,
            WS_EX_COMPOSITED    = 0x02000000,
            WS_EX_NOACTIVATE    = 0x08000000,

            WS_EX_OVERLAPPEDWINDOW  = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,
            WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST
        }

        internal enum WindowStyles : int 
        {
            WS_OVERLAPPED       = 0x00000000,
            WS_POPUP        = unchecked((int)0x80000000),
            WS_CHILD        = 0x40000000,
            WS_MINIMIZE     = 0x20000000,
            WS_VISIBLE      = 0x10000000,
            WS_DISABLED     = 0x08000000,
            WS_CLIPSIBLINGS     = 0x04000000,
            WS_CLIPCHILDREN     = 0x02000000,
            WS_MAXIMIZE     = 0x01000000,
            WS_CAPTION      = 0x00C00000,
            WS_BORDER       = 0x00800000,
            WS_DLGFRAME     = 0x00400000,
            WS_VSCROLL      = 0x00200000,
            WS_HSCROLL      = 0x00100000,
            WS_SYSMENU      = 0x00080000,
            WS_THICKFRAME       = 0x00040000,
            WS_GROUP        = 0x00020000,
            WS_TABSTOP      = 0x00010000,
            WS_MINIMIZEBOX      = 0x00020000,
            WS_MAXIMIZEBOX      = 0x00010000,
            WS_TILED        = 0x00000000,
            WS_ICONIC       = 0x20000000,
            WS_SIZEBOX      = 0x00040000,
            WS_POPUPWINDOW      = unchecked((int)0x80880000),
            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            WS_TILEDWINDOW      = WS_OVERLAPPEDWINDOW,
            WS_CHILDWINDOW      = WS_CHILD,
        }
        
        
        public static void SetBorderStyle(FormBorderStyle border_style, IntPtr handle) 
        {
            uint    style;
            uint    exstyle;

            style = Win32GetWindowLong(handle, WindowLong.GWL_STYLE);
            exstyle = Win32GetWindowLong(handle, WindowLong.GWL_EXSTYLE);

            switch (border_style) 
            {
                case FormBorderStyle.None: 
                {
                    style &= ~(uint)WindowStyles.WS_BORDER;
                    exstyle &= ~(uint)WindowExStyles.WS_EX_CLIENTEDGE;
                    break;
                }

                case FormBorderStyle.FixedSingle: 
                {
                    style |= (uint)WindowStyles.WS_BORDER;
                    exstyle &= ~(uint)WindowExStyles.WS_EX_CLIENTEDGE;
                    break;
                }

                case FormBorderStyle.Fixed3D: 
                {
                    style |= (uint)WindowStyles.WS_BORDER;
                    exstyle |= (uint)WindowExStyles.WS_EX_CLIENTEDGE;
                    break;
                }
            }

            Win32SetWindowLong(handle, WindowLong.GWL_STYLE, style);
            Win32SetWindowLong(handle, WindowLong.GWL_EXSTYLE, exstyle);
            
            Win32SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, 
                SetWindowPosFlags.SWP_FRAMECHANGED | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOACTIVATE);
        }


    
        #region ListView
        // Drawing
        public static   void DrawListViewItems (Graphics dc, Rectangle clip, AutomateListView control)
        {
            bool details = control.View == View.Details;

            dc.FillRectangle (new SolidBrush (control.BackColor), clip);                        
            int first = control.FirstVisibleIndex;  

            for (int i = first; i <= control.LastVisibleIndex; i ++) 
            {                   
//              if (control.GetType() == typeof(TreeList))
//              {
//                  if (clip.IntersectsWith (((TreeList) control).ChildItems[i].GetBounds (ItemBoundsPortion.Entire)))
//                      DrawListViewItem (dc, control, ((TreeList) control).ChildItems [i]);
//              }
//              else
//              {
                        if (clip.IntersectsWith (control.Items[i].GetBounds (ItemBoundsPortion.Entire)))
                            DrawListViewItem (dc, control, control.Items [i]);
                //}
            }
        
            
            // draw the gridlines
            if (details && control.GridLines) 
            {
                int top = (control.HeaderStyle == ColumnHeaderStyle.None) ?
                    2 : control.Font.Height + 2;

                // draw vertical gridlines
                foreach (AutomateColumnHeader col in control.Columns)
                    dc.DrawLine (SystemPens.Control,
                        col.Rect.Right - control.h_marker, top,
                        col.Rect.Right - control.h_marker, control.TotalHeight);
                // draw horizontal gridlines
                AutomateListViewItem last_item = null;
                foreach (AutomateListViewItem item in control.Items) 
                {
                    dc.DrawLine (SystemPens.Control,
                        item.GetBounds (ItemBoundsPortion.Entire).Left, item.GetBounds (ItemBoundsPortion.Entire).Top,
                        control.TotalWidth, item.GetBounds (ItemBoundsPortion.Entire).Top);
                    last_item = item;
                }

                // draw a line after at the bottom of the last item
                if (last_item != null) 
                {
                    dc.DrawLine (SystemPens.Control,
                        last_item.GetBounds (ItemBoundsPortion.Entire).Left,
                        last_item.GetBounds (ItemBoundsPortion.Entire).Bottom,
                        control.TotalWidth,
                        last_item.GetBounds (ItemBoundsPortion.Entire).Bottom);
                }
            }           
            
            // Draw corner between the two scrollbars
            if (control.h_scroll.Visible == true && control.h_scroll.Visible == true) 
            {
                Rectangle rect = new Rectangle ();
                rect.X = control.h_scroll.Location.X + control.h_scroll.Width;
                rect.Width = control.v_scroll.Width;
                rect.Y = control.v_scroll.Location.Y + control.v_scroll.Height;
                rect.Height = control.h_scroll.Height;
                dc.FillRectangle (SystemBrushes.Control, rect);
            }

            Rectangle box_select_rect = control.item_control.BoxSelectRectangle;
            if (!box_select_rect.Size.IsEmpty)
            {
                Pen P = new Pen (SystemColors.ControlText);
                P.DashStyle = DashStyle.Dot;
                dc.DrawRectangle (P, box_select_rect);
            }
        }
        
        public static void DrawListViewHeader (Graphics dc, Rectangle clip, AutomateListView control)
        {   
            bool details = (control.View == View.Details);
                
            // border is drawn directly in the Paint method
            if (details && control.HeaderStyle != ColumnHeaderStyle.None) 
            {               
                dc.FillRectangle (new SolidBrush (control.BackColor),
                    0, 0, control.TotalWidth, control.Font.Height + 5);
                if (control.Columns.Count > 0) 
                {
                    foreach (AutomateColumnHeader col in control.Columns) 
                    {
                        Rectangle rect = col.Rect;
                        rect.X -= control.h_marker;
                        DrawHeader(dc, rect, col.Hover);
                        rect.X += 3;
                        rect.Width -= 8;
                        dc.DrawString(col.Text, DefaultFont, SystemBrushes.ControlText, rect, col.Format);
                    }

                    int right = control.Columns [control.Columns.Count - 1].Rect.Right - control.h_marker;
                    if (right < control.Right) 
                    {
                        Rectangle rect = control.Columns [0].Rect;
                        rect.X = right;
                        rect.Width = control.Right - right;
                        DrawHeader(dc, rect, false);
                                                        
                    }
                }
            }
    }
    

        private static void DrawHeader(Graphics graphics , Rectangle size , bool bHover)
    {
        DrawPrimative(graphics, size, "Header", 1, (int)  (bHover ? 2: 1));
    }


        private static void DrawPrimative(Graphics graphics, Rectangle rectangle, string PartType, int PartNo, int State)
        {
            RECT rect1;
            rect1.left = rectangle.X;
            rect1.top = rectangle.Y;
            rect1.right = rectangle.X + rectangle.Width;
            rect1.bottom = rectangle.Y + rectangle.Height;
            RECT rect2 = rect1;
            IntPtr ptr2 = OpenThemeData(IntPtr.Zero, ref PartType);
            IntPtr ptr1 = graphics.GetHdc();
            DrawThemeBackground(ptr2, ptr1, PartNo, State, ref rect1, ref rect2);
            graphics.ReleaseHdc(ptr1);
            CloseThemeData(ptr2);
        }

 


        public static void DrawListViewHeaderDragDetails (Graphics dc, AutomateListView view, AutomateColumnHeader col, int target_x)
        {
            Rectangle rect = col.Rect;
            rect.X -= view.h_marker;
            Color color = Color.FromArgb (0x7f, SystemColors.ControlDark.R, SystemColors.ControlDark.G, SystemColors.ControlDark.B);
            dc.FillRectangle ( new SolidBrush (color), rect);
            rect.X += 3;
            rect.Width -= 8;
            if (rect.Width <= 0)
                return;
            color = Color.FromArgb (0x7f, SystemColors.ControlText.R, SystemColors.ControlText.G, SystemColors.ControlText.B);
            dc.DrawString (col.Text, DefaultFont, new SolidBrush (color), rect, col.Format);
            dc.DrawLine ( new Pen (SystemColors.Highlight, 2), target_x, 0, target_x, col.Rect.Height);
        }

        public static void DrawListViewItem (Graphics dc, AutomateListView control, AutomateListViewItem item)
        {               
            int col_offset;
            if (control.View == View.Details && control.Columns.Count > 0)
                col_offset = control.Columns [0].Rect.X;
            else
                col_offset = 0;
            
            Rectangle rect_checkrect = item.CheckRectReal;
            rect_checkrect.X += col_offset;
            Rectangle icon_rect = item.GetBounds (ItemBoundsPortion.Icon);
            icon_rect.X += col_offset;
            Rectangle full_rect = item.GetBounds (ItemBoundsPortion.Entire);
            full_rect.X += col_offset;
            Rectangle text_rect = item.GetBounds (ItemBoundsPortion.Label);         
            text_rect.X += col_offset;

            if (control.CheckBoxes) 
            {
                if (control.StateImageList == null) 
                {
                    // Make sure we've got at least a line width of 1
                    int check_wd = Math.Max (3, rect_checkrect.Width / 6);
                    int scale = Math.Max (1, rect_checkrect.Width / 12);

                    // set the checkbox background
                    dc.FillRectangle (SystemBrushes.Window,
                        rect_checkrect);
                    // define a rectangle inside the border area
                    Rectangle rect = new Rectangle (rect_checkrect.X + 2,
                        rect_checkrect.Y + 2,
                        rect_checkrect.Width - 4,
                        rect_checkrect.Height - 4);
                    Pen pen =  new Pen (SystemColors.WindowText, 2);
                    dc.DrawRectangle (pen, rect);

                    // Need to draw a check-mark
                    if (item.Checked) 
                    {
                        Pen check_pen =  new Pen (SystemColors.WindowText, 1);
                        // adjustments to get the check-mark at the right place
                        rect.X ++; rect.Y ++;
                        // following logic is taken from DrawFrameControl method
                        for (int i = 0; i < check_wd; i++) 
                        {
                            dc.DrawLine (check_pen, rect.Left + check_wd / 2,
                                rect.Top + check_wd + i,
                                rect.Left + check_wd / 2 + 2 * scale,
                                rect.Top + check_wd + 2 * scale + i);
                            dc.DrawLine (check_pen,
                                rect.Left + check_wd / 2 + 2 * scale,
                                rect.Top + check_wd + 2 * scale + i,
                                rect.Left + check_wd / 2 + 6 * scale,
                                rect.Top + check_wd - 2 * scale + i);
                        }
                    }
                }
                else 
                {
                    if (item.Checked && control.StateImageList.Images.Count > 1)
                        control.StateImageList.Draw (dc,
                            rect_checkrect.Location, 1);
                    else if (! item.Checked && control.StateImageList.Images.Count > 0)
                        control.StateImageList.Draw (dc,
                            rect_checkrect.Location, 0);
                }
            }

            if (control.View == View.LargeIcon) 
            {
                if (item.ImageIndex > -1 && control.LargeImageList != null &&
                    item.ImageIndex < control.LargeImageList.Images.Count)
                    control.LargeImageList.Draw (dc, icon_rect.Location, item.ImageIndex);
            } 
            else 
            {
                if (item.ImageIndex > -1 && control.SmallImageList != null &&
                    item.ImageIndex < control.SmallImageList.Images.Count)
                    control.SmallImageList.Draw (dc, icon_rect.Location, item.ImageIndex);
            }

            // draw the item text           
            // format for the item text
            StringFormat format = new StringFormat ();
            if (control.View == View.SmallIcon)
                format.LineAlignment = StringAlignment.Near;
            else
                format.LineAlignment = StringAlignment.Center;
            if (control.View == View.LargeIcon)
                format.Alignment = StringAlignment.Center;
            else
                format.Alignment = StringAlignment.Near;
            
            if (!control.LabelWrap)
                format.FormatFlags = StringFormatFlags.NoWrap;
            
            Rectangle highlight_rect = text_rect;
            if (control.View == View.Details && !control.FullRowSelect) 
            {
                Size text_size = Size.Ceiling (dc.MeasureString (item.Text, item.Font));
                highlight_rect.Width = text_size.Width + 4;
            }

            //if (item.Selected && control.Focused)
            if (item.Selected)
                dc.FillRectangle (SystemBrushes.Highlight, highlight_rect);
            else
                dc.FillRectangle (new SolidBrush (item.BackColor), text_rect);

            if (item.Text != null && item.Text.Length > 0) 
            {
                if (item.Selected && control.Focused)
                    dc.DrawString (item.Text, item.Font, SystemBrushes.HighlightText, highlight_rect, format);
                else
                    dc.DrawString (item.Text, item.Font, new SolidBrush
                        (item.ForeColor), text_rect, format);
            }

            if (control.View == View.Details && control.Columns.Count > 0) 
            {
                // draw subitems for details view
                AutomateListViewItem.ListViewSubItemCollection subItems = item.SubItems;
                int count = (control.Columns.Count < subItems.Count ? 
                    control.Columns.Count : subItems.Count);

                if (count > 0) 
                {
                    AutomateColumnHeader col;
                    AutomateListViewItem.ListViewSubItem subItem;
                    Rectangle sub_item_rect = text_rect; 

                    // set the format for subitems
                    format.FormatFlags = StringFormatFlags.NoWrap;

                    // 0th subitem is the item already drawn
                    for (int index = 1; index < count; index++) 
                    {
                        subItem = subItems [index];
                        col = control.Columns [index];
                        format.Alignment = col.Format.Alignment;
                        sub_item_rect.X = col.Rect.X - control.h_marker;
                        sub_item_rect.Width = col.Wd;
                        Rectangle sub_item_text_rect = sub_item_rect;
                        sub_item_text_rect.X += 3;
                        sub_item_text_rect.Width -= 6;

                        SolidBrush sub_item_back_br = null;
                        SolidBrush sub_item_fore_br = null;
                        Font sub_item_font = null;

                        if (item.UseItemStyleForSubItems) 
                        {
                            sub_item_back_br = new SolidBrush (item.BackColor);
                            sub_item_fore_br = new SolidBrush (item.ForeColor);
                            sub_item_font = item.Font;
                        } 
                        else 
                        {
                            sub_item_back_br = new SolidBrush (subItem.BackColor);
                            sub_item_fore_br = new SolidBrush (subItem.ForeColor);
                            sub_item_font = subItem.Font;
                        }

                        if (item.Selected && control.FullRowSelect) 
                        {
                            dc.FillRectangle (SystemBrushes.Highlight, sub_item_rect);
                            if (subItem.Text != null && subItem.Text.Length > 0)
                                dc.DrawString (subItem.Text, sub_item_font,
                                    SystemBrushes.HighlightText,
                                    sub_item_text_rect, format);
                        } 
                        else 
                        {
                            dc.FillRectangle (sub_item_back_br, sub_item_rect);
                            if (subItem.Text != null && subItem.Text.Length > 0)
                                dc.DrawString (subItem.Text, sub_item_font,
                                    sub_item_fore_br,
                                    sub_item_text_rect, format);
                        }
                    }
                }
            }
            
            if (item.Focused && control.Focused) 
            {               
                Rectangle focus_rect = highlight_rect;
                if (control.FullRowSelect && control.View == View.Details) 
                {
                    int width = 0;
                    foreach (AutomateColumnHeader col in control.Columns)
                        width += col.Width;
                    focus_rect = new Rectangle (0, full_rect.Y, width, full_rect.Height);
                }
                if (item.Selected)
                    CPDrawFocusRectangle (dc, focus_rect, SystemColors.HighlightText, SystemColors.Highlight);
                else
                    CPDrawFocusRectangle (dc, focus_rect, control.ForeColor, control.BackColor);
            }

            format.Dispose ();
        }



        // Sizing
        public static   Size ListViewCheckBoxSize 
        {
            get { return new Size (16, 16); }
        }

        public static   int ListViewColumnHeaderHeight 
        {
            get { return 16; }
        }

        public static   int ListViewDefaultColumnWidth 
        {
            get { return 60; }
        }

        public static   int ListViewVerticalSpacing 
        {
            get { return 22; }
        }

        public static   int ListViewEmptyColumnWidth 
        {
            get { return 10; }
        }

        public static   int ListViewHorizontalSpacing 
        {
            get { return 10; }
        }

        public static   Size ListViewDefaultSize 
        {
            get { return new Size (121, 97); }
        }
        #endregion  // ListView
        



        internal  static void ResetMouseHover(IntPtr handle) 
        {
            TRACKMOUSEEVENT tme;

            tme = new TRACKMOUSEEVENT();
            tme.size = Marshal.SizeOf(tme);
            tme.hWnd = handle;
            tme.dwFlags = TMEFlags.TME_LEAVE | TMEFlags.TME_HOVER;
            Win32TrackMouseEvent(ref tme);
        }

        


        public static void CPDrawFocusRectangle (Graphics graphics, Rectangle rectangle, Color foreColor, Color backColor) 
        {           
            Rectangle rect = rectangle;
            Pen pen;
            HatchBrush brush;
                
            if (backColor.GetBrightness () >= 0.5) 
            {
                foreColor = Color.Transparent;
                backColor = Color.Black;
                
            } 
            else 
            {
                backColor = Color.FromArgb (Math.Abs (backColor.R-255), Math.Abs (backColor.G-255), Math.Abs (backColor.B-255));
                foreColor = Color.Black;
            }
                        
            brush = new HatchBrush (HatchStyle.Percent50, backColor, foreColor);
            pen = new Pen (brush, 1);
                        
            rect.Width--;
            rect.Height--;          
            
            graphics.DrawRectangle (pen, rect);
            pen.Dispose ();
        }


        public static void CPDrawButton (Graphics dc, Rectangle rectangle, ButtonState state)
        {
            // sadly enough, the rectangle gets always filled with a hatchbrush
            dc.FillRectangle (new HatchBrush (HatchStyle.Percent50, Color.FromArgb (SystemColors.Control.R + 3, SystemColors.Control.G, SystemColors.Control.B), SystemColors.Control), rectangle.X + 1, rectangle.Y + 1, rectangle.Width - 2, rectangle.Height - 2);
            
            if ((state & ButtonState.All) == ButtonState.All || ((state & ButtonState.Checked) == ButtonState.Checked && (state & ButtonState.Flat) == ButtonState.Flat)) 
            {
                dc.FillRectangle (new HatchBrush (HatchStyle.Percent50, SystemColors.ControlLight, SystemColors.Control), rectangle.X + 2, rectangle.Y + 2, rectangle.Width - 4, rectangle.Height - 4);
                
                dc.DrawRectangle (SystemPens.ControlDark, rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1);
            } 
            else
                if ((state & ButtonState.Flat) == ButtonState.Flat) 
            {
                dc.DrawRectangle (SystemPens.ControlDark, rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1);
            } 
            else
                if ((state & ButtonState.Checked) == ButtonState.Checked) 
            {
                dc.FillRectangle (new HatchBrush (HatchStyle.Percent50, SystemColors.ControlLight, SystemColors.Control), rectangle.X + 2, rectangle.Y + 2, rectangle.Width - 4, rectangle.Height - 4);
                
                Pen pen = SystemPens.ControlDarkDark;
                dc.DrawLine (pen, rectangle.X, rectangle.Y, rectangle.X, rectangle.Bottom - 2);
                dc.DrawLine (pen, rectangle.X + 1, rectangle.Y, rectangle.Right - 2, rectangle.Y);
                
                pen = SystemPens.ControlDark;
                dc.DrawLine (pen, rectangle.X + 1, rectangle.Y + 1, rectangle.X + 1, rectangle.Bottom - 3);
                dc.DrawLine (pen, rectangle.X + 2, rectangle.Y + 1, rectangle.Right - 3, rectangle.Y + 1);
                
                pen = SystemPens.ControlLight;
                dc.DrawLine (pen, rectangle.X, rectangle.Bottom - 1, rectangle.Right - 2, rectangle.Bottom - 1);
                dc.DrawLine (pen, rectangle.Right - 1, rectangle.Y, rectangle.Right - 1, rectangle.Bottom - 1);
            } 
            else
                if (((state & ButtonState.Pushed) == ButtonState.Pushed) && ((state & ButtonState.Normal) == ButtonState.Normal)) 
            {
                Pen pen = SystemPens.ControlDarkDark;
                dc.DrawLine (pen, rectangle.X, rectangle.Y, rectangle.X, rectangle.Bottom - 2);
                dc.DrawLine (pen, rectangle.X + 1, rectangle.Y, rectangle.Right - 2, rectangle.Y);
                
                pen = SystemPens.ControlDark;
                dc.DrawLine (pen, rectangle.X + 1, rectangle.Y + 1, rectangle.X + 1, rectangle.Bottom - 3);
                dc.DrawLine (pen, rectangle.X + 2, rectangle.Y + 1, rectangle.Right - 3, rectangle.Y + 1);
                
                pen = SystemPens.ControlLight;
                dc.DrawLine (pen, rectangle.X, rectangle.Bottom - 1, rectangle.Right - 2, rectangle.Bottom - 1);
                dc.DrawLine (pen, rectangle.Right - 1, rectangle.Y, rectangle.Right - 1, rectangle.Bottom - 1);
            } 
            else
                if (((state & ButtonState.Inactive) == ButtonState.Inactive) || ((state & ButtonState.Normal) == ButtonState.Normal)) 
            {
                Pen pen = SystemPens.ControlLight;
                dc.DrawLine (pen, rectangle.X, rectangle.Y, rectangle.Right - 2, rectangle.Y);
                dc.DrawLine (pen, rectangle.X, rectangle.Y, rectangle.X, rectangle.Bottom - 2);
                
                pen = SystemPens.ControlDark;
                dc.DrawLine (pen, rectangle.X + 1, rectangle.Bottom - 2, rectangle.Right - 2, rectangle.Bottom - 2);
                dc.DrawLine (pen, rectangle.Right - 2, rectangle.Y + 1, rectangle.Right - 2, rectangle.Bottom - 3);
                
                pen = SystemPens.ControlDarkDark;
                dc.DrawLine (pen, rectangle.X, rectangle.Bottom - 1, rectangle.Right - 1, rectangle.Bottom - 1);
                dc.DrawLine (pen, rectangle.Right - 1, rectangle.Y, rectangle.Right - 1, rectangle.Bottom - 2);
            }
        }


        public static Font DefaultFont
    {
            get
            {
        return new Font (FontFamily.GenericSansSerif, 8f);
    }
}

    
    }
}




