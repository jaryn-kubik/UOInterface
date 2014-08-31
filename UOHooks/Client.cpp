#include "stdafx.h"
#include "Client.h"

namespace Client
{
	BYTE *codeBase, *codeEnd, *dataBase, *dataEnd;
	DWORD base;
	PIMAGE_OPTIONAL_HEADER optionalHeader;
	PIMAGE_IMPORT_DESCRIPTOR imports;

	void Init()
	{
		base = (DWORD)GetModuleHandle(nullptr);
		auto idh = (PIMAGE_DOS_HEADER)base;
		auto inh = (PIMAGE_NT_HEADERS)(base + idh->e_lfanew);
		optionalHeader = &inh->OptionalHeader;
		imports = (PIMAGE_IMPORT_DESCRIPTOR)(base + optionalHeader->DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT].VirtualAddress);

		codeBase = (BYTE*)(base + optionalHeader->BaseOfCode);
		codeEnd = codeBase + optionalHeader->SizeOfCode;
		dataBase = (BYTE*)(base + optionalHeader->BaseOfData);
		dataEnd = dataBase + optionalHeader->SizeOfInitializedData;
	}

	bool FindCode(BYTE *signature, size_t len, BYTE **offset) { return Find(signature, len, offset, codeBase, codeEnd); }
	bool FindData(BYTE *signature, size_t len, BYTE **offset) { return Find(signature, len, offset, dataBase, dataEnd); }
	bool Find(BYTE *signature, size_t len, BYTE **offset, BYTE *start, BYTE *end)
	{
		for (*offset = start; *offset < end; (*offset)++)
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

	LPVOID Hook(LPVOID source, LPVOID target, UINT len)
	{
		DWORD oldProtect;
		VirtualProtect(source, len, PAGE_EXECUTE_READWRITE, &oldProtect);

		memset(source, 0x90, len);
		UINT offset = (UINT)target - (UINT)source - 5;
		BYTE *data = (BYTE*)source;
		data[0] = 0xE9;
		*(UINT*)&data[1] = offset;

		VirtualProtect(source, len, oldProtect, &oldProtect);
		return data + len;
	}

	bool Hook(LPCSTR dll, LPCSTR function, LPVOID hook, DWORD ordinal)
	{
		for (auto iid = imports; iid->Name; iid++)
		{
			if (_stricmp(dll, (LPSTR)(base + iid->Name)) != 0)
				continue;

			auto nTable = (PIMAGE_THUNK_DATA)(base + iid->OriginalFirstThunk);
			auto aTable = (PIMAGE_THUNK_DATA)(base + iid->FirstThunk);

			for (; nTable->u1.AddressOfData; nTable++, aTable++)
			{
				if (!(nTable->u1.Ordinal & 0x80000000))
				{
					auto name = (PIMAGE_IMPORT_BY_NAME)(base + nTable->u1.AddressOfData);
					if ((function && strcmp(name->Name, function) == 0) || ordinal == name->Hint)
					{
						Set(&aTable->u1.Function, (DWORD)hook);
						return true;
					}
				}
				else if (ordinal == (nTable->u1.Ordinal & 0xffff))
				{
					Set(&aTable->u1.Function, (DWORD)hook);
					return true;
				}
			}
		}
		return false;
	}
}