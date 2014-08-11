#include "stdafx.h"
#include "PacketHooks.h"
#include "Client.h"
#include "IPC.h"

namespace Hooks
{
	struct PacketInfo{ UINT id, unknown, len; };
	short *packetTable;
	bool FindPacketTable()
	{
		packetTable = IPC::GetPacketTable();
		unsigned char sig[] =
		{
			0x01, 0x00, 0x00, 0x00, 0xCC, 0xCC, 0xCC, 0xCC, 0x05, 0x00, 0x00, 0x00, //packet 1, unknown, len 5
			0x02, 0x00, 0x00, 0x00, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, //packet 2, unknown, len ...
			0x03, 0x00, 0x00, 0x00, 0xCC, 0xCC, 0xCC, 0xCC, 0x00, 0x80, 0x00, 0x00  //packet 3, unknown, len 0x8000 (dynamic)
		};

		byte *offset;
		if (!Client::FindData(sig, &offset))
			return false;

		PacketInfo *table = ((PacketInfo*)offset) - 1;
		for (UINT unknown = table->unknown; table->unknown == unknown; table++)
			packetTable[table->id] = table->len;

		return true;
	}

	USHORT GetPacketLength(byte* buffer)
	{
		USHORT len = packetTable[buffer[0]];
		if (len == 0x8000)
			len = *((USHORT *)(buffer + 1));
		return len;
	}
	//---------------------------------------------------------------------------//
	//---------------------------------------------------------------------------//
	//---------------------------------------------------------------------------//
	UINT networkObject;
	UINT sendType;
	LPVOID sendFunc, recvFunc;

	BOOL __stdcall OnSend(byte *buffer)	{ return IPC::SendData(IPC::PacketToServer, buffer, GetPacketLength(buffer)); }
	BOOL __stdcall OnRecv(byte *buffer)	{ return IPC::SendData(IPC::PacketToClient, buffer, GetPacketLength(buffer)); }

	//calls the original send function (restores overriden code)
	__inline void __declspec(naked) __stdcall SendFunc()
	{
		_asm
		{
			cmp		sendType, 2;
			je		end2;

			push    ebx;
			push    ebp;
			push    esi;
			mov     esi, [esp + 0Ch + 4];
			jmp		sendFunc;

		end2:
			push	ebx;
			push    esi;
			mov		esi, [esp + 08h + 4];
			jmp		sendFunc;
		}
	}

	__inline void __declspec(naked) __stdcall SendHook()
	{
		_asm
		{
			//save state
			push	eax;
			push	ecx;
			push    esi;

			//save network object, call our packet handler
			mov		networkObject, ecx;
			mov		esi, [esp + 0Ch + 4];//buffer
			push	esi;
			call	OnSend;
			test	eax, eax;

			//restore state
			pop		esi;
			pop		ecx;
			pop		eax;

			jnz		filter;//if result is not zero (==true) -> filter the packet
			jmp		SendFunc;//else jump back to original function

		filter:
			ret		4;
		}
	}

	void SendPacket(byte *buffer)
	{
		_asm
		{
			mov		ecx, networkObject;
			push	buffer;
			call	SendFunc;
		}
	}

	bool HookSend()
	{
		byte sig1[] =
		{
			0x8D, 0x8B, 0x94, 0x00, 0x00, 0x00,		//lea		ecx, [ebx+94h]
			0xE8, 0xCC, 0xCC, 0xCC, 0xCC,			//call		sub_XXYYXXYY
			0x55,									//push		ebp
			0x8D, 0x8B, 0xBC, 0x00, 0x00, 0x00		//lea		ecx, [ebx+0BCh]
		};

		byte sig2[] = {
			0x0F, 0xB7, 0xD8, 0x0F, 0xB6, 0x06, 0x83, 0xC4,
			0x04, 0x53, 0x50, 0x8D, 0x4F, 0x6C };

		byte *offset;
		if (Client::FindCode(sig1, &offset))
		{
			sendFunc = Client::Hook(offset - 0x22, &SendHook, 7);
			sendType = 1;
			return true;
		}
		if (Client::FindCode(sig2, &offset))
		{
			sendFunc = Client::Hook(offset - 0x0F, &SendHook, 6);
			sendType = 2;
			return true;
		}
		return false;
	}
	//---------------------------------------------------------------------------//
	//---------------------------------------------------------------------------//
	//---------------------------------------------------------------------------//
	__inline void __declspec(naked) __stdcall RecvHook()
	{
		_asm
		{
			//save state
			push	eax;
			push	ecx;
			push    esi;

			//call our packet handler
			mov		esi, [esp + 0Ch + 4];//buffer
			push	esi;
			call	OnRecv;
			test	eax, eax;

			//restore state
			pop		esi;
			pop		ecx;
			pop		eax;

			jnz		filter;//if result is not zero (==true) -> filter the packet
			jmp		recvFunc;//else jump back to original function

		filter:
			ret		4;
		}
	}

	void RecvPacket(byte* buffer)
	{
		_asm
		{
			mov		ecx, networkObject;
			push	buffer;
			call	recvFunc;
		}
	}

	bool HookRecv()
	{
		byte sig1[] =
		{
			0xE8, 0xCC, 0xCC, 0xCC, 0xCC,			//call		sub_XXYYXXYY
			0xF7, 0xD8,								//neg		eax
			0x1B, 0xC0,								//sbb		eax, eax
			0xF7, 0xD8,								//neg		eax
			0xC3									//retn
		};

		byte sig2[] =
		{
			0x56,									//push		esi
			0x8B, 0xF1,								//mov		esi, ecx
			0xE8, 0xCC, 0xCC, 0xCC, 0xCC,			//call		sub_4196C0
			0xC7, 0x06, 0xCC, 0xCC, 0xCC, 0xCC,		//mov		dword ptr [esi], offset off_XXYYXXYY
			0x8B, 0xC6,								//mov		eax, esi
			0x5E,									//pop		esi
			0xC3									//retn
		};

		byte *offset;
		if (Client::FindCode(sig1, &offset))
		{
			UINT address = (UINT)offset;
			if (Client::FindData((byte*)&address, 4, &offset))
			{
				UINT *pRecv = (UINT*)(offset + 8);
				recvFunc = (LPVOID)*pRecv;
				Client::Set(pRecv, (UINT)&RecvHook);
				return true;
			}
		}

		if (Client::FindCode(sig2, &offset))
		{
			UINT* vtbl = (UINT*)*((UINT*)(offset + 2));
			recvFunc = (LPVOID)vtbl[8];
			Client::Set(&vtbl[8], (UINT)&RecvHook);
			return true;
		}
		return false;
	}
	//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||//
	//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||//
	//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||//
	void Packets()
	{
		if (!HookSend())
			throw L"PacketHooks: HookSend";
		if (!HookRecv())
			throw L"PacketHooks: HookRecv";
		if (!FindPacketTable())
			throw L"PacketHooks: FindPacketTable";
	}
}