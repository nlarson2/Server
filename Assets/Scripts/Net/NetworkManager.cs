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

        }

        private void Update()
        {
            
        }

     
        private void OnApplicationQuit()
        {
            server.listen.Stop();
        }

        //static void Main(string[] args)
        //{
        //    //Debug.Log("Hellow world 1");
        //    Server serv = new Server(55555);
        //    serv.StartServer();



        //    Console.Read();
        //}
    }
}
