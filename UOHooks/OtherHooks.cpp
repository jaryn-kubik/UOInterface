#include "stdafx.h"
#include "OtherHooks.h"
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
	//---------------------------------------------------------------------------//
	//---------------------------------------------------------------------------//
	//---------------------------------------------------------------------------//
	LPVOID gameSizeFunc;
	int sizeW = 800, sizeH = 600;

	__inline void __declspec(naked) __stdcall GameSizeHook()
	{
		_asm
		{
			mov		eax, [esp + 4];
			mov		edx, 280h;
			cmp		eax, edx;
			mov		ecx, 1E0h;
			je		end;

			mov		edx, sizeW;
			mov		ecx, sizeH;
			cmp		eax, eax;

		end:
			jmp		gameSizeFunc;
		}
	}

	bool HookGameSize()
	{
		byte sig[] =
		{
			0x8B, 0x44, 0x24, 0x04,					//mov		eax, [esp+arg_0]
			0xBA, 0x80, 0x02, 0x00, 0x00,			//mov		edx, 280h
			0x3B, 0xC2,								//cmp		eax, edx
			0xB9, 0xE0, 0x01, 0x00, 0x00			//mov		ecx, 1E0h
		};

		byte *offset;
		if (Client::FindCode(sig, &offset))
		{
			gameSizeFunc = Client::Hook(offset, &GameSizeHook, sizeof(sig));
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

	void Other()
	{
		if (!HookPathfinding())
			throw L"Hooks: Pathfinding";
		if (!HookGameSize())
			throw L"Hooks: GameSize";
	}
}