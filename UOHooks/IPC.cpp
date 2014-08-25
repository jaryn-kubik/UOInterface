#include "stdafx.h"
#include "IPC.h"
#include "Client.h"
#include "PacketHooks.h"
#include "ImportHooks.h"
#include "OtherHooks.h"
#include "Patches.h"
#include <mutex>

namespace IPC
{
	struct SharedMemory
	{
		byte bufferIn[0x10000];
		byte bufferOut[0x10000];
		short packetTable[0x100];
		HWND hwnd;
	} *sharedMemory;

	HANDLE Init(HWND hwnd, HANDLE hProcess)
	{
		HANDLE mapping = CreateFileMappingW(INVALID_HANDLE_VALUE, nullptr, PAGE_READWRITE, 0, sizeof(SharedMemory), nullptr);
		if (!mapping)
			throw L"CreateFileMapping";

		sharedMemory = (SharedMemory*)MapViewOfFile(mapping, FILE_MAP_ALL_ACCESS, 0, 0, 0);
		if (!sharedMemory)
			throw L"MapViewOfFile";
		sharedMemory->hwnd = hwnd;

		if (!DuplicateHandle(GetCurrentProcess(), mapping, hProcess, &mapping, 0, FALSE, DUPLICATE_SAME_ACCESS))
			throw L"DuplicateHandle";
		return mapping;
	}

	HWND hwnd;
	DWORD WINAPI OnAttach(LPVOID mapping)
	{
		try
		{
			sharedMemory = (SharedMemory*)MapViewOfFile((HANDLE)mapping, FILE_MAP_ALL_ACCESS, 0, 0, 0);
			if (!sharedMemory)
				throw L"MapViewOfFile";
			hwnd = sharedMemory->hwnd;
			sharedMemory->hwnd = nullptr;

			Client::Init();
			Hooks::Imports();
			Hooks::Packets();
			Hooks::Other();
			Patches::Multi();
			Patches::Intro();
			return EXIT_SUCCESS;
		}
		catch (LPCWSTR str) { MessageBox(nullptr, str, L"Error: Init", MB_ICONERROR | MB_OK); }
		return EXIT_FAILURE;
	}

	std::mutex mtx;
	BOOL Send(UOMessage msg, UINT wParam, UINT lParam)
	{
		mtx.lock();
		LRESULT result = SendMessage(hwnd, msg, wParam, lParam);
		mtx.unlock();
		return result == 1;
	}

	BOOL SendData(UOMessage msg, LPVOID data, UINT len)
	{
		mtx.lock();
		memcpy(sharedMemory->bufferOut, data, len);
		LRESULT result = SendMessage(hwnd, msg, len, 0);
		if (result == 2)
			memcpy(data, sharedMemory->bufferOut, len);
		mtx.unlock();
		return result == 1;
	}

	void OnWindowCreated(HWND hwnd)
	{
		sharedMemory->hwnd = hwnd;
		if (Send(Ready))
			Patches::Encryption();
	}

	void OnMessage(UINT msg, WPARAM wParam, LPARAM lParam)
	{
		switch (msg)
		{
		case PacketToClient:
			Hooks::RecvPacket(sharedMemory->bufferIn);
			break;
		case PacketToServer:
			Hooks::SendPacket(sharedMemory->bufferIn);
			break;
		case ConnectionInfo:
			Hooks::SetConnectionInfo(wParam, LOWORD(lParam));
			break;
		case Pathfinding:
			Hooks::Pathfind(HIWORD(wParam), LOWORD(wParam), LOWORD(lParam));
			break;
		case GameSize:
			Hooks::SetGameSize(wParam, lParam);
			break;
		default:
			break;
		}
	}

	UOINTERFACE_API(byte*) GetInBuffer() { return sharedMemory->bufferOut; }
	UOINTERFACE_API(byte*) GetOutBuffer() { return sharedMemory->bufferIn; }
	UOINTERFACE_API(short*) GetPacketTable() { return sharedMemory->packetTable; }
	UOINTERFACE_API(void) SendUOMessage(UOMessage msg, int wParam, int lParam)
	{ SendMessage(sharedMemory->hwnd, (UINT)msg, wParam, lParam); }
}