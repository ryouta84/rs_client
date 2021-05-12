using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerManager;

public class RPC
{
    private NetworkClient networkClient;
    private PlayerManager playerManager;
    private PlayersController playersController;


    public RPC(NetworkClient network_client, PlayerManager player_manager, PlayersController players_controller)
    {
        networkClient = network_client;
        playerManager = player_manager;
        playersController = players_controller;
    }

    /*
     * 全参加プレイヤーが揃ったらゲーム開始
     */
    public void DoneReadyLocal(Payload payload)
    {
        if((Cmd) payload.command == Cmd.READY) {
            WhoAmIRemote();
        }
    }

    /**
     * プレイヤーIDを発行する
     */
    public void WhoAmIRemote()
    {
        var payload = new Payload();
        payload.command = (byte) Cmd.WHO_AM_I;
        networkClient.SendPayload(payload);
    }

    /**
     * 自分がどのプレイヤーをサーバーに割り当てられたか知る
     * IsNetworkPlayerがtrueなら他プレイヤー
    */
    public void WhoAmILocal(Payload payload)
    {
        // nameをもとにidを固定させておく
        foreach(var playable_player in playerManager.PlayableList) {
            switch(playable_player.Player.name) {
                case "Player1":
                    playable_player.PlayerId = 1;
                    break;
                case "Player2":
                    playable_player.PlayerId = 2;
                    break;
                case "Player3":
                    playable_player.PlayerId = 3;
                    break;
                default:
                    break;
            }
        }

        var name = "Player" + payload.playerId.ToString();
        foreach(var playable_player in playerManager.PlayableList) {
            if(playable_player.Player.name != name) {
                playable_player.IsNetworkPlayer = true;
            } else {
                playerManager.SelfPlayer = playable_player;
                playerManager.SelfPlayer.PlayerId = payload.playerId;
            }
        }


        // ゲーム開始
        playersController.PlayerStart();
        playerManager.State = GameState.playing;
    }

    public void Move(Payload payload)
    {
        playerManager.RecieveMovePosition(payload);
    }
}
