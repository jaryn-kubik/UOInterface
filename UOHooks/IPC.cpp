#include "stdafx.h"
#include "IPC.h"
#include "Client.h"
#include "PacketHooks.h"
#include "ImportHooks.h"
#include "OtherHooks.h"
#include "Patches.h"
#include <thread>

//#define DEBUG_CONSOLE
#ifdef DEBUG_CONSOLE
#include <io.h>
#include <fcntl.h>
#endif

namespace IPC
{
	struct SharedMemory
	{
		byte dataOut[0x10000];
		byte dataIn[0x10000];
		UINT msgOut[4];
		UINT msgIn[4];
	} *shared;

	HANDLE sentOut, handledOut, sentIn, handledIn;
	void MessagePump()
	{
		WaitForSingleObject(sentIn, INFINITE);
		while (true)
		{
			switch (shared->msgIn[0])
			{
				/*case PacketToClient:
					Hooks::RecvPacket(shared->dataIn);
					break;
					case PacketToServer:
					Hooks::SendPacket(shared->dataIn);
					break;*/
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
			shared->msgIn[0] = 0;
			SignalObjectAndWait(handledIn, sentIn, INFINITE, FALSE);
		}
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

			//start ipc server
			/*std::thread t(MessagePump);
			t.detach();*/

			std::thread(MessagePump).detach();

			//init UOHooks
			Client::Init();
			Hooks::Imports();
			Hooks::Packets();
			Hooks::Other();
			Patches::Multi();
			Patches::Intro();
			memcpy(shared->dataOut, Hooks::GetPacketTable(), 0x100 * sizeof(UINT));
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

	void OnWindowCreated(HWND hwnd)
	{
		if (Send(Ready, (UINT)hwnd))
			Patches::Encryption();
	}
}