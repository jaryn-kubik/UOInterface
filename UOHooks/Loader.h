#pragma once

namespace Loader
{
	typedef UINT(*OnUOMessage)(UINT msg, int wParam, int lParam);

	UOINTERFACE_API(DWORD) Start(LPWSTR client, OnUOMessage onMessage);
	UOINTERFACE_API(void) Inject(DWORD pid, OnUOMessage onMessage);
}