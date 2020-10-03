using G9SuperNetCoreServer.Abstarct;
using G9SuperNetCoreServer.Enums;
using G9SuperNetCoreServerSampleApp_GameServer.Commands.Struct;
using G9SuperNetCoreServerSampleApp_GameServer.Core;

namespace G9SuperNetCoreServerSampleApp_GameServer.AccountAndSession
{
    public class GameAccount : AServerAccount<GameSession>
    {
        public SimpleVector3 LastPlayerPosition;

        public long PlayerIdentity;

        public override void OnSessionClosed(DisconnectReason reason)
        {
            GameCore.RemoveGameAccount(this);
        }
    }
}