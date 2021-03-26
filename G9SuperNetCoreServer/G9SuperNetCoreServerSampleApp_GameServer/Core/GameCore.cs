using System.Collections.Generic;
using System.Linq;
using G9SuperNetCoreServer.Server.Socket;
using G9SuperNetCoreServerSampleApp_GameServer.AccountAndSession;
using G9SuperNetCoreServerSampleApp_GameServer.Commands;
using G9SuperNetCoreServerSampleApp_GameServer.Commands.Struct;

namespace G9SuperNetCoreServerSampleApp_GameServer.Core
{
    public static class GameCore
    {

        public static long PlayerCounter = 0;

        private static readonly SortedDictionary<long, GameAccount> GameAccountCollection
            = new SortedDictionary<long, GameAccount>();

        private static G9SuperNetCoreSocketServer<GameAccount, GameSession> _accessToServer;

        public static void Initialize(G9SuperNetCoreSocketServer<GameAccount, GameSession> server)
        {
            _accessToServer = server;
        }

        public static void AddNewGameAccount(GameAccount gameAccount)
        {
            lock (GameAccountCollection)
            {
                PlayerCounter++;
                gameAccount.PlayerIdentity = PlayerCounter;
                foreach (var account in GameAccountCollection)
                {
                    account.Value.Session.SendCommandAsync<CPlayerConnected, long>(PlayerCounter);
                }
                gameAccount.Session.SendCommandAsync<CGetPlayersPosition, G9DtSingTotalPlayers>(
                    new G9DtSingTotalPlayers()
                    {
                        YourIdentity = PlayerCounter,
                        PlayerData = GameAccountCollection.Select(s => new G9DtPlayerMove()
                        {
                            PlayerIdentity = s.Value.PlayerIdentity,
                            NewPosition = s.Value.LastPlayerPosition,
                            Dead = s.Value.Dead,
                            Kill = s.Value.Kill
                        }).ToArray()
                    });
                GameAccountCollection.Add(PlayerCounter, gameAccount);
            }
        }

        public static void RemoveGameAccount(GameAccount gameAccount)
        {
            GameAccountCollection.Remove(gameAccount.PlayerIdentity);
            foreach (var account in GameAccountCollection)
            {
                account.Value.Session.SendCommandAsync<CPlayerDisconnected, long>(gameAccount.PlayerIdentity);
            }
        }

        public static void MoveGameAccount(GameAccount gameAccount)
        {
            foreach (var account in GameAccountCollection.Where(s => s.Key != gameAccount.PlayerIdentity))
            {
                account.Value.Session.SendCommandAsync<CPlayerMove, G9DtPlayerMove>(new G9DtPlayerMove()
                    {NewPosition = gameAccount.LastPlayerPosition,
                        PlayerIdentity = gameAccount.PlayerIdentity,
                        Dead = gameAccount.Dead,
                        Kill = gameAccount.Kill
                    });
            }
        }

        public static void VoiceRecord(GameAccount gameAccount, float[] voiceData)
        {
            foreach (var account in GameAccountCollection.Where(s => s.Key != gameAccount.PlayerIdentity))
            {
                account.Value.Session.SendCommandAsync<CVoice, float[]>(voiceData);
            }
        }

        public static void Attack(GameAccount gameAccount, long attackIdentity)
        {
            gameAccount.Kill++;
            if (GameAccountCollection.ContainsKey(attackIdentity))
                GameAccountCollection[attackIdentity].Dead++;
            _accessToServer.SendCommandToAllAsync<CAttack, long>(attackIdentity);
        }

    }
}