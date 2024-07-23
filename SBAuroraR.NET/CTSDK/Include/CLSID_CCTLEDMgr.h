/*
################################################################################
##
## File:     CLSID_CCTLEDMgr.h
##
## Purpose:  This declares the GUIDs for components.
##
## Remarks:  
##
## ---------------------------------------------------------------------------
## Copyright (c) Creative Technology Ltd., 2016.  All rights reserved.
##
## ---------------------------------------------------------------------------
## THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
## ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
## THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
## PARTICULAR PURPOSE.
##
################################################################################
*/

#ifndef __CLSID_CCTLEDMGR_H__
#define __CLSID_CCTLEDMGR_H__


#include <objbase.h>

//
// Declaration of GUIDs for components.
//
// {3370DE2D-576E-421D-9D04-E01B84D0E3EA}
DEFINE_GUID(CLSID_CCTLEDMgr,
	0x3370de2d, 0x576e, 0x421d, 0x9d, 0x4, 0xe0, 0x1b, 0x84, 0xd0, 0xe3, 0xea);
#ifdef COMPILEFLAG_Aggregation // For testing of aggregation only.
// {55D36C32-AB41-40de-9E0F-C40D37EF4744}
DEFINE_GUID(CLSID_CInternalMediaOne,
	0x55d36c32, 0xab41, 0x40de, 0x9e, 0xf, 0xc4, 0xd, 0x37, 0xef, 0x47, 0x44);
// {77D97E4F-962A-458e-845E-89EB377D9222}
DEFINE_GUID(CLSID_CExternalMediaOne,
	0x77d97e4f, 0x962a, 0x458e, 0x84, 0x5e, 0x89, 0xeb, 0x37, 0x7d, 0x92, 0x22);
#endif


#endif // #ifndef __CLSID_CCTLEDMGR_H__