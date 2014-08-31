#pragma once

namespace Client
{
	void Init();

	LPVOID Hook(LPVOID source, LPVOID target, UINT len);
	bool Hook(LPCSTR dll, LPCSTR function, LPVOID hook, DWORD ordinal = 0);

	bool Find(BYTE *signature, size_t len, BYTE **offset, BYTE *start, BYTE *end);
	bool FindCode(BYTE *signature, size_t len, BYTE **offset);
	bool FindData(BYTE *signature, size_t len, BYTE **offset);

	template<size_t len>
	bool FindCode(BYTE(&signature)[len], BYTE **offset)
	{ return FindCode(signature, len, offset); }

	template<size_t len>
	bool FindData(BYTE(&signature)[len], BYTE **offset)
	{ return FindData(signature, len, offset); }

	template<typename Type>
	void Set(Type *address, Type data)
	{
		DWORD oldProtect;
		VirtualProtect(address, sizeof(Type), PAGE_EXECUTE_READWRITE, &oldProtect);
		*address = data;
		VirtualProtect(address, sizeof(Type), oldProtect, &oldProtect);
	}
}