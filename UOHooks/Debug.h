#pragma once

class Debug
{
	LARGE_INTEGER start, end, elapsed, freq;

public:
	Debug();
	void Stop();

	static void ShowConsole();
	static void Disasm(UINT startAddress);
};