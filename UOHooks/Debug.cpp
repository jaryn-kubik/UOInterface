#include "stdafx.h"
#include "Debug.h"
#include <stdio.h>
#include <io.h>
#include <fcntl.h>
#include "libudis86/udis86.h"

Debug::Debug()
{
	QueryPerformanceFrequency(&freq);
	QueryPerformanceCounter(&start);
}

void Debug::Stop()
{
	QueryPerformanceCounter(&end);
	elapsed.QuadPart = end.QuadPart - start.QuadPart;
	elapsed.QuadPart *= 1000000;
	elapsed.QuadPart /= freq.QuadPart;
	printf("%lld\n", elapsed.QuadPart);
}

void Debug::ShowConsole()
{
	AllocConsole();
	HANDLE handle_out = GetStdHandle(STD_OUTPUT_HANDLE);
	int hCrt = _open_osfhandle((long)handle_out, _O_TEXT);
	FILE* hf_out = _fdopen(hCrt, "w");
	setvbuf(hf_out, nullptr, _IONBF, 1);
	*stdout = *hf_out;
}

void Debug::Disasm(UINT startAddress)
{
	MessageBoxA(nullptr, "Attach me please!", "Debugger", 0);

	ud ud_obj;
	ud_init(&ud_obj);
	ud_set_mode(&ud_obj, 32);
	ud_set_syntax(&ud_obj, nullptr);
	ud_set_input_buffer(&ud_obj, (BYTE*)startAddress, 0x00800000);
	ud_set_pc(&ud_obj, (UINT)startAddress);
	while (ud_disassemble(&ud_obj))
	{
		ud_mnemonic_code op = ud_insn_mnemonic(&ud_obj);
		const ud_operand_t *arg1 = ud_insn_opr(&ud_obj, 0);
		const ud_operand_t *arg2 = ud_insn_opr(&ud_obj, 1);
		const ud_operand_t *arg3 = ud_insn_opr(&ud_obj, 2);
		ud_insn_len(&ud_obj);
	}
}