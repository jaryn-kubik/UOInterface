void InitIPC(HWND hwnd);
BOOL SendIPCMessage(UOMessage msg, UINT data = 0);
BOOL SendIPCData(UOMessage msg, LPVOID data, UINT len);
LRESULT RecvIPCMessage(UOMessage msg, WPARAM wParam, LPARAM lParam);