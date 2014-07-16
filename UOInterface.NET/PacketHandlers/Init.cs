using System;
using UOInterface.Network;

namespace UOInterface
{
    public static partial class World
    {
        private static void InitHandlers()
        {
            //player
            Handlers.ToClient.Add(0x1B, OnLoginConfirm, Priority.High);
            Handlers.ToClient.Add(0x20, OnPlayerUpdate, Priority.High);
            Handlers.ToClient.Add(0x72, OnWarMode, Priority.High);

            //mobiles
            Handlers.ToClient.Add(0x77, OnMobileMoving, Priority.High);
            Handlers.ToClient.Add(0x78, OnMobileIncoming, Priority.High);
            Handlers.ToClient.Add(0x2D, OnMobileAttributes, Priority.High);
            Handlers.ToClient.Add(0xA1, OnMobileHits, Priority.High);
            Handlers.ToClient.Add(0xA2, OnMobileMana, Priority.High);
            Handlers.ToClient.Add(0xA3, OnMobileStamina, Priority.High);
            Handlers.ToClient.Add(0x11, OnMobileStatus, Priority.High);

            //movement
            Handlers.ToServer.Add(0x02, OnMovementRequest, Priority.High);
            Handlers.ToServer.Add(0x22, OnResyncRequest, Priority.High);
            Handlers.ToClient.Add(0x21, OnMovementRejected, Priority.High);
            Handlers.ToClient.Add(0x22, OnMovementAccepted, Priority.High);
            Handlers.ToClient.Add(0x97, OnMovementDemand, Priority.High);

            //speech
            Handlers.ToClient.Add(0x1C, OnAsciiMessage, Priority.High);
            Handlers.ToClient.Add(0xAE, OnUnicodeMessage, Priority.High);
            Handlers.ToClient.Add(0xC1, OnLocalizedMessage, Priority.High);

            //other
            Handlers.ToClient.Add(0x4E, OnInfravision, Priority.High);
            Handlers.ToClient.Add(0x4F, OnGlobalLight, Priority.High);
            Handlers.ToClient.Add(0x1D, OnRemoveObject, Priority.High);
            Handlers.ToClient.Add(0xBF, OnBigFuckingPacket, Priority.High);
        }

        private static readonly Lazy<bool> useNewMobileIncoming = new Lazy<bool>(() => Client.Version >= new Version(7, 0, 33));
        private static readonly Lazy<bool> usePostHSChanges = new Lazy<bool>(() => Client.Version >= new Version(7, 0, 9));
        private static readonly Lazy<bool> usePostSAChanges = new Lazy<bool>(() => Client.Version >= new Version(7, 0));
        private static readonly Lazy<bool> usePostKRPackets = new Lazy<bool>(() => Client.Version >= new Version(6, 0, 1, 7));
    }
}