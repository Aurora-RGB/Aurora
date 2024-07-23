/*
################################################################################
##
## File:     CTIntrfc_DynamicLoad.h
##
## Purpose:  This declares the functions of the Creative Interface Library. 
##
## Remarks:  This header file is required in order to use the Creative 
##           Interface Library.
##
## ---------------------------------------------------------------------------
## Copyright (c) Creative Technology Ltd., 2013.  All rights reserved.
##
## ---------------------------------------------------------------------------
## THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
## ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
## THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
## PARTICULAR PURPOSE.
##
################################################################################
*/

#ifndef __CTINTRFC_DYNAMICLOAD_H__
#define __CTINTRFC_DYNAMICLOAD_H__


//============================================================================
// Return error codes for error type CTINTRFCRESULT.
//
typedef enum tagCTINTRFCRESULT
{
	CTINTRFCRESULT_Success = 0,
	CTINTRFCRESULT_CannotLoadLibrary = 1,
	CTINTRFCRESULT_UnrecognizedLibrary = 2,
	CTINTRFCRESULT_CannotCreateComponent = 3,
	CTINTRFCRESULT_CannotCreateInterface = 4,
	CTINTRFCRESULT_ComponentNotRegistered = 5,
	CTINTRFCRESULT_InvalidCLSID = 6,
	CTINTRFCRESULT_InvalidCLSIDString = 7,
	CTINTRFCRESULT_InvalidObjectID = 8,
	CTINTRFCRESULT_InvalidObjectIDString = 9,
	CTINTRFCRESULT_BufferTooSmall = 10,
	CTINTRFCRESULT_OperationFailed = 11,
	CTINTRFCRESULT_BadParam = 12,
	CTINTRFCRESULT_CannotPerformRegistryOperations = 13,
	CTINTRFCRESULT_AccessDenied = 14,
	CTINTRFCRESULT_CannotPerformIniFileOperations = 15,
	CTINTRFCRESULT_NotLicensed = 16,
	CTINTRFCRESULT_UnknownError = 0xFFFFFFFF
} CTINTRFCRESULT;


CTINTRFCRESULT __inline CTCreateInstanceEx_Dyn(IN HMODULE hModuleCTIntrfu, IN REFCLSID rclsid, IN LPUNKNOWN lpUnknownOuter, IN DWORD dwClsContext, IN REFIID riid, IN LPCTSTR lpcszCategoryExt, IN LPCTSTR lpcszCategoryFileDir, IN LPCTSTR lpcszLibFile, IN LPCTSTR lpcszSideBySideDirPath, OUT PVOID* ppv)
{
	*ppv = NULL;

	CTINTRFCRESULT resultCTIntrfc;
	// "CTCreateInstanceEx"
	typedef CTINTRFCRESULT (__cdecl * PFNCTCREATEINSTANCEEX)(IN REFCLSID rclsid, IN LPUNKNOWN lpUnknownOuter, IN DWORD dwClsContext, IN REFIID riid, IN LPCTSTR lpcszCategoryExt, IN LPCTSTR lpcszCategoryFileDir, IN LPCTSTR lpcszLibFile, IN LPCTSTR lpcszSideBySideDirPath, OUT PVOID* ppv);
	PFNCTCREATEINSTANCEEX pfnCTCreateInstanceEx = (PFNCTCREATEINSTANCEEX)GetProcAddress(hModuleCTIntrfu, "CTCreateInstanceEx");
	if (pfnCTCreateInstanceEx != NULL)
	{
		resultCTIntrfc = pfnCTCreateInstanceEx(rclsid, lpUnknownOuter, dwClsContext, riid, lpcszCategoryExt, lpcszCategoryFileDir, lpcszLibFile, lpcszSideBySideDirPath, (void **)ppv);
		if (resultCTIntrfc == CTINTRFCRESULT_Success)
		{
		}
		else
		{
			*ppv = NULL;
		}
	}
	else
	{
		DWORD dwLastErr = GetLastError();
		//int iRes = MessageBox(_T("FAILED: GetProcAddress(\"CTCreateInstanceEx\")."), _T("ERROR"), MB_ICONERROR | MB_OK);
		resultCTIntrfc = CTINTRFCRESULT_UnrecognizedLibrary;
	}

	return resultCTIntrfc;
}

void __inline CTFreeUnusedLibrariesEx_Dyn(IN HMODULE hModuleCTIntrfu)
{
	// "CTFreeUnusedLibrariesEx".
	typedef void (__cdecl * PFNCTFREEUNUSEDLIBRARIESEX)();
	PFNCTFREEUNUSEDLIBRARIESEX pfnCTFreeUnusedLibrariesEx = (PFNCTFREEUNUSEDLIBRARIESEX)GetProcAddress(hModuleCTIntrfu, "CTFreeUnusedLibrariesEx");
	if (pfnCTFreeUnusedLibrariesEx != NULL)
	{
		pfnCTFreeUnusedLibrariesEx();
	}
	else
	{
		DWORD dwLastErr = GetLastError();
		//int iRes = MessageBox(_T("FAILED: GetProcAddress(\"CTFreeUnusedLibrariesEx\")."), _T("ERROR"), MB_ICONERROR | MB_OK);
		// There is no error code to return since this function returns void.
	}

	return;
}


#endif // #ifndef __CTINTRFC_DYNAMICLOAD_H__