using System;
using UOInterface.Network;

namespace UOInterface
{
    public static partial class World
    {
        private static void InitHandlers()
        {
            //player
            Communication.AddToClientHandler(0x1B, OnLoginConfirm, Priority.High);
            Communication.AddToClientHandler(0x20, OnPlayerUpdate, Priority.High);
            Communication.AddToClientHandler(0x72, OnWarMode, Priority.High);

            //mobiles
            Communication.AddToClientHandler(0x77, OnMobileMoving, Priority.High);
            Communication.AddToClientHandler(0x78, OnMobileIncoming, Priority.High);

            //movement
            Communication.AddToServerHandler(0x02, OnMovementRequest, Priority.High);
            Communication.AddToServerHandler(0x22, OnResyncRequest, Priority.High);
            Communication.AddToClientHandler(0x21, OnMovementRejected, Priority.High);
            Communication.AddToClientHandler(0x22, OnMovementAccepted, Priority.High);
            Communication.AddToClientHandler(0x97, OnMovementDemand, Priority.High);
        }

        private static readonly bool useNewMobileIncoming = Client.Version >= new Version(7, 0, 33);
        private static readonly bool usePostHSChanges = Client.Version >= new Version(7, 0, 9);
        private static readonly bool usePostSAChanges = Client.Version >= new Version(7, 0);
        private static readonly bool usePostKRPackets = Client.Version >= new Version(6, 0, 1, 7);
    }
}