#include "stdafx.h"
#include "UOInterface.h"
#include "ImportHooks.h"
#include "PacketHooks.h"
#include "IPC.h"
#include "Macros.h"

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
	if (ul_reason_for_call == DLL_PROCESS_ATTACH)
		DisableThreadLibraryCalls(hModule);
	return TRUE;
}

DWORD WINAPI Init(LPVOID hwnd)
{
	try
	{
		HANDLE handle = InitIPC(*(HWND*)hwnd);
		HookImports();
		HookPackets();
		InitMacros();
		return (DWORD)handle;
	}
	catch (LPCWSTR str) { MessageBox(nullptr, str, L"Error: Init", MB_ICONERROR | MB_OK); }
	return EXIT_FAILURE;
}

UOINTERFACE_API(void) Start(LPWSTR client, HWND hwnd)
{
	try
	{
		//create suspended process
		PROCESS_INFORMATION pi = {};
		STARTUPINFOW si = {};
		si.cb = sizeof(si);
		if (!CreateProcess(client, nullptr, nullptr, nullptr, false, CREATE_SUSPENDED, nullptr, nullptr, &si, &pi))
			throw L"CreateProcess";
		Inject(pi.dwProcessId, hwnd);
		ResumeThread(pi.hThread);
	}
	catch (LPCWSTR str) { MessageBox(nullptr, str, L"Error: Start", MB_ICONERROR | MB_OK); }
}

UOINTERFACE_API(void) Inject(DWORD pid, HWND hwnd)
{
	try
	{
		HANDLE hProcess = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_VM_OPERATION | PROCESS_VM_WRITE, FALSE, pid);
		if (!hProcess)
			throw L"OpenProcess";

		//get UOInterface module and path, write it in remote process memory
		HMODULE hDll;
		GetModuleHandleExW(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT, (LPCWSTR)&Start, &hDll);
		WCHAR dllPath[1024];
		GetModuleFileNameW(hDll, dllPath, sizeof(dllPath));
		LPVOID pRemote = VirtualAllocEx(hProcess, nullptr, sizeof(dllPath), MEM_COMMIT, PAGE_READWRITE);
		if (!pRemote)
			throw L"VirtualAllocEx";
		if (!WriteProcessMemory(hProcess, pRemote, dllPath, sizeof(dllPath), nullptr))
			throw L"WriteProcessMemory (UOInterface.dll path)";

		//load UOInterface in remote process, get its module
		LPTHREAD_START_ROUTINE pLoadLibrary = (LPTHREAD_START_ROUTINE)GetProcAddress(GetModuleHandle(L"kernel32"), "LoadLibraryW");
		HANDLE hThread = CreateRemoteThread(hProcess, nullptr, 0, pLoadLibrary, pRemote, 0, nullptr);
		if (!hThread)
			throw L"CreateRemoteThread (LoadLibrary)";
		WaitForSingleObject(hThread, INFINITE);
		DWORD hRemote;//this module adrress in created process
		GetExitCodeThread(hThread, &hRemote);
		if (!hRemote)
			throw L"Remote LoadLibrary";
		CloseHandle(hThread);

		//call Init
		if (!WriteProcessMemory(hProcess, pRemote, &hwnd, 4, nullptr))
			throw L"WriteProcessMemory (hwnd)";
		hThread = CreateRemoteThread(hProcess, nullptr, 0, (LPTHREAD_START_ROUTINE)(hRemote + (DWORD)Init - (DWORD)hDll), pRemote, 0, nullptr);
		if (!hThread)
			throw L"CreateRemoteThread (Init)";
		WaitForSingleObject(hThread, INFINITE);
		GetExitCodeThread(hThread, &hRemote);
		CloseHandle(hThread);

		//free memory
		VirtualFreeEx(hProcess, pRemote, sizeof(dllPath), MEM_RELEASE);
		CloseHandle(hProcess);

		sharedMemory = (SharedMemory*)MapViewOfFile((HANDLE)hRemote, FILE_MAP_ALL_ACCESS, 0, 0, 0);
		if (!sharedMemory)
			throw L"MapViewOfFile";
	}
	catch (LPCWSTR str) { MessageBox(nullptr, str, L"Error: Attach", MB_ICONERROR | MB_OK); }
}
//---------------------------------------------------------------------------//
//---------------------------------------------------------------------------//
//---------------------------------------------------------------------------//
UOINTERFACE_API(byte*) GetInBuffer() { return sharedMemory->bufferOut; }
UOINTERFACE_API(byte*) GetOutBuffer() { return sharedMemory->bufferIn; }
UOINTERFACE_API(short*) GetPacketTable() { return sharedMemory->packetTable; }
UOINTERFACE_API(void) SendIPCMessage(UOMessage msg, UINT wParam, UINT lParam)
{
	SendMessage(sharedMemory->hwnd, (UINT)msg, wParam, lParam);
}