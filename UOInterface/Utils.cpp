#include "stdafx.h"
#include "Utils.h"

byte* codeBase = NULL;
byte* codeEnd = NULL;
byte* dataBase = NULL;
byte* dataEnd = NULL;

void Init()
{
	DWORD base = (DWORD)GetModuleHandle(NULL);
	PIMAGE_DOS_HEADER idh = (PIMAGE_DOS_HEADER)base;
	PIMAGE_NT_HEADERS inh = (PIMAGE_NT_HEADERS)(base + idh->e_lfanew);
	PIMAGE_OPTIONAL_HEADER ioh = &inh->OptionalHeader;

	codeBase = (byte*)(base + ioh->BaseOfCode);
	codeEnd = codeBase + ioh->SizeOfCode;
	dataBase = (byte*)(base + ioh->BaseOfData);
	dataEnd = dataBase + ioh->SizeOfInitializedData;
}

bool FindCode(byte *signature, unsigned int len, byte **offset)
{
	if (!codeBase || !codeEnd)
		Init();
	for (*offset = codeBase; *offset < codeEnd; (*offset)++)
	{
		for (unsigned int i = 0; i < len; i++)
		{
			if (signature[i] != 0xCC && signature[i] != (*offset)[i])
				break;
			if (i == len - 1)
				return true;
		}
	}
	return false;
}

bool FindData(byte *signature, unsigned int len, byte **offset)
{
	if (!dataBase || !dataEnd)
		Init();
	for (*offset = dataBase; *offset < dataEnd; (*offset)++)
	{
		for (unsigned int i = 0; i < len; i++)
		{
			if (signature[i] != 0xCC && signature[i] != (*offset)[i])
				break;
			if (i == len - 1)
				return true;
		}
	}
	return false;
}

LPVOID CreateJMP(LPVOID source, LPVOID target, UINT len)
{
	AllowAccess(source, len);
	memset(source, 0x90, len);
	UINT offset = (UINT)target - (UINT)source - 5;
	byte *data = (byte*)source;
	data[0] = 0xE9;
	*(UINT*)&data[1] = offset;
	return data + len;
}

void AllowAccess(LPVOID lpAddress, SIZE_T dwSize)
{
	DWORD oldProtect;
	VirtualProtect(lpAddress, dwSize, PAGE_EXECUTE_READWRITE, &oldProtect);
}