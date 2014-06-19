#pragma once

#ifdef UOINTERFACE_EXPORTS
#define UOINTERFACE_API(ret) extern "C" __declspec(dllexport) ret _stdcall
#else
#define UOINTERFACE_API(ret) __declspec(dllexport) ret _stdcall
#endif

enum struct Message : byte { ExitProcess, Disconnect, WindowCreated, Focus, Visibility};
struct CallBacks
{
	void(__stdcall *OnMessage)(Message msg, UINT param);
	BOOL(__stdcall *OnKeyDown)(UINT virtualKey);

	BOOL(__stdcall *OnRecv)(byte *buffer, UINT len);
	BOOL(__stdcall *OnSend)(byte *buffer, UINT len);
};

extern CallBacks callBacks;

UOINTERFACE_API(void) InstallHooks(CallBacks callbacks, BOOL patchEncryption);
UOINTERFACE_API(void) Start(LPWSTR client, LPWSTR assembly, LPWSTR type, LPWSTR method, LPWSTR args);

UOINTERFACE_API(void) SetConnectionInfo(UINT address, USHORT port);
UOINTERFACE_API(void) SendToServer(byte *buffer);
UOINTERFACE_API(void) SendToClient(byte *buffer);
UOINTERFACE_API(UINT) GetPacketLength(byte id);