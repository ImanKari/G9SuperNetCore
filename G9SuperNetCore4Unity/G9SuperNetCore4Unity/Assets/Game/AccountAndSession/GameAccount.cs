using G9SuperNetCoreClient.Abstract;
using G9SuperNetCoreClient.Enums;
using G9SuperNetCoreServerSampleApp_GameServer.Commands.Struct;

namespace G9SuperNetCoreServerSampleApp_GameServer.AccountAndSession
{
    public class GameAccount : AClientAccount<GameSession>
    {
        public SimpleVector3 LastPlayerPosition;

        public long PlayerIdentity;

        public GameCore AccessToGameCore;

        public int Kill;

        public int Dead;

        public override void OnSessionClosed(DisconnectReason reason)
        {
        }
    }
}