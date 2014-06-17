bool FindCode(byte *signature, unsigned int len, byte **offset);
bool FindData(byte *signature, unsigned int len, byte **offset);
LPVOID CreateJMP(LPVOID source, LPVOID target, UINT len);
void AllowAccess(LPVOID lpAddress, SIZE_T dwSize);