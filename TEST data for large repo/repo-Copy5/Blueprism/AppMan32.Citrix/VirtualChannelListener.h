#pragma once

#include <WinSock2.h>
#include <WS2tcpip.h>
#include <windows.h>
#include "citrix.h"
#include <ica.h>
#include <ica-c2h.h>
#include "clterr.h"
#include "wdapi.h"
#include "vdapi.h"
#include "vd.h"
#include "cmacro.h"

#define BLUEPRISM_VIRTUAL_CHANNEL_NAME "BPAPMAN"
#define VC_TIMEOUT_MILLISECONDS     10000L
#define VC_PING_SIGNATURE           0x4950

/*
 * Lowest and highest compatible version.  See DriverInfo().
 */
#define BLUEPRISM_VER_LO      1
#define BLUEPRISM_VER_HI      1

#pragma pack(1)

/* ping packet structure */

typedef struct _PING {
    USHORT  uSign;              /* signature */
    USHORT  uType;              /* type, BEGIN or END, from server */
    USHORT  uLen;               /* packet length from server */
    USHORT  uCounter;           /* sequencer */
    ULONG   ulServerMS;         /* server millisecond clock */
    ULONG   ulClientMS;         /* client millisecond clock */
} PING, * PPING;

/* vdping driver info (client to host) structure */
typedef struct _VDPING_C2H
{
    VD_C2H  Header;
    USHORT  usMaxDataSize;      /* maximum data block size */
    USHORT  usPingCount;        /* number of times to ping */
} VDPING_C2H, * PVDPING_C2H;

#pragma pack()

static int _SendAvailableData(void);
