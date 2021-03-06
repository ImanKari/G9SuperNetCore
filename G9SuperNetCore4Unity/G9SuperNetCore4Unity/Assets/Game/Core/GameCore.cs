using System.Collections.Generic;
using G9SuperNetCoreServerSampleApp_GameServer.Commands.Struct;
using UnityEngine;

public class GameCore : MonoBehaviour
{
    public static long PlayerCounter;

    public static readonly SortedDictionary<long, OtherCharacterHandler> GameAccountCollection
        = new SortedDictionary<long, OtherCharacterHandler>();

    public GameCharacterHandler MainCharacter;

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
            otherPlayer.Kill = playerData.Kill;
            otherPlayer.Dead = playerData.Dead;
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
        GameAccountCollection[dtPlayerMove.PlayerIdentity].Dead = dtPlayerMove.Dead;
        GameAccountCollection[dtPlayerMove.PlayerIdentity].Kill = dtPlayerMove.Kill;
    }

    public void PlayVoice(float[] voiceData)
    {
        var voiceAudioSource = gameObject.AddComponent<AudioSource>();
        var recordingNew = AudioClip.Create("", voiceData.Length, 1, 6999, false);
        recordingNew.SetData(voiceData, 0);
        voiceAudioSource.clip = recordingNew;
        voiceAudioSource.loop = false;
        voiceAudioSource.Play();
    }

    public void HandleAttack(long accountIdentity)
    {
        if (MainCharacter.AccessToGameAccount.PlayerIdentity == accountIdentity)
        {
            MainCharacter.AccessToGameAccount.Dead++;
            MainCharacter.ReceiveAttack();
        }
        else if (GameAccountCollection.ContainsKey(accountIdentity))
        {
            GameAccountCollection[accountIdentity].ReceiveAttack();
            GameAccountCollection[accountIdentity].Dead++;
        }
    }
}