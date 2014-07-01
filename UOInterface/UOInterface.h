#pragma once

#ifdef UOINTERFACE_EXPORTS
#define UOINTERFACE_API(ret) extern "C" __declspec(dllexport) ret _stdcall
#else
#define UOINTERFACE_API(ret) __declspec(dllexport) ret _stdcall
#endif

const int BufferSize = 0x80000;
const LPCWSTR BufferInName = L"UOInterface_In_%d";
const LPCWSTR BufferOutName = L"UOInterface_Out_%d";

enum struct UOMessage
{
	ExitProcess = WM_USER, Disconnect, WindowCreated, Focus, Visibility,
	KeyDown, ConnectionInfo, PacketLengths, PacketToServer, PacketToClient
};

UOINTERFACE_API(DWORD) Start(LPWSTR client, HWND hwnd);
UOINTERFACE_API(void) Inject(DWORD pid, HWND hwnd);