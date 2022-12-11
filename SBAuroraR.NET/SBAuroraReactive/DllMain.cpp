#pragma unmanaged
#define INITGUID 1

#include "DllMain.h"
#include "Types.h"

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include "CLSID_CCTLEDMgr.h"

static const wchar_t* GetSetModuleFolderPath(const wchar_t* newModuleFolderPath = nullptr, const size_t newModuleFolderLen = 0)
{
	static wchar_t dllFolderPath[2048] = {0};
	if (newModuleFolderPath != nullptr && newModuleFolderLen < array_countof(dllFolderPath))
	{
		memcpy(dllFolderPath, newModuleFolderPath, newModuleFolderLen*sizeof(wchar_t));
		dllFolderPath[newModuleFolderLen] = 0;
	}
	return dllFolderPath;
}

static HMODULE _ctInstHmodule = NULL;
BOOL APIENTRY DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved)
{
	switch (fdwReason)
	{
	case DLL_PROCESS_ATTACH:
	{
		wchar_t dllFolderPath[2048];
		size_t dllFolderPathLen = 0;
		if (hinstDLL != NULL)
		{
			const int modulePathLen = GetModuleFileNameW(hinstDLL, dllFolderPath, (DWORD)array_countof(dllFolderPath));
			if (modulePathLen > 0 && size_t(modulePathLen) < array_countof(dllFolderPath))
			{
				for (int i=modulePathLen-1; i>=0; i--)
				{
					if (dllFolderPath[i] == '\\' || dllFolderPath[i] == '/')
					{
						dllFolderPath[i] = 0;
						dllFolderPathLen = i;
						break;
					}
				}
			}
		}
		GetSetModuleFolderPath(dllFolderPath, dllFolderPathLen);
		DisableThreadLibraryCalls(hinstDLL);

		size_t ctDllPathLen = dllFolderPathLen;
		{
			dllFolderPath[ctDllPathLen++] = '\\';
			const size_t ctDllNameLen = wcslen(DEFINITION_FILENAME_CreativeInterfaceLibrary);
			if (ctDllPathLen+ctDllNameLen < array_countof(dllFolderPath))
			{
				memcpy(&dllFolderPath[ctDllPathLen], DEFINITION_FILENAME_CreativeInterfaceLibrary, ctDllNameLen*sizeof(wchar_t));
				ctDllPathLen += ctDllNameLen;
				dllFolderPath[ctDllPathLen] = 0;
			}
		}
		_ctInstHmodule = LoadLibraryW(dllFolderPath);
		if (_ctInstHmodule == NULL)
		{
			MessageBoxW(NULL, dllFolderPath, DEFINITION_FILENAME_CreativeInterfaceLibrary, 0);
			return FALSE;
		}
		break;
	}
	case DLL_THREAD_ATTACH:
		break;
	case DLL_THREAD_DETACH:
		break;
	case DLL_PROCESS_DETACH:
		if (_ctInstHmodule != NULL)
		{
			CTFreeUnusedLibrariesEx_Dyn(_ctInstHmodule);
			FreeLibrary(_ctInstHmodule);
			_ctInstHmodule = NULL;
		}
		break;
	}
	return TRUE;
}

CTINTRFCRESULT CreateLEDManager(ICTLEDMgr** outLedMgrPtr)
{
	wchar_t ctDllPath[2048] = {0};
	wcscpy_s(ctDllPath, GetSetModuleFolderPath());
	size_t ctDllPathLen = wcslen(ctDllPath);
	{
		ctDllPath[ctDllPathLen++] = '\\';
		const size_t ctDllNameLen = wcslen(DEFINITION_FILENAME_CreativeLedManagerLibrary);
		if (ctDllPathLen+ctDllNameLen < array_countof(ctDllPath))
		{
			memcpy(&ctDllPath[ctDllPathLen], DEFINITION_FILENAME_CreativeLedManagerLibrary, ctDllNameLen*sizeof(wchar_t));
			ctDllPathLen += ctDllNameLen;
			ctDllPath[ctDllPathLen] = 0;
		}
	}

	ICTLEDMgr* pICTLEDMgr = nullptr;
	CTINTRFCRESULT cIntRes = CTCreateInstanceEx_Dyn(_ctInstHmodule, CLSID_CCTLEDMgr, NULL, 0, IID_ICTLEDMgr, NULL, NULL, ctDllPath, NULL, (void **)&pICTLEDMgr);
	if (cIntRes == CTINTRFCRESULT_Success)
	{
		DWORD dwFlag = 0;
		HRESULT hr = pICTLEDMgr->Initialize(DEFINITION_CTLEDMgr_Interface_Version, dwFlag);
		if (SUCCEEDED(hr))
			*outLedMgrPtr = pICTLEDMgr;
		else
		{
			ULONG ul = pICTLEDMgr->Release();
			pICTLEDMgr = nullptr;
			cIntRes = CTINTRFCRESULT_OperationFailed;
		}
	}
	
	return cIntRes;
}