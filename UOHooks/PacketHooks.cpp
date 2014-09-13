#include "stdafx.h"
#include "PacketHooks.h"
#include "IPC.h"
#include <atomic>

namespace Hooks
{
	struct PacketInfo{ UINT id, unknown, len; };
	UINT packetTable[0x100];
	bool FindPacketTable(Client &client)
	{
		unsigned char sig[] =
		{
			0x01, 0x00, 0x00, 0x00, 0xCC, 0xCC, 0xCC, 0xCC, 0x05, 0x00, 0x00, 0x00, //packet 1, unknown, len 5
			0x02, 0x00, 0x00, 0x00, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, //packet 2, unknown, len ...
			0x03, 0x00, 0x00, 0x00, 0xCC, 0xCC, 0xCC, 0xCC, 0x00, 0x80, 0x00, 0x00  //packet 3, unknown, len 0x8000 (dynamic)
		};

		BYTE *offset;
		if (!client.Find(sig, &offset))
			return false;

		PacketInfo *table = ((PacketInfo*)offset) - 1;
		for (UINT unknown = table->unknown; table->unknown == unknown; table++)
			packetTable[table->id] = table->len;

		return true;
	}

	UINT* GetPacketTable()	{ return packetTable; }
	UINT GetPacketLength(BYTE* buffer)
	{
		UINT len = packetTable[buffer[0]];
		if (len == 0x8000)
			len = *((USHORT *)(buffer + 1));
		return len;
	}
	//---------------------------------------------------------------------------//
	//---------------------------------------------------------------------------//
	//---------------------------------------------------------------------------//
	UINT *vtbl, *networkObject;
	bool FindNetworkObject(Client &client)
	{
		ud_instr sig[] =
		{
			ud_instr{ UD_Imov, { ud_arg::mem(UD_R_ESP), ud_arg::reg(UD_R_EAX) } },
			ud_instr{ UD_Ijz, { ud_arg::jimm() } },
			ud_instr{ UD_Imov, { ud_arg::reg(UD_R_ECX), ud_arg::reg(UD_R_ESI) } },
			ud_instr{ UD_Icall, { ud_arg::jimm() } },
			ud_instr{ UD_Imov, { ud_arg::mem(UD_R_ESI), ud_arg::imm() } },
			ud_instr{ UD_Imov, { ud_arg::mem(), ud_arg::reg(UD_R_ESI) } }
		};

		int i;
		if (client.Find(sig, &i))
		{
			vtbl = (UINT*)client[i + 4].args[1].val();
			networkObject = (UINT*)client[i + 5].args[0].val();
			return true;
		}
		return false;
	}

	inline void CallNetworkFunc(LPVOID func, BYTE *buffer)
	{
		__asm
		{
			mov		ecx, [networkObject];
			mov		ecx, [ecx];
			push	buffer;
			call	func;
		}
	}
	//---------------------------------------------------------------------------//
	//---------------------------------------------------------------------------//
	//---------------------------------------------------------------------------//
	LPVOID sendFunc;
	inline void SendPacket(BYTE *buffer) { CallNetworkFunc(sendFunc, buffer); }
	void __stdcall SendHook(BYTE *buffer)
	{
		if (!SendData(IPC::PacketToServer, buffer, GetPacketLength(buffer)))
			SendPacket(buffer);
	}

	bool HookSend(Client &client)
	{
		ud_instr iPush{ UD_Ipush, { ud_arg::reg(UD_R_EBX) } };
		ud_instr sig[] =
		{
			ud_instr{ UD_Ipush, { ud_arg::reg(UD_R_EAX) } },
			ud_instr{ UD_Ilea, { ud_arg::reg(UD_R_ECX), ud_arg::mem() } },
			ud_instr{ UD_Icall, { ud_arg::jimm() } },
			ud_instr{ UD_Ipush, { ud_arg::reg() } },
			ud_instr{ UD_Ilea, { ud_arg::reg(UD_R_ECX), ud_arg::mem() } },
			ud_instr{ UD_Icall, { ud_arg::jimm() } },
			ud_instr{ UD_Imov, { ud_arg::reg(UD_R_AL), ud_arg::mem(UD_R_ESI, 0) } },
			ud_instr{ UD_Icmp, { ud_arg::reg(UD_R_AL), ud_arg::imm() } },
		};

		int i;
		if (client.Find(sig, &i) && client.Find(UD_Icall, &i, -16) && client.Find(iPush, &i, -8))
		{
			sendFunc = client.Hook(client[i].offset, SendHook);
			return true;
		}
		return false;
	}
	//---------------------------------------------------------------------------//
	//---------------------------------------------------------------------------//
	//---------------------------------------------------------------------------//
	LPVOID recvFunc;
	inline void RecvPacket(BYTE *buffer) { CallNetworkFunc(recvFunc, buffer); }
	void __stdcall RecvHook(BYTE *buffer)
	{
		if (!SendData(IPC::PacketToClient, buffer, GetPacketLength(buffer)))
			RecvPacket(buffer);
	}

	bool HookRecv(Client &client)
	{
		recvFunc = (LPVOID)vtbl[8];
		client.Set(&vtbl[8], (UINT)RecvHook);
		return true;
	}
	//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||//
	//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||//
	//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||//
	void Packets(Client &client)
	{
		if (!FindNetworkObject(client))
			throw L"PacketHooks: FindVtbl";
		if (!HookSend(client))
			throw L"PacketHooks: HookSend";
		if (!HookRecv(client))
			throw L"PacketHooks: HookRecv";
		if (!FindPacketTable(client))
			throw L"PacketHooks: FindPacketTable";
	}
}