#include "stdafx.h"
#include "Patches.h"
#include "Client.h"
#include "PacketHooks.h"

namespace Patches
{
	bool ProtocolDecryption(Client &client)
	{
		ud_instr iCmp{ UD_Icmp, { ud_arg::reg(UD_R_EDI), ud_arg::imm(0xFF) } };

		int i;
		if (!client.Find((LPVOID)Hooks::vtbl[5], &i))
			return false;//get fifth function in socket vtbl

		if (!client.Find(iCmp, &i, 32))
			return false;//find cmp, -1 in this function

		int dest = i += 2;
		if (!client.Find(UD_Ijz, &dest, 4))
			return false;//find conditional jump after cmp

		client.Hook(i, client[dest].destination(), 3);
		return true;
	}

	bool LoginEncryption(Client &client)
	{
		ud_instr iRet{ UD_Iret, { ud_arg::imm(4) } };
		ud_instr iCmp{ UD_Icmp, { ud_arg::reg(), ud_arg::imm(0x10000) } };

		int start;
		if (!client.Find(Hooks::sendFunc, &start))
			return false;//find beginning of the function

		int end = start;
		if (!client.Find(iRet, &end, 128))
			return false;//find end of the function

		int i = start;
		if (!client.Find(iCmp, &i, end - start))	//find cmp, 0x10000 in this function
			if (!client.FindAndFollow(UD_Icall, &i, start - end) || !client.Find(iCmp, &i, 16))
				return false;//or in a call in this function

		int dest = i += 2;
		if (!client.Find(UD_Ijnz, &dest, 8))
			return false;//find conditional jump after cmp

		client.Hook(i, client[dest].destination());
		return true;
	}

	bool TwoFishEncryption(Client &client)
	{
		int i;
		if (!client.Find((LPVOID)Hooks::vtbl[6], &i))
			return false;//get sixth function in socket vtbl

		if (!client.FindAndFollow(UD_Icall, &i, 4))
			return false;//follow first call

		if (!client.FindAndFollow(UD_Ijnz, &i, 8))
			return false;//follow first jnz

		int dest = i;
		if (!client.Find(UD_Ijz, &dest, 4))
			return false;//find next jz

		client.Hook(i, client[dest].destination());
		return true;
	}

	void Encryption(Client &client)
	{
		if (!ProtocolDecryption(client))
			throw L"PatchEncryption: ProtocolDecryption";
		if (!LoginEncryption(client))
			throw L"PatchEncryption: LoginEncryption";
		if (!TwoFishEncryption(client))
			throw L"PatchEncryption: TwoFishEncryption";
	}
	//---------------------------------------------------------------------------//
	//---------------------------------------------------------------------------//
	//---------------------------------------------------------------------------//
	HWND WINAPI Hook_FindWindowA(LPCSTR lpClassName, LPCSTR lpWindowName)
	{
		if (lpWindowName == nullptr && strcmp(lpClassName, "Ultima Online") == 0)
			return nullptr;
		return FindWindowA(lpClassName, lpWindowName);
	}

	HANDLE WINAPI Hook_CreateFileMappingA(HANDLE hFile, LPSECURITY_ATTRIBUTES lpAttributes,
		DWORD flProtect, DWORD dwSizeHigh, DWORD dwSizeLow, LPCSTR lpName)
	{
		HANDLE result = CreateFileMappingA(hFile, lpAttributes, flProtect, dwSizeHigh, dwSizeLow, lpName);
		if (lpName != nullptr && strcmp(lpName, "UoClientApp") == 0 && GetLastError() == ERROR_ALREADY_EXISTS)
			SetLastError(0);
		return result;
	}

	char mutexName[32];
	HANDLE WINAPI Hook_OpenMutexA(DWORD dwDesiredAccess, BOOL bInheritHandle, LPCSTR lpName)
	{
		if (lpName != nullptr && strcmp(lpName, "report") == 0)
			lpName = mutexName;
		return OpenMutexA(dwDesiredAccess, bInheritHandle, lpName);
	}

	HANDLE WINAPI Hook_CreateMutexA(LPSECURITY_ATTRIBUTES lpMutexAttributes, BOOL bInitialOwner, LPCSTR lpName)
	{
		if (lpName != nullptr && strcmp(lpName, "report") == 0)
			lpName = mutexName;
		return CreateMutexA(lpMutexAttributes, bInitialOwner, lpName);
	}

	void Multi(Client &client)
	{
		if (!client.Hook("user32.dll", "FindWindowA", Hook_FindWindowA))
			throw L"PatchMulti: FindWindowA";

		if (!client.Hook("kernel32.dll", "CreateFileMappingA", Hook_CreateFileMappingA))
			throw L"PatchMulti: CreateFileMappingA";

		if (!client.Hook("kernel32.dll", "OpenMutexA", Hook_OpenMutexA))
			throw L"PatchMulti: OpenMutexA";

		if (!client.Hook("kernel32.dll", "CreateMutexA", Hook_CreateMutexA))
			throw L"PatchMulti: CreateMutexA";

		sprintf_s(mutexName, "report_%d", GetCurrentProcessId());
	}
	//---------------------------------------------------------------------------//
	//---------------------------------------------------------------------------//
	//---------------------------------------------------------------------------//
	void Intro(Client &client)
	{
		BYTE intro[10] = "intro.bik";
		BYTE osilogo[12] = "osilogo.bik";
		BYTE splash[12] = "Splash gump";

		BYTE *offset;
		if (client.Find(intro, &offset))
			client.Set<BYTE>(offset, '_');

		if (client.Find(osilogo, &offset))
			client.Set<BYTE>(offset, '_');

		if (client.Find(splash, &offset))
		{
			ud_instr iSplash{ UD_Imov, { ud_arg::mem(UD_R_ESI, 8), ud_arg::imm((UINT)offset) } };
			ud_instr iTimeout{ UD_Imov, { ud_arg::reg(UD_R_EAX), ud_arg::mem(UD_R_ESP) } };

			int i;
			if (client.Find(iSplash, &i) && client.Find(iTimeout, &i, 32))
			{
				BYTE *offset = (BYTE*)client[i].offset;
				//xor eax, eax
				client.Set<BYTE>(offset, 0x33);
				client.Set<BYTE>(offset + 1, 0xC0);
				//xor eax, eax
				client.Set<BYTE>(offset + 2, 0x33);
				client.Set<BYTE>(offset + 3, 0xC0);
			}
		}
	}
}