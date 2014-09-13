#pragma once
#include "Client.h"

namespace Hooks
{
	void Pathfind(USHORT x, USHORT y, USHORT z);
	void SetGameSize(int width, int height);
	void Other(Client &client);
}