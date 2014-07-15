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
            Communication.AddToClientHandler(0x2D, OnMobileAttributes, Priority.High);
            Communication.AddToClientHandler(0xA1, OnMobileHits, Priority.High);
            Communication.AddToClientHandler(0xA2, OnMobileMana, Priority.High);
            Communication.AddToClientHandler(0xA3, OnMobileStamina, Priority.High);
            Communication.AddToClientHandler(0x11, OnMobileStatus, Priority.High);

            //movement
            Communication.AddToServerHandler(0x02, OnMovementRequest, Priority.High);
            Communication.AddToServerHandler(0x22, OnResyncRequest, Priority.High);
            Communication.AddToClientHandler(0x21, OnMovementRejected, Priority.High);
            Communication.AddToClientHandler(0x22, OnMovementAccepted, Priority.High);
            Communication.AddToClientHandler(0x97, OnMovementDemand, Priority.High);

            //other
            Communication.AddToClientHandler(0x4E, OnInfravision, Priority.High);
            Communication.AddToClientHandler(0x4F, OnGlobalLight, Priority.High);
            Communication.AddToClientHandler(0x1D, OnRemoveObject, Priority.High);
            Communication.AddToClientHandler(0xBF, OnBigFuckingPacket, Priority.High);
        }

        private static readonly Lazy<bool> useNewMobileIncoming = new Lazy<bool>(() => Client.Version >= new Version(7, 0, 33));
        private static readonly Lazy<bool> usePostHSChanges = new Lazy<bool>(() => Client.Version >= new Version(7, 0, 9));
        private static readonly Lazy<bool> usePostSAChanges = new Lazy<bool>(() => Client.Version >= new Version(7, 0));
        private static readonly Lazy<bool> usePostKRPackets = new Lazy<bool>(() => Client.Version >= new Version(6, 0, 1, 7));
    }
}