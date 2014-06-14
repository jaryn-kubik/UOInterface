#pragma once

#ifdef UOINTERFACE_EXPORTS
#define UOINTERFACE_API(ret) extern "C" __declspec(dllexport) ret _stdcall
#else
#define UOINTERFACE_API(ret) __declspec(dllexport) ret _stdcall
#endif

struct CallBacks
{
	void(__stdcall *OnExitProcess)();
	void(__stdcall *OnDisconnect)();

	void(__stdcall *OnWindowCreated)(HWND hwnd);
	void(__stdcall *OnFocus)(BOOL focus);
	void(__stdcall *OnVisibility)(BOOL visible);
	BOOL(__stdcall *OnKeyDown)(UINT key, BOOL prevState);

	BOOL(__stdcall *OnRecv)(byte *buffer, UINT len);
	BOOL(__stdcall *OnSend)(byte *buffer, UINT len);
};

extern CallBacks callBacks;

UOINTERFACE_API(void) SetCallbacks(CallBacks callbacks);
UOINTERFACE_API(void) Start(LPWSTR clientPath, LPWSTR assemblyPath, LPWSTR typeName,
	LPWSTR methodName, BOOL patchEncryption);

UOINTERFACE_API(void) SetConnectionInfo(UINT address, USHORT port);
UOINTERFACE_API(void) SendToServer(byte *buffer);
UOINTERFACE_API(void) SendToClient(byte *buffer);
UOINTERFACE_API(UINT) GetPacketLength(byte id);