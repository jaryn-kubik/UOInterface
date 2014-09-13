#include "stdafx.h"
#include "Client.h"

BYTE *codeBase, *codeEnd, *dataBase, *dataEnd;
DWORD base;
PIMAGE_IMPORT_DESCRIPTOR imports;

Client::Client()
{
	base = (DWORD)GetModuleHandle(nullptr);
	auto idh = (PIMAGE_DOS_HEADER)base;
	auto inh = (PIMAGE_NT_HEADERS)(base + idh->e_lfanew);
	auto ioh = &inh->OptionalHeader;
	imports = (PIMAGE_IMPORT_DESCRIPTOR)(base + ioh->DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT].VirtualAddress);

	codeBase = (BYTE*)(base + ioh->BaseOfCode);
	codeEnd = codeBase + ioh->SizeOfCode;
	dataBase = (BYTE*)(base + ioh->BaseOfData);
	dataEnd = dataBase + ioh->SizeOfInitializedData;

	ud ud_obj;
	ud_init(&ud_obj);
	ud_set_mode(&ud_obj, 32);
	ud_set_syntax(&ud_obj, nullptr);
	ud_set_input_buffer(&ud_obj, codeBase, codeEnd - codeBase);
	ud_set_pc(&ud_obj, (UINT)codeBase);

	while (ud_disassemble(&ud_obj))
	{
		if (ud_obj.mnemonic == UD_Iint3 || ud_obj.mnemonic == UD_Inop)//useless
			continue;

		ud_instr instr;
		instr.op = ud_obj.mnemonic | (ud_obj.inp_ctr << 16);
		for (int i = 0; i < 2; i++)
		{
			if ((instr.args[i].type = ud_obj.operand[i].type) == UD_NONE)
				break;

			if (instr.args[i].type == UD_OP_MEM)
				instr.args[i].size = ud_obj.operand[i].offset;
			else
				instr.args[i].size = ud_obj.operand[i].size;

			instr.args[i].base = ud_obj.operand[i].base;
			instr.args[i].lval = ud_obj.operand[i].lval.udword;
		}
		instr.offset = (LPVOID)ud_obj.insn_offset;
		data.push_back(instr);
	}
}

bool matches(ud_arg &input, ud_arg &pattern)
{
	if (pattern.type == UD_NONE)
		return true;
	if (pattern.type != input.type)
		return false;
	if (pattern.base != UD_NONE && pattern.base != input.base)
		return false;
	if (pattern.size > 0 && pattern.val() != input.val())
		return false;
	return true;
}

bool matches(ud_instr &input, ud_instr &pattern)
{
	return input.mnemonic() == pattern.mnemonic() &&
		matches(input.args[0], pattern.args[0]) &&
		matches(input.args[1], pattern.args[1]);
}

bool Client::Find(ud_instr instructions[], size_t len, int *index)
{
	int count = 0;
	size_t size = data.size() - len;
	for (size_t i = 0; i < size; i++)
	{
		for (size_t j = 0; j < len; j++)
		{
			if (!matches(data[i + j], instructions[j]))
				break;

			if (j == len - 1)
			{
				*index = i;
				count++;
				//return i;
			}
		}
	}
	if (count > 1)
		throw L"v pici to je";
	return count == 1;
	//return -1;
}

bool Client::Find(ud_instr instr, int *index, int count)
{
	if (count == 0)
	{
		*index = 0;
		count = data.size();
	}
	int delta = count > 0 ? 1 : -1;
	for (int end = *index + count; *index != end; (*index) += delta)
		if (matches(data[*index], instr))
			return true;
	return false;
}

bool Client::Find(BYTE *signature, size_t len, BYTE **offset)
{
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

bool Client::Find(LPVOID offset, int *index)
{
	for (*index = 0; *index < data.size(); (*index)++)
		if (data[*index].offset == offset)
			return true;
	return false;
}

LPVOID Client::Hook(LPVOID func, LPVOID hook)
{
	for (int i = 0; i < data.size(); i++)
	{
		if (data[i].mnemonic() == UD_Icall &&
			data[i].args[0].type == UD_OP_JIMM &&
			data[i].destination() == func)
		{
			BYTE* ptr = (BYTE*)data[i].offset;
			UINT offset = (UINT)hook - (UINT)ptr - 5;
			Set((UINT*)(ptr + 1), offset);
		}
	}
	return func;
}

LPVOID Client::Hook(int index, LPVOID hook, UINT count)
{
	int size = 0;
	for (int i = 0; i < count; i++)
		size += data[index + i].size();

	if (size < 5)
		throw L"Instruction size too small for hooking!";

	BYTE* ptr = (BYTE*)data[index].offset;
	DWORD oldProtect;
	VirtualProtect(ptr, size, PAGE_EXECUTE_READWRITE, &oldProtect);

	memset(ptr, 0x90, size);
	UINT offset = (UINT)hook - (UINT)ptr - 5;
	ptr[0] = 0xE9;
	*(UINT*)&ptr[1] = offset;

	VirtualProtect(ptr, size, oldProtect, &oldProtect);
	return data[index + count].offset;
}

bool Client::Hook(LPCSTR dll, LPCSTR function, LPVOID hook, DWORD ordinal)
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