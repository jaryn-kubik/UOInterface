#pragma once
#include "Client.h"

namespace Hooks
{
	void Packets(Client &client);
	void SendPacket(BYTE *buffer);
	void RecvPacket(BYTE *buffer);
	UINT* GetPacketTable();

	extern LPVOID sendFunc, recvFunc;
	extern UINT *vtbl;
}