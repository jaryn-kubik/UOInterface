#pragma once

namespace Hooks
{
	void Packets();
	void SendPacket(BYTE *buffer);
	void RecvPacket(BYTE *buffer);
	UINT* GetPacketTable();
}