using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;



// サーバー側のpayload構造体と同じ構成
[StructLayout(LayoutKind.Sequential)]
public struct Payload
{
    public byte playerId;
    public byte command;
    public byte command_target;
    public byte padding;
    public int hp;
    public float x;
    public float y;
}

enum Cmd
{
    NOT_CMD,
    MOVE = 1,
    DEATH,
    READY = 90, // サーバーと同じ値にする
    WHO_AM_I,
    CLOSE,
}


public class NetworkClient
{
    private IPAddress serverIp;
    private int serverPort = 55550;

    private TcpClient tcpClient;
    private List<Payload> sendBuf;
    private List<Payload> fetchBuf;

    private readonly int PAYLOAD_SIZE = Marshal.SizeOf<Payload>();

    public NetworkClient(string ip_address, int port = 0)
    {
        if(port != 0) {
            serverPort = port;
        }

        tcpClient = new TcpClient();

        serverIp = IPAddress.Parse(ip_address);
        ConnectServer();
    }

    private bool ConnectServer()
    {
        try {
            // ブロッキング
            tcpClient.Connect(serverIp, serverPort);
        }
        catch(SocketException ex) {
            Debug.LogError(ex.SocketErrorCode.ToString() + ":" + ex.ErrorCode.ToString());
            return false;
        }


        if(tcpClient.Connected) {
            return true;
        } else {
            Debug.Log("not connected!");
            return false;
        }
    }

    public void Close()
    {
        tcpClient.Close();
    }


    public Payload Notify()
    {
        // データは固定長
        if(tcpClient.Available >= PAYLOAD_SIZE) {
            var stream = tcpClient.GetStream();
            if(stream.CanRead) {
                var bytes = new byte[PAYLOAD_SIZE];
                stream.Read(bytes, 0, PAYLOAD_SIZE);
                var payload = ConvertToPayload(bytes);
                return payload;
            }
        }

        return new Payload();
    }

    public void SendPayload(Payload payload)
    {
        var stream = tcpClient.GetStream();
        var bytes = ConvertToBytes(payload);
        stream.Write(bytes, 0, PAYLOAD_SIZE);
    }

    public bool isConnected()
    {
        return tcpClient.Connected;
    }

    private Payload ConvertToPayload(byte[] bytes)
    {
        if(bytes.Length == PAYLOAD_SIZE) {
            var payload = new Payload();
            IntPtr ptr = Marshal.AllocCoTaskMem(PAYLOAD_SIZE);
            Marshal.Copy(bytes, 0, ptr, PAYLOAD_SIZE);
            payload = (Payload) Marshal.PtrToStructure(ptr, typeof(Payload));
            Marshal.FreeCoTaskMem(ptr);
            return payload;
        }

        return new Payload();
    }

    public Byte[] ConvertToBytes(Payload payload)
    {
        byte[] bytes = new byte[PAYLOAD_SIZE];
        IntPtr ptr = Marshal.AllocCoTaskMem(PAYLOAD_SIZE);
        Marshal.StructureToPtr(payload, ptr, false);
        Marshal.Copy(ptr, bytes, 0, PAYLOAD_SIZE);
        Marshal.FreeCoTaskMem(ptr);

        return bytes;
    }

    public static float ReverseByteFloat(float value)
    {
        if(BitConverter.IsLittleEndian) {
            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return BitConverter.ToSingle(bytes, 0);
        } else {
            return value;
        }
    }
}
