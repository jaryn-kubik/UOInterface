#include "stdafx.h"
#include "MetaHost.h"

DWORD LoadCLR(LPCWSTR assemblyPath, LPCWSTR typeName, LPCWSTR methodName, LPCWSTR argument)
{
	HRESULT hr;
	ICLRMetaHost *pMetaHost = 0;
	hr = CLRCreateInstance(CLSID_CLRMetaHost, IID_PPV_ARGS(&pMetaHost));
	if (FAILED(hr))
		throw L"CLRLoader: CLRCreateInstance";

	WCHAR pwzVersion[MAX_PATH];
	DWORD size = sizeof(pwzVersion);
	hr = pMetaHost->GetVersionFromFile(assemblyPath, pwzVersion, &size);
	if (FAILED(hr))
		throw L"GetVersionFromFile";

	ICLRRuntimeInfo *pRuntimeInfo = 0;
	hr = pMetaHost->GetRuntime(pwzVersion, IID_PPV_ARGS(&pRuntimeInfo));
	if (FAILED(hr))
		throw L"GetRuntime";

	ICLRRuntimeHost *pClrRuntimeHost = 0;
	hr = pRuntimeInfo->GetInterface(CLSID_CLRRuntimeHost, IID_PPV_ARGS(&pClrRuntimeHost));
	if (FAILED(hr))
		throw L"GetInterface";

	hr = pClrRuntimeHost->Start();
	if (FAILED(hr))
		throw L"Start";

	DWORD dwLengthRet;
	hr = pClrRuntimeHost->ExecuteInDefaultAppDomain(assemblyPath, typeName, methodName, argument, &dwLengthRet);
	if (FAILED(hr))
		throw L"ExecuteInDefaultAppDomain";

	pMetaHost->Release();
	pRuntimeInfo->Release();
	pClrRuntimeHost->Release();

	return dwLengthRet;
}