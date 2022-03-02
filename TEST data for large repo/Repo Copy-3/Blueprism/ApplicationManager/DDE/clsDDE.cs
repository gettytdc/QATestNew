using System;
using System.Text;
using System.Runtime.InteropServices;

namespace BluePrism.ApplicationManager.DDE
{

    /// Project  : Application Manager
    /// Class    : clsDDE
    /// <summary>
    /// Provides a DDE interface for Application Manager.
    /// </summary>
    internal class clsDDE
    {

        internal enum AfCmd : uint
        {
            APPCLASS_MONITOR=0x00000001u,
            APPCLASS_STANDARD=0x00000000u,
            APPCMD_CLIENTONLY=0x00000010u,
            APPCMD_FILTERINITS=0x00000020u
        }

        internal enum CodePages
        {
            CP_WINANSI=1004,
            CP_WINUNICODE=1200
        }

        internal enum DMLERR : uint
        {
            DMLERR_NO_ERROR=0,
            DMLERR_FIRST=0x4000,
            DMLERR_ADVACKTIMEOUT=0x4000,
            DMLERR_BUSY=0x4001,
            DMLERR_DATAACKTIMEOUT=0x4002,
            DMLERR_DLL_NOT_INITIALIZED=0x4003,
            DMLERR_DLL_USAGE=0x4004,
            DMLERR_EXECACKTIMEOUT=0x4005,
            DMLERR_INVALIDPARAMETER=0x4006,
            DMLERR_LOW_MEMORY=0x4007,
            DMLERR_MEMORY_ERROR=0x4008,
            DMLERR_NOTPROCESSED=0x4009,
            DMLERR_NO_CONV_ESTABLISHED=0x400a,
            DMLERR_POKEACKTIMEOUT=0x400b,
            DMLERR_POSTMSG_FAILED=0x400c,
            DMLERR_REENTRANCY=0x400d,
            DMLERR_SERVER_DIED=0x400e,
            DMLERR_SYS_ERROR=0x400f,
            DMLERR_UNADVACKTIMEOUT=0x4010,
            DMLERR_UNFOUND_QUEUE_ID=0x4011,
            DMLERR_LAST=0x4011
        }

        internal enum DdeTransaction : uint
        {
            XTYPF_NOBLOCK=0x0002,
            XTYPF_NODATA=0x0004,
            XTYPF_ACKREQ=0x0008,

            XCLASS_MASK=0xFC00,
            XCLASS_BOOL=0x1000,
            XCLASS_DATA=0x2000,
            XCLASS_FLAGS=0x4000,
            XCLASS_NOTIFICATION=0x8000,

            XTYP_ERROR=(0x0000|XCLASS_NOTIFICATION|XTYPF_NOBLOCK),
            XTYP_ADVDATA=(0x0010|XCLASS_FLAGS),
            XTYP_ADVREQ=(0x0020|XCLASS_DATA|XTYPF_NOBLOCK),
            XTYP_ADVSTART=(0x0030|XCLASS_BOOL),
            XTYP_ADVSTOP=(0x0040|XCLASS_NOTIFICATION),
            XTYP_EXECUTE=(0x0050|XCLASS_FLAGS),
            XTYP_CONNECT=(0x0060|XCLASS_BOOL|XTYPF_NOBLOCK),
            XTYP_CONNECT_CONFIRM=(0x0070|XCLASS_NOTIFICATION|XTYPF_NOBLOCK),
            XTYP_XACT_COMPLETE=(0x0080|XCLASS_NOTIFICATION),
            XTYP_POKE=(0x0090|XCLASS_FLAGS),
            XTYP_REGISTER=(0x00A0|XCLASS_NOTIFICATION|XTYPF_NOBLOCK),
            XTYP_REQUEST=(0x00B0|XCLASS_DATA),
            XTYP_DISCONNECT=(0x00C0|XCLASS_NOTIFICATION|XTYPF_NOBLOCK),
            XTYP_UNREGISTER=(0x00D0|XCLASS_NOTIFICATION|XTYPF_NOBLOCK),
            XTYP_WILDCONNECT=(0x00E0|XCLASS_DATA|XTYPF_NOBLOCK),
            XTYP_MASK=0x00F0,
            XTYP_SHIFT=4
        }


        [DllImport("user32.dll")]
        internal static extern Boolean DdeAbandonTransaction(UInt32 idInst,UInt32 hConv,Int32 idTransaction);

        [DllImport("user32.dll")]
        internal static extern IntPtr DdeAccessData(Int32 hData,ref int pcbDataSize);

        [DllImport("user32.dll")]
        internal static extern Int32 DdeAddData(Int32 hData,byte pSrc,Int32 cb,Int32 cbOff);

        internal delegate IntPtr DdeCallback(DdeTransaction uType,Int32 uFmt,UInt32 hConv,Int32 hsz1,Int32 hsz2,Int32 hData,IntPtr dwData1,IntPtr dwData2);


        [DllImport("user32.dll")]
        internal static extern Int32 DdeClientTransaction(byte[] pData,Int32 cbData,UInt32 hConv,IntPtr hszItem,CLIPFORMAT wFmt,DdeTransaction wType,UInt32 dwTimeout,ref Int32 pdwResult);

        [DllImport("user32.dll")]
        internal static extern Int32 DdeCmpStringHandles(Int32 hsz1,Int32 hsz2);


        [DllImport("user32.dll",EntryPoint="DdeConnect")]
        internal static extern UInt32 DdeConnect(UInt32 idInst,IntPtr hszService,IntPtr hszTopic,IntPtr nullpointer);

        [DllImport("user32.dll")]
        internal static extern IntPtr DdeConnectList(UInt32 idInst,IntPtr hszService,IntPtr hszTopic,IntPtr ConvList,IntPtr nullpointer);

        [DllImport("user32.dll")]
        internal static extern Int32 DdeCreateDataHandle(UInt32 idInst,byte pSrc,Int32 cb,Int32 cbOff,Int32 hszItem,Int32 wFmt,Int32 afCmd);


        [DllImport("User32.dll",EntryPoint="DdeCreateStringHandle",CharSet=CharSet.Auto)]
        internal static extern IntPtr DdeCreateStringHandle(UInt32 idInst,byte[] psz,CodePages iCodePage);

        [DllImport("user32.dll")]
        internal static extern Boolean DdeDisconnect(UInt32 hConv);

        [DllImport("user32.dll")]
        internal static extern Boolean DdeDisconnectList(IntPtr hConvList);

        [DllImport("user32.dll")]
        internal static extern Int32 DdeEnableCallback(UInt32 idInst,UInt32 hConv,EC wCmd);

        [DllImport("user32.dll")]
        internal static extern Boolean DdeFreeDataHandle(Int32 hData);

        [DllImport("user32.dll")]
        internal static extern Boolean DdeFreeStringHandle(UInt32 idInst,IntPtr hsz);

        [DllImport("user32.dll")]
        internal static extern Int32 DdeGetData(Int32 hData,byte[] pDst,Int32 cbMax,Int32 cbOff);

        [DllImport("user32.dll")]
        internal static extern DMLERR DdeGetLastError(UInt32 idInst);

        [DllImport("user32.dll")]
        internal static extern Boolean DdeImpersonateClient(UInt32 hConv);

        [DllImport("user32.dll")]
        internal static extern Int32 DdeInitialize(ref UInt32 pidInst,DdeCallback pfnCallback,Int32 afCmd,Int32 ulRes);

        [DllImport("user32.dll")]
        internal static extern Boolean DdeKeepStringHandle(UInt32 idInst,Int32 hsz);

        [DllImport("user32.dll")]
        internal static extern Int32 DdeNameService(UInt32 idInst,Int32 hsz1,Int32 hsz2,Int32 afCmd);

        [DllImport("user32.dll")]
        internal static extern Boolean DdePostAdvise(UInt32 idInst,Int32 hszTopic,Int32 hszItem);

        [DllImport("user32.dll")]
        internal static extern IntPtr DdeQueryNextServer(IntPtr hConvList,IntPtr hConvPrev);

        [DllImport("user32.dll")]
        internal static extern uint DdeQueryConvInfo(System.IntPtr hConv,uint idTransaction,ref CONVINFO pConvInfo);

        [DllImport("user32.dll")]
        internal static extern Int32 DdeQueryString(UInt32 idInst,Int32 hsz,StringBuilder psz,Int32 cchMax,CodePages iCodePage);

        [DllImport("user32.dll")]
        internal static extern Int32 DdeReconnect(UInt32 hConv);

        [DllImport("user32.dll")]
        internal static extern Boolean DdeSetUserHandle(UInt32 hConv,Int32 id,IntPtr hUser);

        [DllImport("user32.dll")]
        internal static extern Boolean DdeUnaccessData(Int32 hData);

        [DllImport("user32.dll")]
        internal static extern Boolean DdeUninitialize(UInt32 idInst);


        internal enum SECURITY_CONTEXT_TRACKING_MODE : byte
        {
            SECURITY_STATIC_TRACKING=0,
            SECURITY_DYNAMIC_TRACKING=1
        }


        internal enum CLIPFORMAT : uint
        {
            CF_TEXT=1,
            CF_BITMAP=2,
            CF_HDROP=15
        }

        internal const UInt32 QID_SYNC=0xFFFFFFFF;

        internal const UInt32 TIMEOUT_ASYNC=0xFFFFFFFF;

        internal enum EC : uint
        {
            EC_ENABLEALL=0,
            EC_ENABLEONE=ST.ST_BLOCKNEXT,
            EC_DISABLE=ST.ST_BLOCKED,
            EC_QUERYWAITING=2
        }

        internal enum ST : uint
        {
            ST_CONNECTED=1,
            ST_ADVISE=2,
            ST_ISLOCAL=4,
            ST_BLOCKED=8,
            ST_CLIENT=16,
            ST_TERMINATED=32,
            ST_INLIST=64,
            ST_BLOCKNEXT=128,
            ST_ISSELF=256
        }



        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
        internal struct CONVINFO
        {

            /// DWORD->unsigned int
            public uint cb;
            //public int cb;

            /// DWORD_PTR->ULONG_PTR->unsigned int
            public uint hUser;

            /// HCONV->HCONV__*
            public System.IntPtr hConvPartner;

            /// HSZ->HSZ__*
            public System.IntPtr hszSvcPartner;

            /// HSZ->HSZ__*
            public System.IntPtr hszServiceReq;

            /// HSZ->HSZ__*
            public System.IntPtr hszTopic;

            /// HSZ->HSZ__*
            public System.IntPtr hszItem;

            /// UINT->unsigned int
            public uint wFmt;

            /// UINT->unsigned int
            public uint wType;

            /// UINT->unsigned int
            public uint wStatus;

            /// UINT->unsigned int
            public uint wConvst;

            /// UINT->unsigned int
            public uint wLastError;

            /// HCONVLIST->HCONVLIST__*
            public System.IntPtr hConvList;

            /// CONVCONTEXT->tagCONVCONTEXT
            public CONVCONTEXT ConvCtxt;

            /// HWND->HWND__*
            public System.IntPtr hwnd;

            /// HWND->HWND__*
            public System.IntPtr hwndPartner;
        }

        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
        internal struct CONVCONTEXT
        {

            /// UINT->unsigned int
            public uint cb;

            /// UINT->unsigned int
            public uint wFlags;

            /// UINT->unsigned int
            public uint wCountryID;

            /// int
            public int iCodePage;

            /// DWORD->unsigned int
            public uint dwLangID;

            /// DWORD->unsigned int
            public uint dwSecurity;

            /// SECURITY_QUALITY_OF_SERVICE->_SECURITY_QUALITY_OF_SERVICE
            public SECURITY_QUALITY_OF_SERVICE qos;
        }



        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
        internal struct SECURITY_QUALITY_OF_SERVICE
        {

            /// DWORD->unsigned int
            public uint Length;

            /// SECURITY_IMPERSONATION_LEVEL->_SECURITY_IMPERSONATION_LEVEL
            public SECURITY_IMPERSONATION_LEVEL ImpersonationLevel;

            /// SECURITY_CONTEXT_TRACKING_MODE->BOOLEAN->BYTE->unsigned char
            public byte ContextTrackingMode;

            /// BOOLEAN->BYTE->unsigned char
            public byte EffectiveOnly;
        }

        internal enum SECURITY_IMPERSONATION_LEVEL
        {
            SecurityAnonymous,
            SecurityIdentification,
            SecurityImpersonation,
            SecurityDelegation,
        }


        /// <summary>
        /// The timeout, in milliseconds, to be used during DDE transactions.
        /// </summary>
        internal const Int32 TransactionTimeout=10000;

        /// <summary>
        /// Reads the value of the supplied string, from its handle.
        /// </summary>
        /// <param name="Instance">The instance under which the handle was
        /// created.</param>
        /// <param name="Handle">The handle to the string of interest.</param>
        /// <returns>Returns the value represented by the handle.</returns>
        internal static string ReadString(UInt32 Instance,IntPtr Handle)
        {
            Int32 len=DdeQueryString(Instance,Handle.ToInt32(),null,0,CodePages.CP_WINANSI);
            if(len>0)
            {
                System.Text.StringBuilder sb=new System.Text.StringBuilder(1+2*len);
                Int32 RetVal=DdeQueryString(Instance,Handle.ToInt32(),sb,sb.Capacity,CodePages.CP_WINANSI);
                if(RetVal>0)
                {
                    return sb.ToString();
                }
                else
                {
                    DoError(Instance,"Failed to retrieve string value");
                    return "";
                }
            }
            else
            {
                DoError(Instance,"DdeQueryString reports zero length string");
                return "";
            }
        }

        /// <summary>
        /// Throws an exception with the specified message, appending the latest DDE
        /// Error Code.
        /// </summary>
        /// <param name="Instance">The instance under which the error occurred.</param>
        /// <param name="Message">The message content of the error.</param>
        internal static void DoError(UInt32 Instance,string Message)
        {
            clsDDE.DMLERR ERR=clsDDE.DdeGetLastError(Instance);
            throw new InvalidOperationException(Message+" - Last DDE error was "+ERR.ToString());
        }

    }

}
