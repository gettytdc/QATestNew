namespace AutomateControls.WindowsSupport
{
    public static class WindowsMessage
    {
        #region - Windows Messages -

        public const int WM_NULL = 0x0000;
        public const int WM_CREATE = 0x0001;
        public const int WM_DESTROY = 0x0002;
        public const int WM_MOVE = 0x0003;
        public const int WM_SIZE = 0x0005;
        public const int WM_ACTIVATE = 0x0006;
        public const int WM_SETFOCUS = 0x0007;
        public const int WM_KILLFOCUS = 0x0008;
        public const int WM_ENABLE = 0x000A;
        public const int WM_SETREDRAW = 0x000B;
        public const int WM_SETTEXT = 0x000C;
        public const int WM_GETTEXT = 0x000D;
        public const int WM_GETTEXTLENGTH = 0x000E;
        public const int WM_PAINT = 0x000F;
        public const int WM_CLOSE = 0x0010;

        public const int WM_QUERYENDSESSION = 0x0011;
        public const int WM_QUIT = 0x0012;
        public const int WM_QUERYOPEN = 0x0013;
        public const int WM_ERASEBKGND = 0x0014;
        public const int WM_SYSCOLORCHANGE = 0x0015;
        public const int WM_ENDSESSION = 0x0016;

        public const int WM_SHOWWINDOW = 0x0018;
        public const int WM_WININICHANGE = 0x001A;
        public const int WM_SETTINGCHANGE = WM_WININICHANGE;
        public const int WM_DEVMODECHANGE = 0x001B;
        public const int WM_ACTIVATEAPP = 0x001C;
        public const int WM_FONTCHANGE = 0x001D;
        public const int WM_TIMECHANGE = 0x001E;
        public const int WM_CANCELMODE = 0x001F;
        public const int WM_SETCURSOR = 0x0020;
        public const int WM_MOUSEACTIVATE = 0x0021;
        public const int WM_CHILDACTIVATE = 0x0022;
        public const int WM_QUEUESYNC = 0x0023;
        public const int WM_GETMINMAXINFO = 0x0024;
        public const int WM_PAINTICON = 0x0026;
        public const int WM_ICONERASEBKGND = 0x0027;
        public const int WM_NEXTDLGCTL = 0x0028;
        public const int WM_SPOOLERSTATUS = 0x002A;
        public const int WM_DRAWITEM = 0x002B;
        public const int WM_MEASUREITEM = 0x002C;
        public const int WM_DELETEITEM = 0x002D;
        public const int WM_VKEYTOITEM = 0x002E;
        public const int WM_CHARTOITEM = 0x002F;
        public const int WM_SETFONT = 0x0030;
        public const int WM_GETFONT = 0x0031;
        public const int WM_SETHOTKEY = 0x0032;
        public const int WM_GETHOTKEY = 0x0033;
        public const int WM_QUERYDRAGICON = 0x0037;
        public const int WM_COMPAREITEM = 0x0039;
        public const int WM_GETOBJECT = 0x003D;
        public const int WM_COMPACTING = 0x0041;
        public const int WM_COMMNOTIFY = 0x0044;
        public const int WM_WINDOWPOSCHANGING = 0x0046;
        public const int WM_WINDOWPOSCHANGED = 0x0047;
        public const int WM_POWER = 0x0048;
        public const int WM_NOTIFY = 0x004E;

        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;
        public const int WM_CHAR = 0x0102;

        public const int WM_HSCROLL = 0x0114;
        public const int WM_VSCROLL = 0x0115;
        public const int WM_MOUSEWHEEL = 0x020A;

        public const int WM_PASTE = 0x0302;

        public const int WM_MOUSEMOVE = 0x200;

        public const int WM_LBUTTONDOWN = 0x201;
        public const int WM_LBUTTONUP = 0x202;
        public const int WM_LBUTTONDBLCLK = 0x203;

        public const int WM_RBUTTONDOWN = 0x204;
        public const int WM_RBUTTONUP = 0x205;
        public const int WM_RBUTTONDBLCLK = 0x206;

        public const int WM_MBUTTONDOWN = 0x207;
        public const int WM_MBUTTONUP = 0x208;
        public const int WM_MBUTTONDBLCLK = 0x209;

        #endregion

        #region - Control Messages -

        public const int CCM_FIRST = 0x2000;
        public const int CCM_SETBKCOLOR = 0x2001;
        public const int CCM_SETCOLORSCHEME = 0x2002;
        public const int CCM_GETCOLORSCHEME = 0x2003;
        public const int CCM_GETDROPTARGET = 0x2004;
        public const int CCM_SETUNICODEFORMAT = 0x2005;
        public const int CCM_GETUNICODEFORMAT = 0x2006;
        public const int CCM_SETVERSION = 0x2007;
        public const int CCM_GETVERSION = 0x2008;
        public const int CCM_SETNOTIFYWINDOW = 0x2009;
        public const int CCM_SETWINDOWTHEME = 0x200b;
        public const int CCM_DPISCALE = 0x200c;

        #endregion

        #region - EditControl Messages -

        public const int EC_LEFTMARGIN = 0x0001;
        public const int EC_RIGHTMARGIN = 0x0002;
        public const int EC_USEFONTINFO = 0xFFFF;
        public const int EM_SETMARGINS = 0x00D3;
        public const int EM_GETMARGINS = 0x00D4;
        public const int EM_REPLACESEL = 0x00C2;

        #endregion

        #region - TabControl Messages -

        public const int TCM_FIRST = 0x1300;
        public const int TCM_GETIMAGELIST = 0x1302;
        public const int TCM_SETIMAGELIST = 0x1303;
        public const int TCM_GETITEMCOUNT = 0x1304;
        public const int TCM_GETITEMA = 0x1305;
        public const int TCM_SETITEMA = 0x1306;
        public const int TCM_INSERTITEMA = 0x1307;
        public const int TCM_DELETEITEM = 0x1308;
        public const int TCM_DELETEALLITEMS = 0x1309;
        public const int TCM_GETITEMRECT = 0x130a;
        public const int TCM_GETCURSEL = 0x130b;
        public const int TCM_SETCURSEL = 0x130c;
        public const int TCM_HITTEST = 0x130d;
        public const int TCM_SETITEMEXTRA = 0x130e;

        public const int TCM_ADJUSTRECT = 0x1328;
        public const int TCM_SETITEMSIZE = 0x1329;
        public const int TCM_REMOVEIMAGE = 0x132a;
        public const int TCM_SETPADDING = 0x132b;
        public const int TCM_GETROWCOUNT = 0x132c;
        public const int TCM_GETTOOLTIPS = 0x132d;
        public const int TCM_SETTOOLTIPS = 0x132e;
        public const int TCM_GETCURFOCUS = 0x132f;

        public const int TCM_SETCURFOCUS = 0x1330;
        public const int TCM_SETMINTABWIDTH = 0x1331;
        public const int TCM_DESELECTALL = 0x1332;
        public const int TCM_HIGHLIGHTITEM = 0x1333;
        public const int TCM_SETEXTENDEDSTYLE = 0x1334;
        public const int TCM_GETEXTENDEDSTYLE = 0x1335;
        public const int TCM_GETITEM = 0x133c;
        public const int TCM_SETITEM = 0x133d;
        public const int TCM_INSERTITEM = 0x133e;

        public const int TCM_GETUNICODEFORMAT = CCM_GETUNICODEFORMAT;
        public const int TCM_SETUNICODEFORMAT = CCM_SETUNICODEFORMAT;

        #endregion

    }
}
