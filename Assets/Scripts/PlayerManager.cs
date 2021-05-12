using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public int playerNum = 1;
    public int frameRate = 30;
    public InputField inputField;

    [SerializeField]
    private GameState state;
    private bool isInitialized = false;
    private PlayersController playersController;
    private List<PlayablePlayer> playableList;
    private NetworkClient networkClient;
    private RPC rpc;
    private PlayablePlayer selfPlayer;
    private string ipAdress = "";

    public List<PlayablePlayer> PlayableList { get => playableList; }
    public GameState State { get => state; set => state = value; }
    public PlayablePlayer SelfPlayer { get => selfPlayer; set => selfPlayer = value; }
    public bool IsInitialized { get => isInitialized; set => isInitialized = value; }
    public string IpAdress { get => ipAdress; set => ipAdress = value; }

    public enum GameState
    {
        initializing, // 開始準備中
        connectStart,
        connecting,
        connected, // サーバー接続済み
        playing, // ゲーム中
        end,
    }

    private void Awake()
    {
        Application.targetFrameRate = frameRate;
    }


    // Start is called before the first frame update
    void Start()
    {
        // 初期化開始状態にする
        state = GameState.initializing;

        playableList = new List<PlayablePlayer>();
        var player_list = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        foreach(var player in player_list) {
            var playable_player = new PlayablePlayer();
            playable_player.Player = player;
            playable_player.CharacterController = player.GetComponent<CharacterController>();
            // 移動先の位置の初期値を現在地
            playable_player.MovePosition = player.transform.position;
            playable_player.PlayerId = 0;
            PlayableList.Add(playable_player);
        }

        playersController = new PlayersController();
        playersController.Set(PlayableList);
    }

    // Update is called once per frame
    void Update()
    {
        var payload = new Payload();
        if(state >= GameState.connected) {
            payload = networkClient.Notify();
        }

        switch(state) {
            case GameState.initializing:
                if(isInitialized) {
                    state = GameState.connectStart;
                }
                return;
            case GameState.connectStart:
                networkClient = new NetworkClient(IpAdress);
                state = GameState.connecting;
                break;
            case GameState.connecting:
                if(networkClient.isConnected()) {
                    rpc = new RPC(networkClient, this, playersController);
                    state = GameState.connected;
                }
                break;
            case GameState.connected:
                break;
            case GameState.playing:
                var position = GetMyTargetPosition();
                if(!Vector3.Equals(position, Vector3.zero)) {
                    selfPlayer.MovePosition = position;
                }
                break;
            case GameState.end:
                break;
            default:
                break;
        }

        DoCommand(payload);
    }

    /**
     * 全プレイヤーの移動先を取得してセットする
     */
    public void RecieveMovePosition(Payload payload)
    {
        foreach(var playable_player in PlayableList) {
            if(playable_player.IsNetworkPlayer) {
                if((Cmd) payload.command != Cmd.MOVE) {
                    continue;
                }
                if(playable_player.PlayerId == payload.playerId) {
                    var positon = new Vector3();
                    positon.x = NetworkClient.ReverseByteFloat(payload.x);
                    positon.z = NetworkClient.ReverseByteFloat(payload.y);
                    playable_player.MovePosition = positon;
                }
            }
        }
    }

    /**
     * マウス右クリックの位置を取得する
     */
    private Vector3 GetMyTargetPosition()
    {
        if(Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("Floor"))) {
                var payload = new Payload();
                payload.playerId = selfPlayer.PlayerId;
                payload.command = (byte) Cmd.MOVE;
                payload.x = NetworkClient.ReverseByteFloat(hit.point.x);
                payload.y = NetworkClient.ReverseByteFloat(hit.point.z);
                networkClient.SendPayload(payload);
                return hit.point;
            }
        }

        return Vector3.zero;
    }


    private void DoCommand(Payload payload)
    {
        switch(payload.command) {
            case (byte) Cmd.READY:
                rpc.DoneReadyLocal(payload);
                break;
            case (byte) Cmd.WHO_AM_I:
                rpc.WhoAmILocal(payload);
                break;
            case (byte) Cmd.MOVE:
                rpc.Move(payload);
                break;
            default:
                playersController.MoveAll();
                break;
        }
    }

    /**
     * Connectボタン押下時
     */
    public void Connect()
    {
        IsInitialized = true;
        IpAdress = inputField.text;
    }

    void OnApplicationQuit()
    {
        networkClient?.Close();
    }

    public void OnQuitBtn()
    {
        Application.Quit();
    }
}
