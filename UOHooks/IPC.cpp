#include "stdafx.h"
#include "IPC.h"
#include "Client.h"
#include "PacketHooks.h"
#include "ImportHooks.h"
#include "OtherHooks.h"
#include "Patches.h"
#include <thread>
#include <atomic>

#define DEBUG_CONSOLE
#ifdef DEBUG_CONSOLE
#include <io.h>
#include <fcntl.h>
#endif
#include "MainHook.h"

namespace IPC
{
	const int UOBUFFER_SIZE = 0x80000;
	struct SharedMemory
	{
		BYTE dataOut[UOBUFFER_SIZE];
		BYTE dataIn[UOBUFFER_SIZE];
		UINT msgOut[4];
		UINT msgIn[4];
	} *shared;

	HWND hwnd;
	UINT nextOut, nextIn;
	std::atomic_flag lock = ATOMIC_FLAG_INIT;
	void QueuePacket()
	{
		while (lock.test_and_set(std::memory_order_acquire));

		if (shared->msgIn[2] == nextIn)
			nextIn += shared->msgIn[1];
		shared->msgIn[0] = nextIn;

		lock.clear(std::memory_order_release);
		PostMessage(hwnd, WM_USER, shared->msgIn[0], shared->msgIn[2]);
	}

	HANDLE sentOut, handledOut, sentIn, handledIn;
	void MessagePump()
	{
		WaitForSingleObject(sentIn, INFINITE);
		while (true)
		{
			switch (shared->msgIn[0])
			{
			case PacketToClient:
			case PacketToServer:
				QueuePacket();
				break;
			case ConnectionInfo:
				Hooks::SetConnectionInfo(shared->msgIn[1], shared->msgIn[2]);
				if (shared->msgIn[3])
					Patches::Encryption();
				break;
			case Pathfinding:
				Hooks::Pathfind(shared->msgIn[1], shared->msgIn[2], shared->msgIn[3]);
				break;
			case GameSize:
				Hooks::SetGameSize(shared->msgIn[1], shared->msgIn[2]);
				break;
			}
			SignalObjectAndWait(handledIn, sentIn, INFINITE, FALSE);
		}
	}

	void Process(WPARAM msg, LPARAM offset)
	{
		switch (msg)
		{
		case PacketToClient:
			Hooks::RecvPacket(shared->dataIn + offset);
			break;
		case PacketToServer:
			Hooks::SendPacket(shared->dataIn + offset);
			break;
		}

		while (lock.test_and_set(std::memory_order_acquire));
		if (nextIn > offset && offset > UOBUFFER_SIZE / 2)
			nextIn = 0;
		lock.clear(std::memory_order_release);
	}

	HANDLE Duplicate(HANDLE hProcess, HANDLE handle)
	{
		HANDLE newHandle;
		if (!DuplicateHandle(GetCurrentProcess(), handle, hProcess, &newHandle, 0, FALSE, DUPLICATE_SAME_ACCESS))
			throw L"DuplicateHandle";
		return newHandle;
	}

	extern "C" __declspec(dllexport) DWORD WINAPI OnAttach(LPVOID pId)
	{
#ifdef DEBUG_CONSOLE
		AllocConsole();
		HANDLE handle_out = GetStdHandle(STD_OUTPUT_HANDLE);
		int hCrt = _open_osfhandle((long)handle_out, _O_TEXT);
		FILE* hf_out = _fdopen(hCrt, "w");
		setvbuf(hf_out, nullptr, _IONBF, 1);
		*stdout = *hf_out;
#endif
		try
		{
			Hooks::pica();
			HANDLE hProcess = OpenProcess(PROCESS_DUP_HANDLE | PROCESS_CREATE_THREAD | PROCESS_VM_OPERATION | PROCESS_VM_WRITE, FALSE, (DWORD)pId);
			if (!hProcess)
				throw L"OpenProcess";

			//init shared memory
			HANDLE mmf = CreateFileMappingW(INVALID_HANDLE_VALUE, nullptr, PAGE_READWRITE, 0, sizeof(SharedMemory), nullptr);
			if (!mmf)
				throw L"CreateFileMapping";
			shared = (SharedMemory*)MapViewOfFile(mmf, FILE_MAP_ALL_ACCESS, 0, 0, 0);
			if (!shared)
				throw L"MapViewOfFile";
			HANDLE mmfRemote = Duplicate(hProcess, mmf);
			CloseHandle(mmf);

			//init events
			sentOut = CreateEvent(nullptr, FALSE, FALSE, nullptr);
			handledOut = CreateEvent(nullptr, FALSE, FALSE, nullptr);
			sentIn = CreateEvent(nullptr, FALSE, FALSE, nullptr);
			handledIn = CreateEvent(nullptr, FALSE, FALSE, nullptr);
			if (!sentOut || !handledOut || !sentIn || !handledIn)
				throw L"CreateEvent";
			shared->msgOut[0] = (UINT)Duplicate(hProcess, sentOut);
			shared->msgOut[1] = (UINT)Duplicate(hProcess, handledOut);
			shared->msgOut[2] = (UINT)Duplicate(hProcess, sentIn);
			shared->msgOut[3] = (UINT)Duplicate(hProcess, handledIn);
			CloseHandle(hProcess);

			//init UOHooks
			Client::Init();
			Hooks::Imports();
			Hooks::Packets();
			Hooks::Other();
			Patches::Multi();
			Patches::Intro();
			memcpy(shared->dataOut, Hooks::GetPacketTable(), 0x100 * sizeof(UINT));

			std::thread(MessagePump).detach();//start ipc server
			return (DWORD)mmfRemote;
		}
		catch (LPCWSTR str) { MessageBox(nullptr, str, L"OnAttach", MB_ICONERROR | MB_OK); }
		return 0;
	}

	BOOL Send(UOMessage msg, UINT arg1, UINT arg2, UINT arg3)
	{
		shared->msgOut[0] = msg;
		shared->msgOut[1] = arg1;
		shared->msgOut[2] = arg2;
		shared->msgOut[3] = arg3;
		SignalObjectAndWait(sentOut, handledOut, INFINITE, FALSE);
		return shared->msgOut[0] == 1;
	}

	BOOL SendData(UOMessage msg, LPVOID data, UINT len)
	{
		memcpy(shared->dataOut, data, len);
		shared->msgOut[0] = msg;
		shared->msgOut[1] = len;
		SignalObjectAndWait(sentOut, handledOut, INFINITE, FALSE);
		if (shared->msgOut[0] == 2)
			memcpy(data, shared->dataOut, len);
		return shared->msgOut[0] == 1;
	}

	void SetHWND(HWND handle)
	{
		hwnd = handle;
		Send(Ready, (UINT)hwnd);
	}
}