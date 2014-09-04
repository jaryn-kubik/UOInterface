#pragma once

#include <SDKDDKVer.h>

#define WIN32_LEAN_AND_MEAN// Exclude rarely-used stuff from Windows headers
#include <windows.h>

#pragma comment (lib, "Ws2_32.lib")
#pragma comment (lib, "Shlwapi.lib")

#define BEA_ENGINE_STATIC
#define BEA_USE_STDCALL