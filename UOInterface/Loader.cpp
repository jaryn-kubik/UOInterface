#include "stdafx.h"
#include "IPC.h"
#include "Loader.h"
#include <thread>
#include <future>

namespace Loader
{
	OnUOMessage callback;
	LRESULT CALLBACK WindowProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
	{
		if (msg >= IPC::First && msg <= IPC::Last)
		{
			if (msg == IPC::Closing)
				PostQuitMessage(0);
			return callback(msg, wParam, lParam);
		}
		return DefWindowProc(hWnd, msg, wParam, lParam);
	}

	void MessagePump(std::promise<HWND> *p)
	{
		try
		{
			WNDCLASS wnd = { 0 };
			wnd.lpfnWndProc = WindowProc;
			wnd.hInstance = GetModuleHandle(nullptr);
			wnd.lpszClassName = L"UOInterface";

			if (!RegisterClass(&wnd))
				throw L"RegisterClass";

			HWND hwnd = CreateWindow(wnd.lpszClassName, nullptr, 0, 0, 0, 0, 0, HWND_MESSAGE, nullptr, wnd.hInstance, nullptr);
			if (!hwnd)
				throw L"CreateWindow";
			p->set_value(hwnd);

			MSG msg;
			while (GetMessage(&msg, hwnd, 0, 0) > 0)
			{
				TranslateMessage(&msg);
				DispatchMessage(&msg);
			}
		}
		catch (LPCWSTR str) { MessageBox(nullptr, str, L"Window thread", MB_ICONERROR | MB_OK); }
	}

	UOINTERFACE_API(DWORD) Start(LPWSTR client, OnUOMessage onMessage)
	{
		try
		{
			//create suspended process
			PROCESS_INFORMATION pi = {};
			STARTUPINFOW si = {};
			si.cb = sizeof(si);
			if (!CreateProcess(client, nullptr, nullptr, nullptr, false, CREATE_SUSPENDED, nullptr, nullptr, &si, &pi))
				throw L"CreateProcess";
			Inject(pi.dwProcessId, onMessage);
			ResumeThread(pi.hThread);
			return pi.dwProcessId;
		}
		catch (LPCWSTR str) { MessageBox(nullptr, str, L"Error: Start", MB_ICONERROR | MB_OK); }
		return -1;
	}

	UOINTERFACE_API(void) Inject(DWORD pid, OnUOMessage onMessage)
	{
		try
		{
			callback = onMessage;
			std::promise<HWND> p;
			std::thread t(MessagePump, &p);
			HWND hwnd = p.get_future().get();
			t.detach();

			HANDLE hProcess = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_DUP_HANDLE, FALSE, pid);
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
			auto pLoadLibrary = (LPTHREAD_START_ROUTINE)GetProcAddress(GetModuleHandle(L"kernel32"), "LoadLibraryW");
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
			auto onAttach = (LPTHREAD_START_ROUTINE)(hRemote + (DWORD)IPC::OnAttach - (DWORD)hDll);
			hThread = CreateRemoteThread(hProcess, nullptr, 0, onAttach, IPC::Init(hwnd, hProcess), 0, nullptr);
			if (!hThread)
				throw L"CreateRemoteThread (Init)";
			WaitForSingleObject(hThread, INFINITE);
			GetExitCodeThread(hThread, &hRemote);
			CloseHandle(hThread);

			//free memory
			VirtualFreeEx(hProcess, pRemote, sizeof(dllPath), MEM_RELEASE);
			CloseHandle(hProcess);
		}
		catch (LPCWSTR str) { MessageBox(nullptr, str, L"Error: Attach", MB_ICONERROR | MB_OK); }
	}
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD reason, LPVOID lpReserved) { return TRUE; }