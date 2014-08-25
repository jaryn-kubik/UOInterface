#include "stdafx.h"
#include "Patches.h"
#include "Client.h"

namespace Patches
{
	bool LoginEncryption()
	{
		//1.x - 5.x
		unsigned char sig1[] = { 0x81, 0xF9, 0x00, 0x00, 0x01, 0x00, 0x0F, 0x8F };
		//6.x - 7.x
		unsigned char sig2[] = { 0x75, 0x12, 0x8B, 0x54, 0x24, 0x0C };

		bool result;
		byte *offset;
		if (result = Client::FindCode(sig1, &offset))
			Client::Set<byte>(offset + 0x15, 0x84);
		else if (result = Client::FindCode(sig2, &offset))
			Client::Set<byte>(offset, 0xEB);
		return result;
	}

	bool TwoFishEncryption()
	{
		//3.x - 5.x
		unsigned char sig1[] = { 0x8B, 0xD9, 0x8B, 0xC8, 0x48, 0x85, 0xC9, 0x0F, 0x84 };
		//6.x - 7.x
		unsigned char sig2[] = { 0x74, 0x0F, 0x83, 0xB9, 0xB4, 0x00, 0x00, 0x00, 0x00 };
		//3.x - 4.x
		unsigned char sig3[] = { 0x81, 0xEC, 0x04, 0x01, 0x00, 0x00, 0x85, 0xC0, 0x53, 0x8B, 0xD9, 0x0F, 0x84 };
		//1.x - 3.x
		unsigned char sig4[] = { 0x5E, 0xC3, 0x8B, 0x86, 0xCC, 0xCC, 0xCC, 0xCC, 0x85, 0xC0, 0x74 };

		bool result;
		byte *offset;
		if (result = Client::FindCode(sig1, &offset))
			Client::Set<byte>(offset + 8, 0x85);
		else if (result = Client::FindCode(sig2, &offset))
			Client::Set<byte>(offset, 0xEB);
		else if (result = Client::FindCode(sig3, &offset))
			Client::Set<byte>(offset + 0xC, 0x85);

		if (Client::FindCode(sig4, &offset))
		{
			Client::Set<byte>(offset + 8, 0x3B);
			Client::Set<byte>(offset + 0x12, 0x3B);
			result = true;
		}
		return result;
	}

	bool ProtocolDecryption()
	{
		//3.x - 5.x
		byte sig1[] = { 0x83, 0xFF, 0xFF, 0x0F, 0x84, 0xCC, 0xCC, 0xCC, 0xCC, 0x8B, 0x86, 0xCC, 0xCC, 0xCC, 0xCC, 0x85, 0xC0 };
		//6.x - 7.x
		byte sig2[] = { 0x74, 0x37, 0x83, 0xBE, 0xB4, 0x00, 0x00, 0x00, 0x00 };
		//3.x
		byte sig3[] = { 0x33, 0xFF, 0x3B, 0xFD, 0x0F, 0x85 };
		//1.x - 4.x
		byte sig4[] = { 0x8B, 0xF8, 0x83, 0xFF, 0xFF, 0x74, 0xCC, 0x8B, 0x86 };

		bool result;
		byte *offset;
		if (result = Client::FindCode(sig1, &offset))
			Client::Set<byte>(offset + 0xF, 0x3B);
		else if (result = Client::FindCode(sig2, &offset))
			Client::Set<byte>(offset, 0xEB);
		else if (result = Client::FindCode(sig3, &offset))
			Client::Set<byte>(offset + 0x1A, 0x3B);
		else if (result = Client::FindCode(sig4, &offset))
			Client::Set<byte>(offset + 0xD, 0x3B);
		return result;
	}

	void Encryption()
	{
		if (!LoginEncryption())
			throw L"PatchEncryption: LoginEncryption";
		if (!TwoFishEncryption())
			throw L"PatchEncryption: TwoFishEncryption";
		if (!ProtocolDecryption())
			throw L"PatchEncryption: ProtocolDecryption";
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

	void Multi()
	{
		if (!Client::Hook("user32.dll", "FindWindowA", Hook_FindWindowA))
			throw L"PatchMulti: FindWindowA";

		if (!Client::Hook("kernel32.dll", "CreateFileMappingA", Hook_CreateFileMappingA))
			throw L"PatchMulti: CreateFileMappingA";

		if (!Client::Hook("kernel32.dll", "OpenMutexA", Hook_OpenMutexA))
			throw L"PatchMulti: OpenMutexA";

		if (!Client::Hook("kernel32.dll", "CreateMutexA", Hook_CreateMutexA))
			throw L"PatchMulti: CreateMutexA";

		sprintf_s(mutexName, "report_%d", GetCurrentProcessId());
	}
	//---------------------------------------------------------------------------//
	//---------------------------------------------------------------------------//
	//---------------------------------------------------------------------------//
	void Intro()
	{
		byte intro[10] = "intro.bik";
		byte osilogo[12] = "osilogo.bik";
		byte splash[12] = "Splash gump";

		byte *offset;
		if (Client::FindData(intro, &offset))
			Client::Set<byte>(offset, '_');

		if (Client::FindData(osilogo, &offset))
			Client::Set<byte>(offset, '_');

		if (Client::FindData(splash, &offset))
		{
			//mov		dword ptr [esi+8], offset aSplashGump ; "Splash gump"
			byte sig[] = { 0xC7, 0x46, 0x08, 0x00, 0x00, 0x00, 0x00 };
			*(UINT*)(sig + 3) = (UINT)offset;

			if (Client::FindCode(sig, &offset))
			{
				//xor eax, eax
				Client::Set<byte>(offset + 0x30, 0x33);
				Client::Set<byte>(offset + 0x30 + 1, 0xC0);
				//xor eax, eax
				Client::Set<byte>(offset + 0x30 + 2, 0x33);
				Client::Set<byte>(offset + 0x30 + 3, 0xC0);
			}
		}
	}
}