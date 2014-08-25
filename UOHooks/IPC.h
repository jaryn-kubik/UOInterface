#pragma once

#ifdef UOINTERFACE_EXPORTS
#define UOINTERFACE_API(ret) extern "C" __declspec(dllexport) ret _stdcall
#else
#define UOINTERFACE_API(ret) __declspec(dllexport) ret _stdcall
#endif

namespace IPC
{
	enum KeyModifiers { None = 0, Alt = 1, Control = 2, Shift = 4 };
	enum UOMessage
	{
		First = WM_USER,
		Ready = First, Connected, Disconnecting, Closing, Focus, Visibility,
		KeyDown, PacketToClient, PacketToServer,
		ConnectionInfo, Pathfinding, GameSize,
		Last = GameSize
	};

	HANDLE Init(HWND hwnd, HANDLE hProcess);
	DWORD WINAPI OnAttach(LPVOID mapping);
	BOOL Send(UOMessage msg, UINT wParam = 0, UINT lParam = 0);
	BOOL SendData(UOMessage msg, LPVOID data, UINT len);
	void OnMessage(UINT msg, WPARAM wParam, LPARAM lParam);
	void OnWindowCreated(HWND hwnd);

	UOINTERFACE_API(byte*) GetInBuffer();
	UOINTERFACE_API(byte*) GetOutBuffer();
	UOINTERFACE_API(short*) GetPacketTable();
	UOINTERFACE_API(void) SendUOMessage(UOMessage msg, int wParam, int lParam);
}