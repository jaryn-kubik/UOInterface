void HookPackets();
void __stdcall SendPacket(byte *buffer);
void __stdcall RecvPacket(byte *buffer);

USHORT GetPacketLength(byte* buffer);