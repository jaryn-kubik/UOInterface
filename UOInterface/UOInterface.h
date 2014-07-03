#pragma once

#ifdef UOINTERFACE_EXPORTS
#define UOINTERFACE_API(ret) extern "C" __declspec(dllexport) ret _stdcall
#else
#define UOINTERFACE_API(ret) __declspec(dllexport) ret _stdcall
#endif

enum struct UOMessage
{
	Init = WM_USER, Focus, Visibility, Disconnect, ExitProcess,
	KeyDown, PacketToClient, PacketToServer,
	ConnectionInfo, Pathfinding, Patch
};

UOINTERFACE_API(void) Start(LPWSTR client, HWND hwnd);
UOINTERFACE_API(void) Inject(DWORD pid, HWND hwnd);

UOINTERFACE_API(byte*) GetInBuffer();
UOINTERFACE_API(byte*) GetOutBuffer();
UOINTERFACE_API(short*) GetPacketTable();

UOINTERFACE_API(void) SendIPCMessage(UOMessage msg, UINT wParam, UINT lParam);