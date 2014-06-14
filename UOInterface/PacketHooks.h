void HookPackets();
UINT GetPacketLength(byte *buffer);
void __stdcall SendPacket(byte *buffer);
void __stdcall RecvPacket(byte *buffer);