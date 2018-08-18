#include "stdafx.h"
#include <metahost.h>
#pragma comment(lib, "mscoree.lib")

#include "Bootstrapper.h"

DllExport void LoadManagedProject(const wchar_t * managedDllLocation)
{
	
    ICLRRuntimeHost* pClr = StartCLR(L"v4.0.30319");

	DWORD result;
	pClr->ExecuteInDefaultAppDomain(
		managedDllLocation,
		L"Hijack.Sample.Program",
		L"EntryPoint",
		L"Argument",
		&result);
   
}

ICLRRuntimeHost* StartCLR(LPCWSTR dotNetVersion)
{

    ICLRMetaHost* pClrMetaHost = NULL;
    ICLRRuntimeInfo* pClrRuntimeInfo = NULL;
    ICLRRuntimeHost* pClrRuntimeHost = NULL;
	BOOL fLoadable;

	CLRCreateInstance(CLSID_CLRMetaHost, IID_ICLRMetaHost, (LPVOID*)&pClrMetaHost);
	pClrMetaHost->GetRuntime(dotNetVersion, IID_PPV_ARGS(&pClrRuntimeInfo));
	pClrRuntimeInfo->IsLoadable(&fLoadable);
	pClrRuntimeInfo->GetInterface(CLSID_CLRRuntimeHost, IID_PPV_ARGS(&pClrRuntimeHost));
	pClrRuntimeHost->Start();

	return pClrRuntimeHost;
}
