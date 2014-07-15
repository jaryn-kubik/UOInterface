#pragma once

#ifdef UOINTERFACE_EXPORTS
#define UOINTERFACE_API(ret) extern "C" __declspec(dllexport) ret _stdcall
#else
#define UOINTERFACE_API(ret) __declspec(dllexport) ret _stdcall
#endif

enum struct KeyModifiers { None = 0, Alt = 1, Control = 2, Shift = 4 };
enum struct UOMessage
{
	First = WM_USER,
	Init = First, Connected, Disconnecting, Closing, Focus, Visibility,
	KeyDown, PacketToClient, PacketToServer,
	ConnectionInfo, Pathfinding, Patch,
	Last = Patch
};
typedef UINT(*OnUOMessage)(UOMessage msg, int wParam, int lParam);

UOINTERFACE_API(DWORD) Start(LPWSTR client, OnUOMessage onMessage);
UOINTERFACE_API(void) Inject(DWORD pid, OnUOMessage onMessage);

UOINTERFACE_API(byte*) GetInBuffer();
UOINTERFACE_API(byte*) GetOutBuffer();
UOINTERFACE_API(short*) GetPacketTable();

UOINTERFACE_API(void) SendUOMessage(UOMessage msg, int wParam, int lParam);