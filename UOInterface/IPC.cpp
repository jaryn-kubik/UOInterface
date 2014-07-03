#include "stdafx.h"
#include "UOInterface.h"
#include "PacketHooks.h"
#include "ImportHooks.h"
#include "Patches.h"
#include <mutex>
#include "Macros.h"

byte *CreateSharedMemory(LPCWSTR memoryName)
{
	DWORD pId = GetCurrentProcessId();
	WCHAR name[128];
	swprintf_s(name, memoryName, pId);

	HANDLE mapping = CreateFileMappingW(INVALID_HANDLE_VALUE, nullptr, PAGE_READWRITE, 0, BufferSize, name);
	if (!mapping)
		throw L"CreateFileMapping";

	byte *memory = (byte*)MapViewOfFile(mapping, FILE_MAP_ALL_ACCESS, 0, 0, 0);
	if (!memory)
		throw L"MapViewOfFile";
	return memory;
}

HWND hwnd;
byte *memoryIn, *memoryOut;
std::mutex mtx;
void InitIPC(HWND hWND)
{
	hwnd = hWND;
	memoryIn = CreateSharedMemory(BufferInName);
	memoryOut = CreateSharedMemory(BufferOutName);
}

BOOL SendIPCMessage(UOMessage msg, UINT data = 0)
{
	mtx.lock();
	BOOL result = SendMessage(hwnd, (int)msg, data, 0);
	mtx.unlock();
	return result;
}

BOOL SendIPCData(UOMessage msg, LPVOID data, UINT len)
{
	mtx.lock();
	memcpy(memoryOut, data, len);
	BOOL result = SendMessage(hwnd, (int)msg, len, 0);
	mtx.unlock();
	return result;
}

LRESULT RecvIPCMessage(UOMessage msg, WPARAM wParam, LPARAM lParam)
{
	switch (msg)
	{
	case UOMessage::PacketToClient:
		RecvPacket(memoryIn);
		break;
	case UOMessage::PacketToServer:
		SendPacket(memoryIn);
		break;
	case UOMessage::ConnectionInfo:
		SetConnectionInfo(wParam, LOWORD(lParam));
		break;
	case UOMessage::Pathfinding:
		Pathfind(HIWORD(wParam), LOWORD(wParam), LOWORD(lParam));
		break;
	case UOMessage::Patch:
		if (wParam)
			PatchEncryption();
		if (lParam)
			PatchMulti();
		break;
	default:
		break;
	}
	return 0;
}