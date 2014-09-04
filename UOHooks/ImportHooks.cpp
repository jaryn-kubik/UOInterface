#include "stdafx.h"
#include "ImportHooks.h"
#include "Client.h"
#include "IPC.h"
#include "WinSock2.h"
#include <Shlwapi.h>

namespace Hooks
{
	int GetKeyModifiers()
	{
		int mods = 0;

		if (GetKeyState(VK_MENU) & 0xFF00)
			mods |= IPC::Alt;

		if (GetKeyState(VK_CONTROL) & 0xFF00)
			mods |= IPC::Control;

		if (GetKeyState(VK_SHIFT) & 0xFF00)
			mods |= IPC::Shift;

		return mods;
	}

	WPARAM keyToIgnore;
	WNDPROC oldWndProc;
	LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
	{
		switch (msg)
		{
		case WM_SETFOCUS:
			IPC::Send(IPC::Focus, TRUE);
			break;

		case WM_KILLFOCUS:
			IPC::Send(IPC::Focus, FALSE);
			break;

		case WM_SIZE:
			IPC::Send(IPC::Visibility, wParam != SIZE_MINIMIZED);
			break;

		case WM_MBUTTONDOWN:
			if (IPC::Send(IPC::KeyDown, VK_MBUTTON, GetKeyModifiers()))
				return 0;
			break;

		case WM_XBUTTONDOWN:
			if (IPC::Send(IPC::KeyDown, HIWORD(wParam) == XBUTTON1 ? VK_XBUTTON1 : VK_XBUTTON2, GetKeyModifiers()))
				return 0;
			break;

		case WM_MOUSEWHEEL:
			if (IPC::Send(IPC::KeyDown, GET_WHEEL_DELTA_WPARAM(wParam) < 0 ? VK_F23 : VK_F24, GetKeyModifiers()))
				return 0;
			break;

		case WM_SYSDEADCHAR:
		case WM_SYSKEYDOWN:
		case WM_KEYDOWN:
			if (wParam != VK_SHIFT && wParam != VK_CONTROL && wParam != VK_MENU &&
				(lParam & (1 << 30)) == 0 && IPC::Send(IPC::KeyDown, wParam, GetKeyModifiers()))
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
			IPC::SetHWND(hwnd);
			break;
		}
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
		IPC::Send(IPC::Closing);
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
		IPC::Send(IPC::Connected);
		return result;
	}

	int WINAPI Hook_closesocket(SOCKET s)
	{
		if (s == currentSocket)
			IPC::Send(IPC::Disconnecting);
		return closesocket(s);
	}
	//---------------------------------------------------------------------------//
	//---------------------------------------------------------------------------//
	//---------------------------------------------------------------------------//
	void Imports()
	{
		GetModuleFileNameA(GetModuleHandle(nullptr), clientDirA, sizeof(clientDirA));
		PathRemoveFileSpecA(clientDirA);
		GetModuleFileNameW(GetModuleHandle(nullptr), clientDirW, sizeof(clientDirW));
		PathRemoveFileSpecW(clientDirW);

		if (!Client::Hook("kernel32.dll", "ExitProcess", Hook_ExitProcess))
			throw L"ImportHooks: ExitProcess";

		bool result = Client::Hook("kernel32.dll", "GetCurrentDirectoryA", Hook_GetCurrentDirectoryA) |
			Client::Hook("kernel32.dll", "GetCurrentDirectoryW", Hook_GetCurrentDirectoryW);
		if (!result)
			throw L"ImportHooks: GetCurrentDirectory";

		result = Client::Hook("kernel32.dll", "CreateFileA", Hook_CreateFileA) |
			Client::Hook("kernel32.dll", "CreateFileW", Hook_CreateFileW);
		if (!result)
			throw L"ImportHooks: CreateFile";

		result = Client::Hook("user32.dll", "RegisterClassA", Hook_RegisterClassA) |
			Client::Hook("user32.dll", "RegisterClassW", Hook_RegisterClassW);
		if (!result)
			throw L"ImportHooks: RegisterClass";

		result = Client::Hook("wsock32.dll", "connect", Hook_connect, 4) |
			Client::Hook("WS2_32.dll", "connect", Hook_connect, 4);
		if (!result)
			throw L"ImportHooks: connect";

		result = Client::Hook("wsock32.dll", "closesocket", Hook_closesocket, 3) |
			Client::Hook("WS2_32.dll", "closesocket", Hook_closesocket, 3);
		if (!result)
			throw L"ImportHooks: closesocket";
	}
}