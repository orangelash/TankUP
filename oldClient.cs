using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using System;

public class Client : MonoBehaviour
{
    public string clientName;
    private bool socketReady;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;

    public GameClient c;


    public bool isHost = false;

    public void Start()
    {
        DontDestroyOnLoad(gameObject);

    }

    public bool ConnectToServer(string host, int port)
    {
        if (socketReady)
            return false;
        try
        {
            socket = new TcpClient(host, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);

            socketReady = true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket error" + e.Message);
        }

        return socketReady;
    }

    public void Update()
    {
        if (socketReady)
        {
            if (stream.DataAvailable)
            {
                string data = reader.ReadLine();
                if (data != null)
                {
                    Debug.Log(data);
                    OnIncomingData(data);
                }
            }
        }
    }



    //Sending messages to the server
    public void Send(string data)
    {
        if (!socketReady)
            return;

        writer.WriteLine(data);
        writer.Flush();
    }

    //Read messages from the server
    private void OnIncomingData(string data)
    {
        string[] aData = data.Split('|');

        switch (aData[0])
        {
            case "SWHO":
                Debug.Log("SWHO|"+aData);
                for (int i = 1; i < aData.Length - 1; i++)
                {
                    UserConnected(aData[i], false);
                }
                Send("CWHO|" + clientName + "|" + ((isHost) ? 1 : 0).ToString());
                break;

            case "SCNN":
                UserConnected(aData[1], false);
                break;


            case "1":
                c.isPlayer1 = true;
                c.isHost = true;
                c.name = aData[1];
                Debug.Log("1-" + c.isPlayer1.ToString());
                GameManager.Instance.StartGame();
                Send(c.name + "has started playing as player 1");
                break;

            case "2":
                // Debug.Log("2-" + c.isPlayer1.ToString());
                c.name = aData[1];
                GameManager.Instance.StartGame();
                Send(c.name + "has started playing as player 2");
                break;

            case "ATTACK":
                Debug.Log("Activate attack button for player " + aData[1]);
                MainBoardManager.mbm.toggleButton();
                break;

            case "NEWBOARD&ATTACK":
                Debug.Log(aData[2] + " PLAYER WANTS TO REFRESH AND ATTACK");
                MainBoardManager.mbm.refresh(c.board);
                break;
        }
    }


    private void UserConnected(string name, bool host)
    {
        if(name==null || name == "")
            return;
            c = new GameClient();
            c.name = name;
            c.isHost = host;
            Debug.Log(c.name + "has been created");
    }

    private void OnApplicationQuit()
    {
        CloseSocket();
    }

    private void CloseSocket()
    {
        if (!socketReady)
            return;

        writer.Close();
        reader.Close();
        socket.Close();
        socketReady = false;
    }

}

public class GameClient1
{
    public string name;
    public bool isHost = false;
    public bool isPlayer1 = false;
    public string board;
}

