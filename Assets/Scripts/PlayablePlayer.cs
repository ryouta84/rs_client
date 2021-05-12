using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayablePlayer
{
    // ネットワーク越しに操作されるプレイヤーならtrue
    private bool isNetworkPlayer = false;
    private byte playerId;
    private GameObject player;
    private CharacterController characterController;
    // 移動先の位置
    private Vector3 movePosition;

    public bool IsNetworkPlayer { get => isNetworkPlayer; set => isNetworkPlayer = value; }
    public byte PlayerId { get => playerId; set => playerId = value; }
    public GameObject Player { get => player; set => player = value; }
    public CharacterController CharacterController { get => characterController; set => characterController = value; }
    public Vector3 MovePosition { get => movePosition; set => movePosition = value; }
}
