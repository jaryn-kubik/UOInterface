#include "UOInterface.h"

struct SharedMemory
{
	byte bufferIn[0x40000];
	byte bufferOut[0x40000];

	short packetTable[0x100];
	HWND hwnd;
};
extern SharedMemory *sharedMemory;

HANDLE InitIPC(HWND hwnd);
BOOL SendIPCMessage(UOMessage msg, UINT data = 0);
BOOL SendIPCData(UOMessage msg, LPVOID data, UINT len);
LRESULT RecvIPCMessage(UOMessage msg, WPARAM wParam, LPARAM lParam);