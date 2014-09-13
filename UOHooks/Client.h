#pragma once

#include "libudis86/udis86.h"
#include <vector>

struct ud_arg
{
	BYTE type, base;
	USHORT size;
	UINT lval;

	UINT val() { return lval & (UINT64_MAX >> (64 - size)); };
	static ud_arg reg(ud_type reg = UD_NONE) { return ud_arg{ UD_OP_REG, reg }; }
	static ud_arg mem(ud_type reg = UD_NONE) { return ud_arg{ UD_OP_MEM, reg }; }
	static ud_arg mem(ud_type reg, UINT val) { return ud_arg{ UD_OP_MEM, reg, 32, val }; }
	static ud_arg imm() { return ud_arg{ UD_OP_IMM }; }
	static ud_arg imm(UINT val) { return ud_arg{ UD_OP_IMM, UD_NONE, 32, val }; }
	static ud_arg jimm() { return ud_arg{ UD_OP_JIMM }; }
	static ud_arg jimm(UINT val) { return ud_arg{ UD_OP_JIMM, UD_NONE, 32, val }; }
	static ud_arg cons() { return ud_arg{ UD_OP_CONST }; }
	static ud_arg cons(UINT val) { return ud_arg{ UD_OP_CONST, UD_NONE, 32, val }; }
};

struct ud_instr
{
	UINT op;
	ud_arg args[2];
	LPVOID offset;

	USHORT mnemonic() { return LOWORD(op); }
	USHORT size() { return HIWORD(op); }
	LPVOID destination() { return (BYTE*)offset + size() + args[0].val(); }
};

class Client
{
	BYTE *codeBase, *codeEnd, *dataBase, *dataEnd;
	DWORD base;
	PIMAGE_IMPORT_DESCRIPTOR imports;
	std::vector<ud_instr> data;

	bool Find(ud_instr instructions[], size_t len, int *index);
	bool Find(BYTE *signature, size_t len, BYTE **offset);

public:
	Client();
	ud_instr& operator[](int index) { return data[index]; }

	bool Find(LPVOID offset, int *index);
	bool Find(ud_instr instr, int *index, int count = 0);

	bool Find(ud_mnemonic_code op, int *index, int count = 0)
	{ return Find({ op, { ud_arg::jimm() } }, index, count); }
	bool FindAndFollow(ud_mnemonic_code op, int *index, int count = 0)
	{ return Find(op, index, count) && Find(data[*index].destination(), index); }

	template<size_t len> bool Find(ud_instr(&signature)[len], int *index) { return Find(signature, len, index); }
	template<size_t len> bool Find(BYTE(&signature)[len], BYTE **offset) { return Find(signature, len, offset); }

	LPVOID Hook(LPVOID func, LPVOID hook);
	LPVOID Hook(int index, LPVOID hook, UINT count = 1);
	bool Hook(LPCSTR dll, LPCSTR function, LPVOID hook, DWORD ordinal = 0);

	template<typename Type>
	void Set(Type *address, Type data)
	{
		DWORD oldProtect;
		VirtualProtect(address, sizeof(Type), PAGE_EXECUTE_READWRITE, &oldProtect);
		*address = data;
		VirtualProtect(address, sizeof(Type), oldProtect, &oldProtect);
	}
};