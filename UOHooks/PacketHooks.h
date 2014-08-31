#pragma once

namespace Hooks
{
	void Packets();
	void SendPacket(byte *buffer);
	void RecvPacket(byte *buffer);
	UINT* GetPacketTable();
}