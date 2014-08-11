#include "stdafx.h"
#include "MacroHooks.h"
#include "Client.h"

namespace Hooks
{
	UINT pathfindingType;
	LPVOID pathfindingFunc;
	bool HookPathfinding()
	{
		byte sig1[] =
		{
			0x0F, 0xBF, 0x68, 0x24,					//movsx		ebp, word ptr [eax+24h]
			0x0F, 0xBF, 0x50, 0x26,					//movsx		edx, word ptr [eax+26h]
			0x0F, 0xBF, 0x40, 0x28					//movsx		eax, word ptr [eax+28h]
		};

		byte sig2[] =
		{
			0x0F, 0xBF, 0x50, 0x26,					//movsx		edx, word ptr [eax+26h]
			0x53,									//push		ebx
			0x8B, 0x1D, 0xCC, 0xCC, 0xCC, 0xCC,		//mov		ebx, dword_XXYYXXYY
			0x55									//push		ebp
		};

		bool result;
		if (result = Client::FindCode(sig1, (byte**)&pathfindingFunc))
			pathfindingType = 1;
		else if (result = Client::FindCode(sig2, (byte**)&pathfindingFunc))
			pathfindingType = 2;
		return result;
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

	void Macros()
	{
		if (!HookPathfinding())
			throw L"Macros: Pathfinding";
	}
}