#include "stdafx.h"
#include "IPC.h"
#include "UOInterface.h"
#include "PacketHooks.h"
#include "ImportHooks.h"
#include "Patches.h"
#include "Macros.h"
#include <mutex>

HWND hwnd;
SharedMemory *sharedMemory;
std::mutex mtx;
HANDLE InitIPC(HWND _hwnd)
{
	HANDLE mapping = CreateFileMappingW(INVALID_HANDLE_VALUE, nullptr, PAGE_READWRITE, 0, sizeof(SharedMemory), nullptr);
	if (!mapping)
		throw L"CreateFileMapping";

	sharedMemory = (SharedMemory*)MapViewOfFile(mapping, FILE_MAP_ALL_ACCESS, 0, 0, 0);
	if (!sharedMemory)
		throw L"MapViewOfFile";

	DWORD pId;
	GetWindowThreadProcessId(hwnd = _hwnd, &pId);
	HANDLE hProcess = OpenProcess(PROCESS_DUP_HANDLE, FALSE, pId);

	if (!DuplicateHandle(GetCurrentProcess(), mapping, hProcess, &mapping, 0, FALSE, DUPLICATE_SAME_ACCESS))
		throw L"DuplicateHandle";
	return mapping;
}

BOOL SendIPCMessage(UOMessage msg, UINT data)
{
	mtx.lock();
	LRESULT result = SendMessage(hwnd, (int)msg, data, 0);
	mtx.unlock();
	return result == 1;
}

BOOL SendIPCData(UOMessage msg, LPVOID data, UINT len)
{
	mtx.lock();
	memcpy(sharedMemory->bufferOut, data, len);
	LRESULT result = SendMessage(hwnd, (int)msg, len, 0);
	if (result == 2)
		memcpy(data, sharedMemory->bufferOut, len);
	mtx.unlock();
	return result == 1;
}

LRESULT RecvIPCMessage(UOMessage msg, WPARAM wParam, LPARAM lParam)
{
	switch (msg)
	{
	case UOMessage::PacketToClient:
		RecvPacket(sharedMemory->bufferIn);
		break;
	case UOMessage::PacketToServer:
		SendPacket(sharedMemory->bufferIn);
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