#include "stdafx.h"
#include "IPC.h"
#include "Client.h"
#include "PacketHooks.h"
#include "ImportHooks.h"
#include "OtherHooks.h"
#include "Patches.h"
#include <thread>
#include <queue>
#include <array>
#include <vector>

namespace IPC
{
	const int UOBUFFER_SIZE = 0x10000;
	struct SharedMemory
	{
		BYTE dataOut[UOBUFFER_SIZE];
		BYTE dataIn[UOBUFFER_SIZE];
		std::array<UINT, 4> msgOut;
		std::array<UINT, 4> msgIn;
	} *shared;

	HWND hwnd;
	std::queue<std::array<UINT, 4>> msgQueue;
	std::queue<std::vector<BYTE>> dataQueue;

	HANDLE sentOut, handledOut, sentIn, handledIn;
	void MessagePump()
	{
		WaitForSingleObject(sentIn, INFINITE);
		do
		{
			switch (shared->msgIn[0])
			{
			case PacketToClient:
			case PacketToServer:
				dataQueue.emplace(shared->dataIn, shared->dataIn + shared->msgIn[1]);
			case Pathfinding:
				msgQueue.push(shared->msgIn);
				break;
			case ConnectionInfo:
				Hooks::SetConnectionInfo(shared->msgIn[1], shared->msgIn[2]);
				break;
			case GameSize:
				Hooks::SetGameSize(shared->msgIn[1], shared->msgIn[2]);
				break;
			}
		} while (!SignalObjectAndWait(handledIn, sentIn, INFINITE, FALSE));
	}

	void Process()
	{
		while (!msgQueue.empty())
		{
			auto msg = msgQueue.front();
			switch (msg[0])
			{
			case PacketToClient:
				Hooks::RecvPacket(dataQueue.front().data());
				dataQueue.pop();
				break;
			case PacketToServer:
				Hooks::SendPacket(dataQueue.front().data());
				dataQueue.pop();
				break;
			case Pathfinding:
				Hooks::Pathfind(msg[1], msg[2], msg[3]);
				break;
			}
			msgQueue.pop();
		}
	}

	void CALLBACK OnExit(PVOID lpParameter, BOOLEAN TimerOrWaitFired)
	{
		CloseHandle(sentOut);
		CloseHandle(handledOut);
		CloseHandle(sentIn);
		CloseHandle(handledIn);
	}

	HANDLE Duplicate(HANDLE hProcess, HANDLE handle)
	{
		HANDLE newHandle;
		if (!DuplicateHandle(GetCurrentProcess(), handle, hProcess, &newHandle, 0, FALSE, DUPLICATE_SAME_ACCESS))
			throw L"DuplicateHandle";
		return newHandle;
	}

	extern "C" __declspec(dllexport) DWORD WINAPI OnAttach(LPDWORD args)
	{
		try
		{
			const DWORD access = PROCESS_DUP_HANDLE | PROCESS_CREATE_THREAD | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | SYNCHRONIZE;
			HANDLE hProcess = OpenProcess(access, FALSE, args[0]);
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

			if (!RegisterWaitForSingleObject(&mmf, hProcess, OnExit, nullptr, INFINITE, WT_EXECUTEONLYONCE))
				throw L"RegisterWaitForSingleObject";
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
			Client client;
			Hooks::Imports(client);
			Hooks::Packets(client);
			Hooks::Other(client);
			if (args[1])
				Patches::Encryption(client);
			Patches::Multi(client);
			Patches::Intro(client);
			memcpy(shared->dataOut, Hooks::GetPacketTable(), 0x100 * sizeof(UINT));

			std::thread(MessagePump).detach();//start ipc server
			return (DWORD)mmfRemote;
		}
		catch (LPCWSTR str) { MessageBox(nullptr, str, L"OnAttach", MB_ICONERROR | MB_OK); }
		return 0;
	}

	bool Send(UOMessage msg, UINT arg1, UINT arg2, UINT arg3)
	{
		shared->msgOut[0] = msg;
		shared->msgOut[1] = arg1;
		shared->msgOut[2] = arg2;
		shared->msgOut[3] = arg3;
		if (SignalObjectAndWait(sentOut, handledOut, INFINITE, FALSE))
			return false;
		return shared->msgOut[0] == 1;
	}

	bool SendData(UOMessage msg, LPVOID data, UINT len)
	{
		memcpy(shared->dataOut, data, len);
		shared->msgOut[0] = msg;
		shared->msgOut[1] = len;
		if (SignalObjectAndWait(sentOut, handledOut, INFINITE, FALSE))
			return false;
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