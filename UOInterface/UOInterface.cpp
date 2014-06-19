#include "stdafx.h"
#include "UOInterface.h"
#include "CLRLoader.h"
#include "ImportHooks.h"
#include "PacketHooks.h"
#include "Patches.h"

CallBacks callBacks;
UOINTERFACE_API(void) InstallHooks(CallBacks callbacks, BOOL patchEncryption)
{
	try
	{
		callBacks = callbacks;
		HookImports();
		HookPackets();
		if (patchEncryption)
			PatchEncryption();
		PatchMulti();
	}
	catch (LPCWSTR str) { MessageBoxW(nullptr, str, L"Error", MB_ICONERROR | MB_OK); }
}

DWORD WINAPI Init(LPCWSTR info)
{
	try
	{
		LPCWSTR assembly = info;
		LPCWSTR type = assembly + wcslen(assembly) + 1;
		LPCWSTR method = type + wcslen(type) + 1;
		LPCWSTR args = method + wcslen(method) + 1;
		return LoadCLR(assembly, type, method, args);
	}
	catch (LPCWSTR str) { MessageBox(nullptr, str, L"Error", MB_ICONERROR | MB_OK); }
	return EXIT_FAILURE;
}

UOINTERFACE_API(void) Start(LPWSTR client, LPWSTR assembly, LPWSTR type, LPWSTR method, LPWSTR args)
{
	WCHAR empty[1] = {};
	WCHAR callerAssembly[MAX_PATH];
	if (assembly == nullptr)
	{
		GetModuleFileName(GetModuleHandle(nullptr), callerAssembly, sizeof(callerAssembly));
		assembly = callerAssembly;
	}
	if (args == nullptr)
		args = empty;

	//create suspended process
	PROCESS_INFORMATION pi = {};
	STARTUPINFOW si = {};
	si.cb = sizeof(si);
	CreateProcess(client, nullptr, nullptr, nullptr, false, CREATE_SUSPENDED, nullptr, nullptr, &si, &pi);

	//get UOInterface module and path, write it in remote process memory
	HMODULE hDll;
	GetModuleHandleEx(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT, (LPCWSTR)&Start, &hDll);
	WCHAR dllPath[MAX_PATH];
	GetModuleFileName(hDll, dllPath, sizeof(dllPath));
	SIZE_T pRemoveLen = 2 * max((wcslen(assembly) + wcslen(type) + wcslen(method) + wcslen(args) + 4), wcslen(dllPath));
	LPVOID pRemote = (LPWSTR)VirtualAllocEx(pi.hProcess, nullptr, pRemoveLen, MEM_COMMIT, PAGE_READWRITE);
	WriteProcessMemory(pi.hProcess, pRemote, dllPath, sizeof(dllPath), nullptr);

	//load UOInterface in remote process, get its module
	LPTHREAD_START_ROUTINE pLoadLibrary = (LPTHREAD_START_ROUTINE)GetProcAddress(GetModuleHandle(L"kernel32"), "LoadLibraryW");
	HANDLE hThread = CreateRemoteThread(pi.hProcess, nullptr, 0, pLoadLibrary, pRemote, 0, nullptr);
	WaitForSingleObject(hThread, INFINITE);
	DWORD hRemote;//this module adrress in created process
	GetExitCodeThread(hThread, &hRemote);
	CloseHandle(hThread);

	//write info in remove process
	byte *pArgs = (byte*)pRemote;
	SIZE_T written;
	WriteProcessMemory(pi.hProcess, pArgs, assembly, (wcslen(assembly) + 1) * 2, &written);
	WriteProcessMemory(pi.hProcess, pArgs += written, type, (wcslen(type) + 1) * 2, &written);
	WriteProcessMemory(pi.hProcess, pArgs += written, method, (wcslen(method) + 1) * 2, &written);
	WriteProcessMemory(pi.hProcess, pArgs += written, args, (wcslen(args) + 1) * 2, &written);

	//call Init
	hThread = CreateRemoteThread(pi.hProcess, nullptr, 0, (LPTHREAD_START_ROUTINE)(hRemote + (DWORD)Init - (DWORD)hDll), pRemote, 0, nullptr);
	WaitForSingleObject(hThread, INFINITE);
	GetExitCodeThread(hThread, &hRemote);
	CloseHandle(hThread);

	//free memory, resume process
	VirtualFreeEx(pi.hProcess, pRemote, pRemoveLen, MEM_RELEASE);
	ResumeThread(pi.hThread);
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
	if (ul_reason_for_call == DLL_PROCESS_ATTACH)
		DisableThreadLibraryCalls(hModule);
	return TRUE;
}