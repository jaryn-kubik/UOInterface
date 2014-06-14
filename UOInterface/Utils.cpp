#include "stdafx.h"
#include "Utils.h"

bool FindSignatureOffset(byte *signature, unsigned int len, byte **offset)
{
	for (*offset = (byte*)0x00400000; *offset < (byte*)(0x00600000 - len); (*offset)++)
		for (unsigned int i = 0; i < len; i++)
		{
			if (signature[i] != 0xCC && signature[i] != (*offset)[i])
				break;
			if (i == len - 1)
				return true;
		}
	return false;
}

LPVOID CreateJMP(LPVOID source, LPVOID target, UINT len)
{
	memset(source, 0x90, len);
	UINT offset = (UINT)target - (UINT)source - 5;
	byte *data = (byte*)source;
	data[0] = 0xE9;
	*(UINT*)&data[1] = offset;
	return data + len;
}