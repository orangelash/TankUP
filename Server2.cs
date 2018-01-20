using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server2 : MonoBehaviour
{

    public int port;
    private List<ServerClient> clients;
    private List<ServerClient> DisconnectList;
    private TcpListener server;
    private bool serverStarted;


    // Use this for initialization
    public void Init()
    {
        DontDestroyOnLoad(gameObject);
        clients = new List<ServerClient>();
        DisconnectList = new List<ServerClient>();
        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
        }
        catch (Exception e)
        {
            Debug.Log("Socket error" + e.Message);
        }
    }

    // Update is called once per frame
    public void Update() //se quiser por o server em linux, monobehavior remove, corre update num outro thread, em loop
    {
        if (!serverStarted)
            return;
        foreach (ServerClient c in clients)
        {
            //Is the client still connected?
            if (!IsConnected(c.tcp))
            {
                c.tcp.Close();
                DisconnectList.Add(c);
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
                        OnIncommingData(c, data);
                }
            }
        }
        for (int i = 0; i < DisconnectList.Count - 1; i++)
        {
            //tell someone has disconnected
            clients.Remove(DisconnectList[i]);
            DisconnectList.RemoveAt(i);
        }
    }

    private void OnIncommingData(ServerClient c, string data)
    {
        Debug.Log(c.ClientName + " : " + data);

        string[] aData = data.Split('|');
        switch (aData[0])
        {
            case "CWHO":
                c.ClientName = aData[1];
                c.isHost = (aData[2] == "0") ? false : true;
                Broadcast("SCNN|" + c.ClientName, clients);
                break;
        }
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

    private void StartListening()
    {
        server.BeginAcceptTcpClient(AcceptTcpClient, server);
    }
    private void AcceptTcpClient(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener)ar.AsyncState;

        string allUsers = "";
        foreach (ServerClient i in clients)
        {
            allUsers += i.ClientName + "|";
        }

        ServerClient sc = new ServerClient(listener.EndAcceptTcpClient(ar));
        clients.Add(sc);
        StartListening();
        Debug.Log("Somebody has connected");


        Broadcast("SWHO|" + allUsers, clients[clients.Count - 1]);
    }



    private void Broadcast(string data, List<ServerClient> cl)
    {
        foreach (ServerClient sc in cl)
        {
            try
            {
                StreamWriter writer = new StreamWriter(sc.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (Exception e)
            {
                Debug.Log("write error " + e.Message);
            }
        }
    }

    private void Broadcast(string data, ServerClient sc)
    {
        List<ServerClient> lsc = new List<ServerClient> { sc };
        Broadcast(data, lsc);
    }

    public class ServerClient
    {
        public string ClientName;
        public bool isHost;
        public TcpClient tcp;
        public ServerClient(TcpClient tcp)
        {
            this.tcp = tcp;
        }

    }

}
