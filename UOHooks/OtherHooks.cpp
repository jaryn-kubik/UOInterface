#include "stdafx.h"
#include "OtherHooks.h"
#include "IPC.h"

namespace Hooks
{
	UINT pathfindingType;
	LPVOID pathfindingFunc;
	bool HookPathfinding(Client &client)
	{
		ud_instr sig1[] =
		{
			ud_instr{ UD_Imovsx, { ud_arg::reg(UD_R_EBP), ud_arg::mem(UD_R_EAX, 0x24) } },
			ud_instr{ UD_Imovsx, { ud_arg::reg(UD_R_EDX), ud_arg::mem(UD_R_EAX, 0x26) } },
			ud_instr{ UD_Imovsx, { ud_arg::reg(UD_R_EAX), ud_arg::mem(UD_R_EAX, 0x28) } }
		};

		ud_instr sig2[] =
		{
			ud_instr{ UD_Imovsx, { ud_arg::reg(UD_R_EDX), ud_arg::mem(UD_R_EAX, 0x26) } },
			ud_instr{ UD_Ipush, { ud_arg::reg(UD_R_EBX) } },
			ud_instr{ UD_Imov, { ud_arg::reg(UD_R_EBX), ud_arg::mem(UD_NONE) } },
			ud_instr{ UD_Ipush, { ud_arg::reg(UD_R_EBP) } }
		};

		int i;
		if (client.Find(sig1, &i))
			pathfindingType = 1;
		else if (client.Find(sig2, &i))
			pathfindingType = 2;
		else
			return false;

		pathfindingFunc = client[i].offset;
		return true;
	}

	USHORT pathfindingData[0x15];
	void __stdcall Pathfind()
	{
		_asm
		{
			lea		eax, pathfindingData;
			sub		esp, 8;
			push	end;

			cmp		pathfindingType, 2;
			je		type2;

			push	ecx;
			push	ebx;
			push	ebp;
			push	esi;
			push	edi;
			jmp		pathfindingFunc;

		type2:
			sub		esp, 4;
			jmp		pathfindingFunc;

		end:
		}
	}

	void Pathfind(USHORT x, USHORT y, USHORT z)
	{
		pathfindingData[0x12] = x;
		pathfindingData[0x13] = y;
		pathfindingData[0x14] = z;
		Pathfind();
	}
	//---------------------------------------------------------------------------//
	//---------------------------------------------------------------------------//
	//---------------------------------------------------------------------------//
	LPVOID gameSizeFunc;
	int sizeW = 800, sizeH = 600;

	void __declspec(naked) GameSizeHook()
	{
		_asm
		{
			mov		eax, [esp + 4];
			mov		edx, 640;
			cmp		eax, edx;
			mov		ecx, 480;
			je		end;

			mov		edx, sizeW;
			mov		ecx, sizeH;
			cmp		eax, eax;

		end:
			jmp		gameSizeFunc;
		}
	}

	bool HookGameSize(Client &client)
	{
		ud_instr sig[] =
		{
			ud_instr{ UD_Imov, { ud_arg::reg(UD_R_EAX), ud_arg::mem(UD_R_ESP, 4) } },
			ud_instr{ UD_Imov, { ud_arg::reg(UD_R_EDX), ud_arg::imm(640) } },
			ud_instr{ UD_Icmp, { ud_arg::reg(UD_R_EAX), ud_arg::reg(UD_R_EDX) } },
			ud_instr{ UD_Imov, { ud_arg::reg(UD_R_ECX), ud_arg::imm(480) } }
		};

		int i;
		if (client.Find(sig, &i))
		{
			gameSizeFunc = client.Hook(i, GameSizeHook, 4);
			return true;
		}
		return false;
	}

	void SetGameSize(int width, int height)
	{
		if (width > 0)
			sizeW = width;
		if (height > 0)
			sizeH = height;
	}
	//---------------------------------------------------------------------------//
	//---------------------------------------------------------------------------//
	//---------------------------------------------------------------------------//
	LPVOID mainClose, mainRet;
	void __declspec(naked) MainHook()
	{
		__asm
		{
			jz close;
			call IPC::Process;
			jmp mainRet;

		close:
			jmp mainClose;
		}
	}

	bool HookMain(Client &client)
	{
		ud_instr sig[] =
		{
			ud_instr{ UD_Icall },
			ud_instr{ UD_Itest, { ud_arg::reg(UD_R_EAX), ud_arg::reg(UD_R_EAX) } },
			ud_instr{ UD_Ijnz },
			ud_instr{ UD_Icmp, { ud_arg::mem(UD_R_ESP), ud_arg::imm(WM_QUIT) } },
			ud_instr{ UD_Ijz }
		};

		int i;
		if (client.Find(sig, &i))
		{
			mainClose = client[i + 4].destination();
			mainRet = client.Hook(i + 4, MainHook);
			return true;
		}
		return false;
	}
	//---------------------------------------------------------------------------//
	//---------------------------------------------------------------------------//
	//---------------------------------------------------------------------------//
	void Other(Client &client)
	{
		if (!HookPathfinding(client))
			throw L"Hooks: Pathfinding";
		if (!HookGameSize(client))
			throw L"Hooks: GameSize";
		if (!HookMain(client))
			throw L"Hooks: Main";
	}
}