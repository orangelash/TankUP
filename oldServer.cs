using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server : MonoBehaviour
{
    public int port = 6321;
    private List<ServerClient> clients;
    private List<ServerClient> disconnectList;
    private int beginCounter = 0;


    private TcpListener server;
    private bool serverStarted;

    public void Init()
    {
        DontDestroyOnLoad(gameObject);
        clients = new List<ServerClient>();
        disconnectList = new List<ServerClient>();

        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            serverStarted = true;
            Debug.Log("server started = "+serverStarted);
            StartListening();

        }
        catch (Exception e)
        {
            Debug.Log("Socket Error " + e.Message);
        }

    }

    private void Update()
    {
        if (!serverStarted)
            return;

        foreach (ServerClient c in clients)
        {
            //not Connected
            if (!IsConnected(c.tcp))
            {
                c.tcp.Close();
                disconnectList.Add(c);
                continue;
            }
            else
            {
                NetworkStream s = c.tcp.GetStream();
                if (s.DataAvailable)
                {
                    StreamReader reader = new StreamReader(s, true);
                    string data = reader.ReadLine();

                    if (data != null)
                        OnIncomingData(c, data);
                }
            }
        }

        for (int i = 0; i < disconnectList.Count - 1; i++)
        {
            clients.Remove(disconnectList[i]);
            disconnectList.RemoveAt(i);
        }

        //Encontrar dois clientes que não estão a jogar
        //for (int i = 0; i < clients.Count - 1; i++)
        //{
        //    if(clients[i].inGame == false)
        //    {
        //        for (int j = 0; j < clients.Count -1; j++)
        //        {
        //            if (j != i && clients[j].inGame == false)
        //            {
        //                clients[i].inGame = true;
        //                clients[j].inGame = true;
        //                Broadcast("board|", clients[i]);
        //                Broadcast("board|", clients[j]);
        //            }
        //        }
        //    }
        //}
    }

    private void StartListening()
    {
        server.BeginAcceptTcpClient(AcceptTcpClient, server);
    }

    private void AcceptTcpClient(IAsyncResult ar)
    {   
        Debug.Log("Server has begun listening for clients");
       
        TcpListener listener = (TcpListener)ar.AsyncState;



        string allUsers = "";
        foreach (ServerClient I in clients)
        {
            if (I.clientName != "")

                allUsers += I.clientName + "|";
            Debug.Log(allUsers);
        }

        ServerClient sc = new ServerClient(listener.EndAcceptTcpClient(ar));
       
            clients.Add(sc);
            Debug.Log(clients);

            if (clients.Count < 3)
                StartListening();
            else
            {
                //Game launch!
                clients[1].inGame = true;
                clients[2].inGame = true;

                Broadcast("1|" + clients[1].clientName, clients[1]);
                Broadcast("2|" + clients[2].clientName, clients[2]);
            }
            Broadcast("SWHO|", clients[clients.Count - 1]);
        
    }

    private bool IsConnected(TcpClient c)
    {
        try
        {
            if (c != null && c.Client != null && c.Client.Connected)
            {
                if (c.Client.Poll(0, SelectMode.SelectRead))
                    return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                return true;
            }
            else
                return false;
        }
        catch (System.Exception)
        {

            return false;
        }
    }

    private void Broadcast(string data, List<ServerClient> cl)
    {
        foreach (ServerClient sc in cl)
        {
            try
            {
                StreamWriter writer = new StreamWriter(sc.tcp.GetStream());
                writer.WriteLine(data + "|");
                writer.Flush();
            }
            catch (Exception e)
            {
                Debug.Log("write error: " + e.Message);
            }
        }


    }

    private void Broadcast(string data, ServerClient c)
    {
        List<ServerClient> sc = new List<ServerClient> { c };
        Broadcast(data, sc);
    }

    private void OnIncomingData(ServerClient c, string data)
    {

        string[] aData = data.Split('|');

        switch (aData[0])
        {
            case "CWHO":
                c.clientName = aData[1];
                c.isHost = (aData[2] == "0") ? false : true;
                Broadcast("SCNN|" + c.clientName, clients);
                break;

            case "READYTOBEGIN":
                beginCounter++;
                //Defines player1 and player2 to the server. Also defines the boards for each
                //client.Send("READYTOBEGIN|" + client.c.name + "|"+client.c.isPlayer1+"|" + board);

                if (aData[2] == "true")
                {
                    if (aData[1] == clients[1].clientName)
                    {
                        clients[1].isPlayer1 = true;
                        clients[1].isHost = true;
                        clients[1].board = aData[3];
                    }
                    else
                        clients[1].board = aData[3];

                }

                if (beginCounter == 2)
                {
                    Debug.Log("TWO PLAYERS ARE READY");
                    //The two players are now ready to begin playing. Player 1 has the attack button enabled, player2 does not
                    if (clients[1].isPlayer1 == true) //should always be true
                        Broadcast("ATTACK|1|" + clients[1].clientName + "|" + clients[1].isPlayer1, clients);
                    else
                        Broadcast("ATTACK|0|" + clients[2].clientName + "|" + clients[2].isPlayer1, clients);
                }
                break;

            case "ATTACKBUTTON":
                Debug.Log("A CLIENT WANTS TO ATTACK");
                if (aData[1] == clients[1].clientName)
                {
                    Broadcast("NEWBOARD&ATTACK|" + clients[2].board + "|" + clients[2].isPlayer1, clients[1]);
                }
                else
                    Broadcast("NEWBOARD&ATTACK|" + clients[1].board + "|" + clients[1].isPlayer1, clients[2]);
                break;
        }

    }
}

public class ServerClient
{
    public string clientName;
    public TcpClient tcp;
    public bool isHost = false;
    public bool inGame;
    public string board;
    public bool isPlayer1 = false;

    public ServerClient(TcpClient tcp)
    {
        this.tcp = tcp;
    }
}