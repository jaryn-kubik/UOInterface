#include "stdafx.h"
#include "UOInterface.h"
#include "CLRLoader.h"
#include "ImportHooks.h"
#include "PacketHooks.h"
#include "Patches.h"

struct StartInfo
{
	WCHAR assemblyPath[MAX_PATH];
	WCHAR typeName[MAX_PATH];
	WCHAR methodName[MAX_PATH];
	BOOL patchEncryption;
};

DWORD WINAPI Init(StartInfo *info)
{
	try
	{
		DWORD oldProtect;
		VirtualProtect((LPVOID)0x00400000, 0x00200000, PAGE_EXECUTE_READWRITE, &oldProtect);

		HookImports();
		HookPackets();
		PatchMulti();
		if (info->patchEncryption)
			PatchEncryption();

		return LoadCLR(info->assemblyPath, info->typeName, info->methodName, L"");
	}
	catch (LPCWSTR str) { MessageBox(0, str, L"Error", MB_ICONERROR | MB_OK); }
	catch (...) { MessageBox(0, L"Unknown error", L"Fatal error", MB_ICONERROR | MB_OK); }
	return EXIT_FAILURE;
}

CallBacks callBacks;
UOINTERFACE_API(void) SetCallbacks(CallBacks callbacks) { callBacks = callbacks; }
UOINTERFACE_API(void) Start(LPWSTR clientPath, LPWSTR assemblyPath, LPWSTR typeName,
	LPWSTR methodName, BOOL patchEncryption)
{
	//create the structure which we will pass to the new process
	StartInfo info = {};
	if (assemblyPath == NULL)
		GetModuleFileName(GetModuleHandle(0), info.assemblyPath, sizeof(info.assemblyPath));
	else
		wcscpy_s(info.assemblyPath, assemblyPath);
	wcscpy_s(info.typeName, typeName);
	wcscpy_s(info.methodName, methodName);
	info.patchEncryption = patchEncryption;

	//create suspended process
	PROCESS_INFORMATION pi = {};
	STARTUPINFOW si = {};
	si.cb = sizeof(si);
	CreateProcess(clientPath, 0, 0, 0, false, CREATE_SUSPENDED, 0, NULL, &si, &pi);

	//get our dll module and path, write it in remote process memory
	HMODULE hDll;
	GetModuleHandleEx(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT, (LPCWSTR)&Start, &hDll);
	WCHAR dllPath[MAX_PATH];
	GetModuleFileName(hDll, dllPath, sizeof(dllPath));
	LPVOID pRemote = VirtualAllocEx(pi.hProcess, 0, sizeof(StartInfo), MEM_COMMIT, PAGE_READWRITE);
	WriteProcessMemory(pi.hProcess, pRemote, dllPath, sizeof(dllPath), 0);

	//load our dll in remote process, get its module
	LPTHREAD_START_ROUTINE pLoadLibrary = (LPTHREAD_START_ROUTINE)GetProcAddress(GetModuleHandle(L"kernel32"), "LoadLibraryW");
	HANDLE hThread = CreateRemoteThread(pi.hProcess, 0, 0, pLoadLibrary, pRemote, 0, 0);
	WaitForSingleObject(hThread, INFINITE);
	DWORD hRemote;//this module adrress in created process
	GetExitCodeThread(hThread, &hRemote);
	CloseHandle(hThread);

	//write info in remove process and call Init
	WriteProcessMemory(pi.hProcess, pRemote, &info, sizeof(StartInfo), 0);
	hThread = CreateRemoteThread(pi.hProcess, 0, 0, (LPTHREAD_START_ROUTINE)(hRemote + (DWORD)Init - (DWORD)hDll), pRemote, 0, 0);
	WaitForSingleObject(hThread, INFINITE);
	GetExitCodeThread(hThread, &hRemote);
	CloseHandle(hThread);

	//free memory, resume process
	VirtualFreeEx(pi.hProcess, pRemote, sizeof(StartInfo), MEM_RELEASE);
	ResumeThread(pi.hThread);
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
	if (ul_reason_for_call == DLL_PROCESS_ATTACH)
		DisableThreadLibraryCalls(hModule);
	return TRUE;
}