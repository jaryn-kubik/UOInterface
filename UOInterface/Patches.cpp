#include "stdafx.h"
#include "Utils.h"
#include "UOInterface.h"
#include "ImportHooks.h"

void SetByte(byte* address, byte data)
{
	AllowAccess(address, sizeof(byte));
	*address = data;
}

bool LoginEncryption()
{
	//1.x - 5.x
	unsigned char sig1[] = { 0x81, 0xF9, 0x00, 0x00, 0x01, 0x00, 0x0F, 0x8F };
	//6.x - 7.x
	unsigned char sig2[] = { 0x75, 0x12, 0x8B, 0x54, 0x24, 0x0C };

	bool result;
	byte *offset;
	if (result = FindCode(sig1, sizeof(sig1), &offset))
		SetByte(offset + 0x15, 0x84);
	else if (result = FindCode(sig2, sizeof(sig2), &offset))
		SetByte(offset, 0xEB);
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
	if (result = FindCode(sig1, sizeof(sig1), &offset))
		SetByte(offset + 8, 0x85);
	else if (result = FindCode(sig2, sizeof(sig2), &offset))
		SetByte(offset, 0xEB);
	else if (result = FindCode(sig3, sizeof(sig3), &offset))
		SetByte(offset + 0xC, 0x85);

	if (FindCode(sig4, sizeof(sig4), &offset))
	{
		SetByte(offset + 8, 0x3B);
		SetByte(offset + 0x12, 0x3B);
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
	if (result = FindCode(sig1, sizeof(sig1), &offset))
		SetByte(offset + 0xF, 0x3B);
	else if (result = FindCode(sig2, sizeof(sig2), &offset))
		SetByte(offset, 0xEB);
	else if (result = FindCode(sig3, sizeof(sig3), &offset))
		SetByte(offset + 0x1A, 0x3B);
	else if (result = FindCode(sig4, sizeof(sig4), &offset))
		SetByte(offset + 0xD, 0x3B);
	return result;
}

void PatchEncryption()
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

void PatchMulti()
{
	if (!HookImport("user32.dll", "FindWindowA", 0, Hook_FindWindowA))
		throw L"PatchMulti: FindWindowA";

	if (!HookImport("kernel32.dll", "CreateFileMappingA", 0, Hook_CreateFileMappingA))
		throw L"PatchMulti: CreateFileMappingA";

	if (!HookImport("kernel32.dll", "OpenMutexA", 0, Hook_OpenMutexA))
		throw L"PatchMulti: OpenMutexA";

	if (!HookImport("kernel32.dll", "CreateMutexA", 0, Hook_CreateMutexA))
		throw L"PatchMulti: CreateMutexA";

	sprintf_s(mutexName, "report_%d", GetCurrentProcessId());
}
//---------------------------------------------------------------------------//
//---------------------------------------------------------------------------//
//---------------------------------------------------------------------------//
void PatchIntro()
{
	byte intro[] = "intro.bik";
	byte osilogo[] = "osilogo.bik";
	byte splash[] = "Splash gump";

	byte *offset;
	if (FindData(intro, sizeof(intro), &offset))
		SetByte(offset, '_');

	if (FindData(osilogo, sizeof(osilogo), &offset))
		SetByte(offset, '_');

	if (FindData(splash, sizeof(splash), &offset))
	{
		//mov		dword ptr [esi+8], offset aSplashGump ; "Splash gump"
		byte sig[] = { 0xC7, 0x46, 0x08, 0x00, 0x00, 0x00, 0x00 };
		*(UINT*)(sig + 3) = (UINT)offset;

		if (FindCode(sig, sizeof(sig), &offset))
		{
			//xor eax, eax
			SetByte(offset + 0x30, 0x33);
			SetByte(offset + 0x30 + 1, 0xC0);
			//xor eax, eax
			SetByte(offset + 0x30 + 2, 0x33);
			SetByte(offset + 0x30 + 3, 0xC0);
		}
	}
}