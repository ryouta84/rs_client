using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayersController
{
    // 全プレイヤーの接続が完了するまで移動できないようにするためのフラグ
    public bool isPlayerMoveStop = true;

    private List<PlayablePlayer> playableList;
    private float playerSpeed = 2.0f;

    public void Set(List<PlayablePlayer> playable_list)
    {
        playableList = playable_list;
    }

    public void PlayerStart()
    {
        isPlayerMoveStop = false;
    }

    public void MoveAll()
    {
        if(isPlayerMoveStop) {
            return;
        }

        foreach(var playable_player in playableList) {
            Move(playable_player.CharacterController, playable_player.Player.transform.position, playable_player.MovePosition);
        }
    }


    /**
	 * target近辺まで移動する
	 */
    private void Move(CharacterController characterController, Vector3 current_position, Vector3 target)
    {
        if(Vector3.Distance(current_position, target) > 0.1f) {
            var direction = (target - current_position).normalized;
            direction.y = 0;
            characterController.Move(direction * Time.deltaTime * playerSpeed);
        }
    }
}
