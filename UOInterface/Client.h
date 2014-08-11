#pragma once

namespace Client
{
	void Init();

	LPVOID Hook(LPVOID source, LPVOID target, UINT len);
	bool Hook(LPCSTR dll, LPCSTR function, LPVOID hook, DWORD ordinal = 0);

	bool Find(byte *signature, size_t len, byte **offset, byte *start, byte *end);
	bool FindCode(byte *signature, size_t len, byte **offset);
	bool FindData(byte *signature, size_t len, byte **offset);

	template<size_t len>
	bool FindCode(byte(&signature)[len], byte **offset)
	{ return FindCode(signature, len, offset); }

	template<size_t len>
	bool FindData(byte(&signature)[len], byte **offset)
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