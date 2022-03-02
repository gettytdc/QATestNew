#include "VirtualChannelListener.h"

//=============================================================================
//==   Definitions
//=============================================================================
#ifdef DEBUG
#pragma optimize ("", off)	// turn off optimization
#endif // DEBUG

#define SIZEOF_CONSOLIDATION_BUFFER 2000		// size of the consolidation buffer to allocate (arbitrary 2000 for the sample).
#define NUMBER_OF_MEMORY_SECTIONS 1             // number of memory buffers to send as a single packet

//=============================================================================
//==   Functions Defined
//=============================================================================

int DriverOpen(PVD, PVDOPEN, PUINT16);
int DriverClose(PVD, PDLLCLOSE, PUINT16);
int DriverInfo(PVD, PDLLINFO, PUINT16);
int DriverPoll(PVD, PVOID, PUINT16);
int DriverQueryInformation(PVD, PVDQUERYINFORMATION, PUINT16);
int DriverSetInformation(PVD, PVDSETINFORMATION, PUINT16);
int DriverGetLastError(PVD, PVDLASTERROR);

static void WFCAPI ICADataArrival(PVOID, USHORT, LPBYTE, USHORT);

//=============================================================================
//==   Data
//=============================================================================

PVOID g_pWd = NULL;                         // returned when we register our hook
PQUEUEVIRTUALWRITEPROC g_pQueueVirtualWrite = NULL;  // returned when we register our hook
BOOL g_fBufferEmpty = TRUE;					// True if the data buffer is empty

USHORT g_usMaxDataSize = 0;                 // Maximum Data Write Size
HANDLE g_ClientHandle;
USHORT g_usVirtualChannelNum = 0;           // Channel number assigned by WD
BOOL g_fIsHpc = FALSE;                      // T: The engine is HPC
PSENDDATAPROC g_pSendData = NULL;           // pointer to the HPC engine SendData function.  Returned when we register our hook.
LPBYTE g_pbaConsolidationBuffer = NULL;     // buffer to consolidate a write that spans the end and beginning of the ring buffer.
MEMORY_SECTION g_MemorySections[NUMBER_OF_MEMORY_SECTIONS];  // memory buffer pointer array
ULONG g_ulUserData = 0xCAACCAAC;      		// sample user data for HP
volatile BOOL appManServiceExists = FALSE;           //flag to track the current state of Appman.Service

/*******************************************************************************
 *
 *  DriverOpen
 *
 *    Called once to set up things.
 *
 * ENTRY:
 *    pVd (input)
 *       pointer to virtual driver data structure
 *    pVdOpen (input/output)
 *       pointer to the structure VDOPEN
 *    puiSize (Output)
 *       size of VDOPEN structure.
 *
 * EXIT:
 *    CLIENT_STATUS_SUCCESS - no error
 *    CLIENT_ERROR_NO_MEMORY - could not allocate data buffer
 *    On other errors, an error code is returned from lower level functions.
 *
 ******************************************************************************/

 // Need to link with Ws2_32.lib
#pragma comment (lib, "Ws2_32.lib")

static SOCKET ClientSocket = (SOCKET)0;

DWORD WINAPI ListenThread(LPVOID lpPram)
{
    while (TRUE) {
        Sleep(1000);
        if(appManServiceExists)
        {
            WSADATA wsaData;
            WSAStartup(MAKEWORD(2, 2), &wsaData);

            struct addrinfo hints;
            ZeroMemory(&hints, sizeof(hints));
            hints.ai_family = AF_INET;
            hints.ai_socktype = SOCK_STREAM;
            hints.ai_protocol = IPPROTO_TCP;
            hints.ai_flags = AI_PASSIVE;

            struct addrinfo* result = NULL;
            const int iResult = getaddrinfo(NULL, "31926", &hints, &result);
            if (iResult != 0) {
                if(result != NULL)
                    freeaddrinfo(result);
                WSACleanup();
                return FALSE;
            }

            const SOCKET ListenSocket = socket(result->ai_family, result->ai_socktype, result->ai_protocol);

            bind(ListenSocket, result->ai_addr, (int)result->ai_addrlen);
            if (result != NULL)
            {
                freeaddrinfo(result);
                result = NULL;
            }

            listen(ListenSocket, SOMAXCONN);

            ClientSocket = accept(ListenSocket, NULL, NULL);

            closesocket(ListenSocket);

                char receiveBuffer[512];
                int receiveBufferLength = 512;
                while (TRUE) {
                    int bytesrecived = recv(ClientSocket, receiveBuffer, receiveBufferLength, 0);
                    if (bytesrecived > 0) {
                        g_MemorySections[0].pSection = (LPBYTE)receiveBuffer;
                        g_MemorySections[0].length = (USHORT)bytesrecived;
                        g_fBufferEmpty = FALSE;
                        _SendAvailableData();
                    }
                    else if (bytesrecived == 0) {
                        break;
                    }
                    else {
                        closesocket(ClientSocket);
                        WSACleanup();
                        break;
                    }
                }
        }
    }
}

void CreateListenerThread()
{
    DWORD threadId;
    CreateThread(NULL, 0, ListenThread, NULL, 0, &threadId);
}

void HandshakeThread(LPVOID lpPram)
{
    while (!appManServiceExists)
    {
        char receiveBuffer[12] = "AppmanInit\r\n";
        g_MemorySections[0].pSection = (LPBYTE)receiveBuffer;
        g_MemorySections[0].length = 12;
        g_fBufferEmpty = FALSE;
        _SendAvailableData();
        Sleep(1000);
    }
}

void CreateHandshakeThread()
{
    DWORD threadId;
    CreateThread(NULL, 0, HandshakeThread, NULL, 0, &threadId);
}



int DriverOpen(PVD pVd, PVDOPEN pVdOpen, PUINT16 puiSize)
{
    WDSETINFORMATION   wdsi;
    VDWRITEHOOK        vdwh;
    VDWRITEHOOKEX      vdwhex;					// struct for getting more engine information, used by HPC
    WDQUERYINFORMATION wdqi;
    OPENVIRTUALCHANNEL OpenVirtualChannel;
    UINT16             uiSize;
        
    CreateListenerThread();
    g_fBufferEmpty = TRUE;

    *puiSize = sizeof(VDOPEN);

    // Get a virtual channel (this is the channel to the server)
    wdqi.WdInformationClass = WdOpenVirtualChannel;
    wdqi.pWdInformation = &OpenVirtualChannel;
    wdqi.WdInformationLength = sizeof(OPENVIRTUALCHANNEL);

    OpenVirtualChannel.pVCName = BLUEPRISM_VIRTUAL_CHANNEL_NAME;
    // uiSize will be set  when we return back from VdCallWd.
    uiSize = sizeof(WDQUERYINFORMATION);

    int rc = VdCallWd(pVd, WDxQUERYINFORMATION, &wdqi, &uiSize);
    
    if (rc != CLIENT_STATUS_SUCCESS)
    {
        return(rc);
    }

    g_usVirtualChannelNum = OpenVirtualChannel.Channel;

    pVd->pPrivate = NULL; /* pointer to private data, if needed */

    // Register write hooks for our virtual channel

    vdwh.Type = g_usVirtualChannelNum;
    vdwh.pVdData = pVd;
    vdwh.pProc = (PVDWRITEPROCEDURE)ICADataArrival;
    wdsi.WdInformationClass = WdVirtualWriteHook;
    wdsi.pWdInformation = &vdwh;
    wdsi.WdInformationLength = sizeof(VDWRITEHOOK);
    uiSize = sizeof(WDSETINFORMATION);

    rc = VdCallWd(pVd, WDxSETINFORMATION, &wdsi, &uiSize);
   
    if (CLIENT_STATUS_SUCCESS != rc)
    {
        return(rc);
    }
    g_pWd = vdwh.pWdData;										// get the pointer to the WD data
    g_pQueueVirtualWrite = vdwh.pQueueVirtualWriteProc;			// grab pointer to function to use to send data to the host

    // Do extra initialization to determine if we are talking to an HPC client

    wdsi.WdInformationClass = WdVirtualWriteHookEx;
    wdsi.pWdInformation = &vdwhex;
    wdsi.WdInformationLength = sizeof(VDWRITEHOOKEX);
    vdwhex.usVersion = HPC_VD_API_VERSION_LEGACY;				// Set version to 0; older clients will do nothing
    rc = VdCallWd(pVd, WDxQUERYINFORMATION, &wdsi, &uiSize);

    if (CLIENT_STATUS_SUCCESS != rc)
    {
        return(rc);
    }
    g_fIsHpc = (HPC_VD_API_VERSION_LEGACY != vdwhex.usVersion);	// if version returned, this is HPC or later
    g_pSendData = vdwhex.pSendDataProc;         // save HPC SendData API address

    // If it is an HPC client, tell it the highest version of the HPC API we support.

    if (g_fIsHpc)
    {
        WDSET_HPC_PROPERITES hpcProperties;

        hpcProperties.usVersion = HPC_VD_API_VERSION_V1;
        hpcProperties.pWdData = g_pWd;
        hpcProperties.ulVdOptions = HPC_VD_OPTIONS_NO_POLLING;
        wdsi.WdInformationClass = WdHpcProperties;
        wdsi.pWdInformation = &hpcProperties;
        wdsi.WdInformationLength = sizeof(WDSET_HPC_PROPERITES);

        rc = VdCallWd(pVd, WDxSETINFORMATION, &wdsi, &uiSize);
        if (CLIENT_STATUS_SUCCESS != rc)
        {
            return(rc);
        }
    }
    CreateHandshakeThread();

    return(CLIENT_STATUS_SUCCESS);
}

/*******************************************************************************
 *
 *  ICADataArrival
 *
 *   A data PDU arrived over our channel.
 *
 * ENTRY:
 *    pVd (input)
 *       pointer to virtual driver data structure
 *
 *    uChan (input)
 *       ICA channel the data is for.
 *
 *    pBuf (input)
 *       Buffer with arriving data packet
 *
 *    Length (input)
 *       Length of the data packet
 *
 * EXIT:
 *       void
 *
 ******************************************************************************/

static void SendDataToBluePrism(LPBYTE pBuf, USHORT Length)
{
    if (ClientSocket)
    {
        const int iSendResult = send(ClientSocket, pBuf, Length, 0);
        if (iSendResult == SOCKET_ERROR) {
            closesocket(ClientSocket);
            WSACleanup();
        }
    }
}

static void WFCAPI ICADataArrival(PVOID pVd, USHORT uChan, LPBYTE pBuf, USHORT Length)
{
    appManServiceExists = TRUE;
    SendDataToBluePrism(pBuf, Length);
}

/*******************************************************************************
 *
 *  DriverPoll
 *
 *  The Winstation driver calls DriverPoll
 *
 * ENTRY:
 *    pVd (input)
 *       pointer to virtual driver data structure
 *    pVdPoll (input)
 *       pointer to the structure DLLPOLL or DLL_HPC_POLL
 *    puiSize (input)
 *       size of DLLPOOL structure.
 *
 * EXIT:
 *    CLIENT_STATUS_SUCCESS - OK so far.  We will be polled again.
 *    CLIENT_STATUS_NO_DATA - No more data to send.
 *    CLIENT_STATUS_ERROR_RETRY - Could not send the data to the WD's output queue - no space.
 *                                Hopefully, space will be available next time.
 *    Otherwise, a fatal error code is returned from lower level functions.
 *
 *    NOTE:  CLIENT_STATUS_NO_DATA signals the WD that it is OK to stop
 *           polling until another host to client packet comes through.
 *
 * REMARKS:
 *    If polling is enabled (pre-HPC client, or HPC client with polling enabled),
 *    this function is called regularly.  DriverPoll is always called at least once.
 *    Otherwise (HPC with polling disabled), DriverPoll is called only when requested
 *    via WDSET_REQUESTPOLL or SENDDATA_NOTIFY.
 *
 ******************************************************************************/

int DriverPoll(PVD pVd, PDLLPOLL pVdPoll, PUINT16 puiSize)
{
    int rc = CLIENT_STATUS_NO_DATA;
    PDLLPOLL pVdPollLegacy;             // legacy DLLPOLL structure pointer
    PDLL_HPC_POLL pVdPollHpc;           // DLL_HPC_POLL structure pointer
    static BOOL fFirstTimeDebug = TRUE;  /* Only print on first invocation */

    
    if (g_fBufferEmpty)
    {
        rc = CLIENT_STATUS_NO_DATA;
        goto Exit;
    }

    // Data is available to write.  Send it.  Check for new HPC write API.

    if (g_fIsHpc)
    {
        rc = _SendAvailableData();                      // send data via HPC client
    }
    else
    {
        // Use the legacy QueueVirtualWrite interface
        // Note that the FLUSH_IMMEDIATELY control will attempt to put the data onto the wire immediately,
        // causing any existing equal or higher priority data in the queue to be flushed as well.
        // This may result in the use of very small wire packets.  Using the value !FLUSH_IMMEDIATELY
        // may result in the data being delayed for a short while (up to 50 ms?) so it can possibly be combined
        // with other subsequent data to result in fewer and larger packets.

        rc = g_pQueueVirtualWrite(g_pWd, g_usVirtualChannelNum, g_MemorySections, NUMBER_OF_MEMORY_SECTIONS, FLUSH_IMMEDIATELY);

        // Normal status returns are CLIENT_STATUS_SUCCESS (it worked) or CLIENT_ERROR_NO_OUTBUF (no room in output queue)

        if (CLIENT_STATUS_SUCCESS == rc)
        {
            g_fBufferEmpty = TRUE;
        }
        else if (CLIENT_ERROR_NO_OUTBUF == rc)
        {
            rc = CLIENT_STATUS_ERROR_RETRY;            // Try again later
        }
    }
Exit:
    return(rc);
}

/*******************************************************************************
 *
 *  DriverClose
 *
 *  The user interface calls VdClose to close a Vd before unloading it.
 *
 * ENTRY:
 *    pVd (input)
 *       pointer to procotol driver data structure
 *    pVdClose (input/output)
 *       pointer to the structure DLLCLOSE
 *    puiSize (input)
 *       size of DLLCLOSE structure.
 *
 * EXIT:
 *    CLIENT_STATUS_SUCCESS - no error
 *
 ******************************************************************************/

int DriverClose(PVD pVd, PDLLCLOSE pVdClose, PUINT16 puiSize)
{    
    return(CLIENT_STATUS_SUCCESS);
}

/*******************************************************************************
 *
 *  DriverInfo
 *
 *    This routine is called to get module information
 *
 * ENTRY:
 *    pVd (input)
 *       pointer to virtual driver data structure
 *    pVdInfo (output)
 *       pointer to the structure DLLINFO
 *    puiSize (output)
 *       size of DLLINFO structure
 *
 * EXIT:
 *    CLIENT_STATUS_SUCCESS - no error
 *
 ******************************************************************************/

int DriverInfo(PVD pVd, PDLLINFO pVdInfo, PUINT16 puiSize)
{
    USHORT ByteCount;
    PVDPING_C2H pVdData;
    PMODULE_C2H pHeader;
    PVDFLOW pFlow;

    ByteCount = sizeof(VDPING_C2H);

    *puiSize = sizeof(DLLINFO);

    
    // Check if buffer is big enough
    // If not, the caller is probably trying to determine the required
    // buffer size, so return it in ByteCount.

    if (pVdInfo->ByteCount < ByteCount)
    {
        pVdInfo->ByteCount = ByteCount;
        return(CLIENT_ERROR_BUFFER_TOO_SMALL);
    }

    // Initialize default data

    pVdInfo->ByteCount = ByteCount;
    pVdData = (PVDPING_C2H)pVdInfo->pBuffer;

    // Initialize module header

    pHeader = &pVdData->Header.Header;
    pHeader->ByteCount = ByteCount;
    pHeader->ModuleClass = Module_VirtualDriver;

    pHeader->VersionL = BLUEPRISM_VER_LO;
    pHeader->VersionH = BLUEPRISM_VER_HI;

    //strcpy((char*)(pHeader->HostModuleName), "ICA"); // max 8 characters (unsafe)
    strcpy_s((char*)(pHeader->HostModuleName), sizeof(pHeader->HostModuleName), "ICA"); // max 8 characters

    // Initialize virtual driver header

    pFlow = &pVdData->Header.Flow;
    pFlow->BandwidthQuota = 0;
    pFlow->Flow = VirtualFlow_None;

    // add our own data

    pVdData->usMaxDataSize = g_usMaxDataSize;
    pVdData->usPingCount = 3;

    pVdInfo->ByteCount = WIRE_WRITE(VDPING_C2H, pVdData, ByteCount);

    return(CLIENT_STATUS_SUCCESS);
}

/*******************************************************************************
 *
 *  DriverQueryInformation
 *
 *   Required vd function.
 *
 * ENTRY:
 *    pVd (input)
 *       pointer to virtual driver data structure
 *    pVdQueryInformation (input/output)
 *       pointer to the structure VDQUERYINFORMATION
 *    puiSize (output)
 *       size of VDQUERYINFORMATION structure
 *
 * EXIT:
 *    CLIENT_STATUS_SUCCESS - no error
 *
 ******************************************************************************/

int DriverQueryInformation(PVD pVd, PVDQUERYINFORMATION pVdQueryInformation, PUINT16 puiSize)
{   
    *puiSize = sizeof(VDQUERYINFORMATION);
    return(CLIENT_STATUS_SUCCESS);
}

/*******************************************************************************
 *
 *  DriverSetInformation
 *
 *   Required vd function.
 *
 * ENTRY:
 *    pVd (input)
 *       pointer to virtual driver data structure
 *    pVdSetInformation (input/output)
 *       pointer to the structure VDSETINFORMATION
 *    puiSize (input)
 *       size of VDSETINFORMATION structure
 *
 * EXIT:
 *    CLIENT_STATUS_SUCCESS - no error
 *
 ******************************************************************************/

int DriverSetInformation(PVD pVd, PVDSETINFORMATION pVdSetInformation, PUINT16 puiSize)
{    
    return(CLIENT_STATUS_SUCCESS);
}

/*******************************************************************************
 *
 *  DriverGetLastError
 *
 *   Queries error data.
 *   Required vd function.
 *
 * ENTRY:
 *    pVd (input)
 *       pointer to virtual driver data structure
 *    pLastError (output)
 *       pointer to the error structure to return (message is currently always
 *       NULL)
 *
 * EXIT:
 *    CLIENT_STATUS_SUCCESS - no error
 *
 ******************************************************************************/

int DriverGetLastError(PVD pVd, PVDLASTERROR pLastError)
{    
    pLastError->Error = pVd->LastError;
    pLastError->Message[0] = '\0';
    return(CLIENT_STATUS_SUCCESS);
}

/*******************************************************************************
 *
 *  int _SendAvailableData(void)
 *
 *  Send any available data to the server on the channel.
 *
 * ENTRY: Nothing
 *
 * EXIT:
 *    CLIENT_STATUS_SUCCESS - OK.
 *    CLIENT_STATUS_NO_DATA - No more data to send.
 *    CLIENT_STATUS_ERROR_RETRY - Could not send the data to the WD's output queue - no space.
 *                                Hopefully, space will be available next time.
 *    Otherwise, a fatal error code is returned from lower level functions.
 *
 * REMARKS:
 *    This function should be called only with the HPC client.
 *
 ******************************************************************************/

static int _SendAvailableData(void)
{
    int rc = CLIENT_STATUS_NO_DATA;

    // Check for something to write

    if (g_fBufferEmpty)
    {
        rc = CLIENT_STATUS_NO_DATA;
        return(rc);
    }

    // HPC does not support scatter write.  If there are multiple buffers to write
    // to a single packet, you must consolidate the buffers into a single buffer to 
    // send the data to the engine.


    // Send an ICA packet.  Parameters:
    //    g_pWd - A pointer via the WDxSETINFORMATION engine call in DriverOpen().
    //    g_MemorySections[0].pSection - The address of the data to send.
    //    g_MemorySections[0].length - The size of the data to send.
    //    &g_ulUserData - An arbitrary pointer to user data.  This pointer will be returned with a
    //                  notification poll.  In this case, the pointer just points to arbitrary data
    //                  (0xCAACCAAC).
    //    SENDDATA_NOTIFY - Flags.  See vdapi.h:SENDDATA*.
    rc = g_pSendData((DWORD)g_pWd, g_usVirtualChannelNum, g_MemorySections[0].pSection, g_MemorySections[0].length, &g_ulUserData, SENDDATA_NOTIFY);

    // Normal status returns are CLIENT_STATUS_SUCCESS (it worked) or CLIENT_ERROR_NO_OUTBUF (no room in output queue)

    if (CLIENT_STATUS_SUCCESS == rc)
    {
        g_fBufferEmpty = TRUE;
    }
    else if (CLIENT_ERROR_NO_OUTBUF == rc)
    {
        rc = CLIENT_STATUS_ERROR_RETRY;                        // Try again later
    }
    else if (CLIENT_STATUS_NO_DATA == rc)
    {
        // There was nothing to do.  Just fall through.
    }
    else
    {
        // We may be waiting on a callback.  This would be indicated by a 
        // CLIENT_ERROR_BUFFER_STILL_BUSY return when we tried
        // to send data.  Just return CLIENT_STATUS_ERROR_RETRY in this case.

        if (CLIENT_ERROR_BUFFER_STILL_BUSY == rc)
        {
            rc = CLIENT_STATUS_ERROR_RETRY;                        // Try again later
        }
    }
    return(rc);
}


