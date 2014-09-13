#pragma once
#include "Client.h"

namespace Hooks
{
	void Imports(Client &client);
	void SetConnectionInfo(UINT address, USHORT port);
}