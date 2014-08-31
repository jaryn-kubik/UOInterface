#pragma once

namespace IPC
{
	enum KeyModifiers { None = 0, Alt = 1, Control = 2, Shift = 4 };
	enum UOMessage
	{
		Ready, Connected, Disconnecting, Closing, Focus, Visibility,
		KeyDown, PacketToClient, PacketToServer,
		ConnectionInfo, Pathfinding, GameSize
	};

	extern "C" __declspec(dllexport) DWORD WINAPI OnAttach(LPVOID pId);
	BOOL Send(UOMessage msg, UINT arg1 = 0, UINT arg2 = 0, UINT arg3 = 0);
	BOOL SendData(UOMessage msg, LPVOID data, UINT len);
}