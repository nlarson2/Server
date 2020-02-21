//For documentation on Net and Net.Socket (TcpClients, IPaddress, etc...)
//https://docs.microsoft.com/en-us/dotnet/api/system.net?view=netframework-4.8


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using UnityEngine;
using Unity.Jobs;
namespace SmashDomeNetwork
{
    class NetworkManager : MonoBehaviour
    {

        
        Server server;// = new Server(50000);
        protected Dictionary<int, PlayerData> users;            //hashtable of users
        protected Dictionary<int, ClientData> connectingUsers;  //hashtable of users that havent finished connecting

        public GameObject playerPrefab;   //Networked player model
        public Transform parent;          //Location in hierarchy
        public Transform spawnpoint;       //Spawn point in world

        Queue<PlayerData> instatiatePlayerQ = new Queue<PlayerData>();
        Queue<PlayerData> removePlayerQ = new Queue<PlayerData>();
        Queue<Bullet> bulletQ = new Queue<Bullet>();

        Thread msgThread; //thread to receive messages continuously

        //Set up to make NetworkManger a singleton
        private NetworkManager() { }
        private static NetworkManager instance = null;
        private static readonly object padlock = new object(); //lock down the Instance
        public static NetworkManager Instance
        {
            get
            {
                return instance;
            }
            set
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = value;
                        instance.server = new Server(50000);
                    }
                }

            }

        }


        private void Start()
        {
            Instance = this;
            msgThread = new Thread(ReceiveMessages);
            msgThread.Start();
        }

        private void Update()
        {
            //watch all queues for updates
            while (server.newUserData.Count > 0)
            {
                ClientData cli = server.newUserData.Dequeue();
                connectingUsers.Add(cli.id, cli);
            }

            while (instatiatePlayerQ.Count > 0)
            {
                PlayerData player = instatiatePlayerQ.Dequeue();
                player.obj = Instantiate(playerPrefab, spawnpoint.position, spawnpoint.rotation, parent);
            }

            while(removePlayerQ.Count > 0)
            {
                PlayerData player = removePlayerQ.Dequeue();
                Destroy(player.obj);
                try
                {
                    player.clientData.socket.Close();
                    users.Remove(player.clientData.id);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
            
            while(bulletQ.Count > 0)
            {
                //dont care right now
            }
        }
        
        private void OnApplicationQuit()
        {
            server.listen.Stop();
        }

        
        //thread continuely runs trying to pull messages form the server
        public void ReceiveMessages()
        {
            string newMsg;
            Message msg;
            while (true)
            {
                newMsg = string.Empty;
                while (server.msgQueue.Count > 0)
                {
                    newMsg = server.msgQueue.Dequeue();
                    msg = JsonUtility.FromJson<Message>(newMsg);
                    MsgType type = (MsgType)msg.msgType;
                    switch(type)
                    {
                        case MsgType.LOGIN:
                            break;
                        case MsgType.LOGOUT:
                            break;
                        case MsgType.MOVE:
                            break;
                        case MsgType.SHOOT:
                            break;
                        case MsgType.SNAPSHOT:
                            break;
                        case MsgType.STRUCTURE:
                            break;

                    }
                    //process message and add to whatever queue
                    //bullet or move
                }
                SpinWait.SpinUntil(() => server.msgQueue.Count > 0);
            }
        }

        public void Send(Message msg)
        {
            byte[] json = System.Text.ASCIIEncoding.ASCII.GetBytes(JsonUtility.ToJson(msg));
            ClientData cli= users.ElementAt(msg.to).Value.clientData;
            server.SendMsg(cli, json);
        }
        
        
    }
}
