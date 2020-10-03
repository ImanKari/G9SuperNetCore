﻿using System.Collections.Generic;
using System.Linq;
using G9SuperNetCoreServerSampleApp_GameServer.AccountAndSession;
using G9SuperNetCoreServerSampleApp_GameServer.Commands;
using G9SuperNetCoreServerSampleApp_GameServer.Commands.Struct;
using UnityEngine;

public class GameCore : MonoBehaviour
{
    public static long PlayerCounter;

    private static readonly SortedDictionary<long, OtherCharacterHandler> GameAccountCollection
        = new SortedDictionary<long, OtherCharacterHandler>();

    public OtherCharacterHandler PlayerObject;


    public void GenerateTotalPlayer(G9DtSingTotalPlayers playersInformation)
    {
        foreach (var playerData in playersInformation.PlayerData)
        {
            if (GameAccountCollection.ContainsKey(playerData.PlayerIdentity)) continue;
            var otherPlayer = Instantiate(PlayerObject,
                playerData.NewPosition.ConvertSimpleToMain(),
                PlayerObject.transform.rotation);
            otherPlayer.PlayerIdentity = playerData.PlayerIdentity;
            GameAccountCollection.Add(playerData.PlayerIdentity, otherPlayer);
        }
    }

    public void GenerateTotalPlayer(long playerIdentity)
    {
        if (GameAccountCollection.ContainsKey(playerIdentity)) return;
        var otherPlayer = Instantiate(PlayerObject);
        otherPlayer.PlayerIdentity = playerIdentity;
        GameAccountCollection.Add(playerIdentity, otherPlayer);
    }

    public void RemovePlayer(long playerIdentity)
    {
        if (!GameAccountCollection.ContainsKey(playerIdentity)) return;
        var player = GameAccountCollection[playerIdentity];
        GameAccountCollection.Remove(playerIdentity);
        Destroy(player.gameObject);
    }

    public void MovePlayer(G9DtPlayerMove dtPlayerMove)
    {
        if (!GameAccountCollection.ContainsKey(dtPlayerMove.PlayerIdentity)) return;
        GameAccountCollection[dtPlayerMove.PlayerIdentity]
            .MoveCharacter(dtPlayerMove.NewPosition.ConvertSimpleToMain());
    }

}