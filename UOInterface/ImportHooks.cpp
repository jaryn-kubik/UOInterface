#include "stdafx.h"
#include "WinSock2.h"
#include "PacketHooks.h"
#include "Utils.h"
#include "IPC.h"

bool HookImport(LPCSTR dll, LPCSTR function, DWORD ordinal, LPVOID hook)
{
	DWORD base = (DWORD)GetModuleHandle(nullptr);
	PIMAGE_DOS_HEADER idh = (PIMAGE_DOS_HEADER)base;
	PIMAGE_NT_HEADERS inh = (PIMAGE_NT_HEADERS)(base + idh->e_lfanew);
	PIMAGE_DATA_DIRECTORY idd = &inh->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT];
	PIMAGE_IMPORT_DESCRIPTOR iid = (PIMAGE_IMPORT_DESCRIPTOR)(base + idd->VirtualAddress);

	for (; iid->Name; iid++)
	{
		if (_stricmp(dll, (LPSTR)(base + iid->Name)) != 0)
			continue;

		PIMAGE_THUNK_DATA nTable = (PIMAGE_THUNK_DATA)(base + iid->OriginalFirstThunk);
		PIMAGE_THUNK_DATA aTable = (PIMAGE_THUNK_DATA)(base + iid->FirstThunk);

		for (; nTable->u1.AddressOfData; nTable++, aTable++)
		{
			if (!(nTable->u1.Ordinal & 0x80000000))
			{
				PIMAGE_IMPORT_BY_NAME name = (PIMAGE_IMPORT_BY_NAME)(base + nTable->u1.AddressOfData);
				if ((function && strcmp(name->Name, function) == 0) || ordinal == name->Hint)
				{
					AllowAccess(&aTable->u1.Function, sizeof(DWORD));
					aTable->u1.Function = (DWORD)hook;
					return true;
				}
			}
			else if (ordinal == (nTable->u1.Ordinal & 0xffff))
			{
				AllowAccess(&aTable->u1.Function, sizeof(DWORD));
				aTable->u1.Function = (DWORD)hook;
				return true;
			}
		}
	}
	return false;
}

int GetKeyModifiers()
{
	int mods = (int)KeyModifiers::None;

	if (GetKeyState(VK_MENU) & 0xFF00)
		mods |= (int)KeyModifiers::Alt;

	if (GetKeyState(VK_CONTROL) & 0xFF00)
		mods |= (int)KeyModifiers::Control;

	if (GetKeyState(VK_SHIFT) & 0xFF00)
		mods |= (int)KeyModifiers::Shift;

	return mods;
}

WPARAM keyToIgnore;
WNDPROC oldWndProc;
LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch (msg)
	{
	case WM_SETFOCUS:
		SendIPCMessage(UOMessage::Focus, TRUE);
		break;

	case WM_KILLFOCUS:
		SendIPCMessage(UOMessage::Focus, FALSE);
		break;

	case WM_SIZE:
		SendIPCMessage(UOMessage::Visibility, wParam != SIZE_MINIMIZED);
		break;

	case WM_MBUTTONDOWN:
		if (SendIPCMessage(UOMessage::KeyDown, VK_MBUTTON, GetKeyModifiers()))
			return 0;
		break;

	case WM_XBUTTONDOWN:
		if (SendIPCMessage(UOMessage::KeyDown, HIWORD(wParam) == XBUTTON1 ? VK_XBUTTON1 : VK_XBUTTON2, GetKeyModifiers()))
			return 0;
		break;

	case WM_MOUSEWHEEL:
		if (SendIPCMessage(UOMessage::KeyDown, GET_WHEEL_DELTA_WPARAM(wParam) < 0 ? VK_F23 : VK_F24, GetKeyModifiers()))
			return 0;
		break;

	case WM_SYSDEADCHAR:
	case WM_SYSKEYDOWN:
	case WM_KEYDOWN:
		if (wParam != VK_SHIFT && wParam != VK_CONTROL && wParam != VK_MENU &&
			(lParam & (1 << 30)) == 0 && SendIPCMessage(UOMessage::KeyDown, wParam, GetKeyModifiers()))
		{//bit 30 == previous key state
			keyToIgnore = lParam;
			return 0;
		}
		break;

	case WM_CHAR:
		if (keyToIgnore && keyToIgnore == lParam)
		{
			keyToIgnore = 0;
			return 0;
		}
		break;

	case WM_CREATE:
		sharedMemory->hwnd = hwnd;
		SendIPCMessage(UOMessage::Init);
		break;
	}

	if (msg >= (int)UOMessage::First && msg <= (int)UOMessage::Last)
		return RecvIPCMessage((UOMessage)msg, wParam, lParam);
	return oldWndProc(hwnd, msg, wParam, lParam);
}
//---------------------------------------------------------------------------//
//---------------------------------------------------------------------------//
//---------------------------------------------------------------------------//
CHAR clientDirA[MAX_PATH];
DWORD WINAPI Hook_GetCurrentDirectoryA(DWORD nBufferLength, LPSTR lpBuffer)
{
	size_t size = strnlen(clientDirA, MAX_PATH);
	if (nBufferLength == 0)
		return size + 1;
	strcpy_s(lpBuffer, size + 1, clientDirA);
	return size;
}

WCHAR clientDirW[MAX_PATH];
DWORD WINAPI Hook_GetCurrentDirectoryW(DWORD nBufferLength, LPWSTR lpBuffer)
{
	size_t size = wcsnlen(clientDirW, MAX_PATH);
	if (nBufferLength == 0)
		return size + 1;
	wcscpy_s(lpBuffer, size + 1, clientDirW);
	return size;
}

HANDLE WINAPI Hook_CreateFileA(LPCSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode,
	LPSECURITY_ATTRIBUTES lpSecAtt, DWORD dwCreationDisposition, DWORD dwFlags, HANDLE hTemplate)
{
	if (PathIsRelativeA(lpFileName))
	{
		CHAR path[MAX_PATH];
		PathCombineA(path, clientDirA, lpFileName);
		return CreateFileA(path, dwDesiredAccess, dwShareMode, lpSecAtt,
			dwCreationDisposition, dwFlags, hTemplate);
	}
	return CreateFileA(lpFileName, dwDesiredAccess, dwShareMode, lpSecAtt,
		dwCreationDisposition, dwFlags, hTemplate);
}

HANDLE WINAPI Hook_CreateFileW(LPCWSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode,
	LPSECURITY_ATTRIBUTES lpSecAtt, DWORD dwCreationDisposition, DWORD dwFlags, HANDLE hTemplate)
{
	if (PathIsRelativeW(lpFileName))
	{
		WCHAR path[MAX_PATH];
		PathCombineW(path, clientDirW, lpFileName);
		return CreateFileW(path, dwDesiredAccess, dwShareMode, lpSecAtt,
			dwCreationDisposition, dwFlags, hTemplate);
	}
	return CreateFileW(lpFileName, dwDesiredAccess, dwShareMode, lpSecAtt,
		dwCreationDisposition, dwFlags, hTemplate);
}
//---------------------------------------------------------------------------//
//---------------------------------------------------------------------------//
//---------------------------------------------------------------------------//
void WINAPI Hook_ExitProcess(UINT uExitCode)
{
	SendIPCMessage(UOMessage::Closing);
	ExitProcess(uExitCode);
}

ATOM WINAPI Hook_RegisterClassA(WNDCLASSA *lpWndClass)
{
	oldWndProc = lpWndClass->lpfnWndProc;
	lpWndClass->lpfnWndProc = WndProc;
	return RegisterClassA(lpWndClass);
}

ATOM WINAPI Hook_RegisterClassW(WNDCLASSW *lpWndClass)
{
	oldWndProc = lpWndClass->lpfnWndProc;
	lpWndClass->lpfnWndProc = WndProc;
	return RegisterClassW(lpWndClass);
}
//---------------------------------------------------------------------------//
//---------------------------------------------------------------------------//
//---------------------------------------------------------------------------//
UINT connect_address;
USHORT connect_port;
void SetConnectionInfo(UINT address, USHORT port)
{
	connect_address = address;
	connect_port = port;
}

SOCKET currentSocket;
int WINAPI Hook_connect(SOCKET s, sockaddr_in* inaddr, int namelen)
{
	if (connect_address)
		inaddr->sin_addr.s_addr = connect_address;

	if (connect_port)
		inaddr->sin_port = ntohs(connect_port);

	int result = connect(currentSocket = s, (sockaddr*)inaddr, namelen);
	SendIPCMessage(UOMessage::Connected);
	return result;
}

int WINAPI Hook_closesocket(SOCKET s)
{
	if (s == currentSocket)
		SendIPCMessage(UOMessage::Disconnecting);
	return closesocket(s);
}
//---------------------------------------------------------------------------//
//---------------------------------------------------------------------------//
//---------------------------------------------------------------------------//
void HookImports()
{
	GetModuleFileNameA(GetModuleHandle(nullptr), clientDirA, sizeof(clientDirA));
	PathRemoveFileSpecA(clientDirA);
	GetModuleFileNameW(GetModuleHandle(nullptr), clientDirW, sizeof(clientDirW));
	PathRemoveFileSpecW(clientDirW);

	if (!HookImport("kernel32.dll", "ExitProcess", 0, Hook_ExitProcess))
		throw L"ImportHooks: ExitProcess";

	bool result = HookImport("kernel32.dll", "GetCurrentDirectoryA", 0, Hook_GetCurrentDirectoryA) |
		HookImport("kernel32.dll", "GetCurrentDirectoryW", 0, Hook_GetCurrentDirectoryW);
	if (!result)
		throw L"ImportHooks: GetCurrentDirectory";

	result = HookImport("kernel32.dll", "CreateFileA", 0, Hook_CreateFileA) |
		HookImport("kernel32.dll", "CreateFileW", 0, Hook_CreateFileW);
	if (!result)
		throw L"ImportHooks: CreateFile";

	result = HookImport("user32.dll", "RegisterClassA", 0, Hook_RegisterClassA) |
		HookImport("user32.dll", "RegisterClassW", 0, Hook_RegisterClassW);
	if (!result)
		throw L"ImportHooks: RegisterClass";

	result = HookImport("wsock32.dll", "connect", 4, Hook_connect) |
		HookImport("WS2_32.dll", "connect", 4, Hook_connect);
	if (!result)
		throw L"ImportHooks: connect";

	result = HookImport("wsock32.dll", "closesocket", 3, Hook_closesocket) |
		HookImport("WS2_32.dll", "closesocket", 3, Hook_closesocket);
	if (!result)
		throw L"ImportHooks: closesocket";
}