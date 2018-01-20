using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

public class Client2 : MonoBehaviour
{
	public bool isHost;
    public string clientName;
    private bool socketReady;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;

    private List<GameClient> players = new List<GameClient>();

    private void Update()
    {
        if (socketReady)
        {
            if (stream.DataAvailable)
            {
                string data = reader.ReadLine();
                if (data != null)
                    OnIncomingData(data);
            }
        }
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
        catch (System.Exception e)
        {
            Debug.Log("Socket error " + e.Message);
        }
        return socketReady;

    }

    private void OnIncomingData(string data)
    {
        Debug.Log(data);
        string[] aData = data.Split('|');

        switch (aData[0])
        {
            case "SWHO":
                for (int i = 1; 1 < aData.Length - 1; i++)
                {
                    UserConnected(aData[i], false);
                }
                Send("CWHO|" + clientName+"|"+((isHost)?1:0));
                break;
			case"SCNN":
			UserConnected(aData[1],false);
			break;
        }
    }

    private void UserConnected(string name, bool host)
    {
        GameClient c = new GameClient();
        c.name = name;
        c.isHost = host;
        players.Add(c);


    }

    public void Send(string data)
    {
        if (!socketReady)
            return;
        writer.WriteLine(data);
        writer.Flush();
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
    private void OnApplicationQuit()
    {
        CloseSocket();
    }
    private void OnDisable()
    {
        CloseSocket();
    }
}

public class GameClient
{
    public string name;
    public bool isHost;
}
