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
        protected Dictionary<int, Player> users; //hashtable of users

        public Transform spawpoint;
        Queue<Player> instatiatePlayerQ = new Queue<Player>();
        Queue<Player> removePlayerQ = new Queue<Player>();
        Queue<Bullet> bulletQ = new Queue<Bullet>();
        Thread msgThread;

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
            
        }
        
        private void OnApplicationQuit()
        {
            server.listen.Stop();
        }

        
        //thread continuely runs trying to pull messages form the server
        public void ReceiveMessages()
        {
            string newMsg;
            while (true)
            {
                newMsg = string.Empty;
                while (server.msgQueue.Count > 0)
                {
                    newMsg = server.msgQueue.Dequeue();
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
