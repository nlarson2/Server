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
        protected Dictionary<int, PlayerData> users =  new Dictionary<int, PlayerData>();            //hashtable of users
       // protected Dictionary<int, ClientData> connectingUsers =  new Dictionary<int, ClientData>();  //hashtable of users that havent finished connecting


        public GameObject playerPrefab;   //Networked player model
        public Transform parent;          //Location in hierarchy
        public Transform spawnpoint;       //Spawn point in world

        Queue<ClientData> instantiatePlayerQ = new Queue<ClientData>();
        Queue<int> removePlayerQ = new Queue<int>();
        Queue<Bullet> bulletQ = new Queue<Bullet>();

        Thread msgThread; //thread to receive messages continuously
        Thread snapShot;

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
           

            while (instantiatePlayerQ.Count > 0)
            {
                //instatiate net player object in the game and store object and client info in users dictionary
                PlayerData player = new PlayerData(instantiatePlayerQ.Dequeue());
                player.obj = Instantiate(playerPrefab, spawnpoint.position, spawnpoint.rotation, parent);
                player.playerControl = player.obj.GetComponent<Player>();
                users.Add(player.clientData.id, player);
                player.obj.name = "NET_PLAYER_" + player.clientData.id;
            }

            while(removePlayerQ.Count > 0)
            {
                int player = removePlayerQ.Dequeue();
                try
                {
                    //destory object and remove them from users
                    PlayerData playerData = users[player];
                    Destroy(playerData.obj);
                    playerData.clientData.socket.Close();
                    users.Remove(player);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
            
            while(bulletQ.Count > 0)
            {
                //waiting on bullets
            }
        }
        
        private void OnApplicationQuit()
        {

            try
            {
                msgThread.Abort();
                snapShot.Abort();
            }
            catch( Exception e)
            {
                Debug.Log(e);
            }
            foreach(PlayerData p in users.Values)
            {
                p.clientData.socket.Close();
            }
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
                    try
                    {
                        msg = JsonUtility.FromJson<Message>(newMsg);
                        Debug.Log("WORKED");
                    }
                    catch (ArgumentException e) //used to test what errors occur with Json messaging
                    {
                        Debug.Log(e);
                        Debug.Log(newMsg);
                        continue;
                    }
                    switch((MsgType)msg.msgType)
                    {
                        case MsgType.LOGIN:
                            Login(newMsg);
                            break;
                        case MsgType.LOGOUT:
                            Logout(newMsg);
                            Debug.Log("LOGGOUT");
                            break;
                        case MsgType.MOVE:
                            if(true) //eventually check if VR player
                            {
                                Move(newMsg);
                            }
                            else
                            {
                                //MoveVR(newMsg);
                            }
                            break;
                        case MsgType.SHOOT:
                            break;
                        /*Shouldn't get any cases below this points*/
                        case MsgType.SNAPSHOT:
                            break;
                        case MsgType.STRUCTURE:
                            break;

                    }
                }
                //wait for a message to be on the queue and then restart the loop
                SpinWait.SpinUntil(() => server.msgQueue.Count > 0);
            }
        }

        private void Login(string msg)
        {
            //pull new player off connectingUsers on the server and instantiate them into the game
            LoginMsg loginMsg = JsonUtility.FromJson<LoginMsg>(msg);
            ClientData clientData = server.connecting[loginMsg.from];
            server.connecting.Remove(loginMsg.from);
            instantiatePlayerQ.Enqueue(clientData);

        }
        private void Logout(string msg)
        {
            LogoutMsg logout = JsonUtility.FromJson<LogoutMsg>(msg);
            removePlayerQ.Enqueue(logout.from);
        }
        private void Move(string msg)
        {
            MoveMsg moveMsg = JsonUtility.FromJson<MoveMsg>(msg);
            Player playerController = users[moveMsg.from].playerControl;
            playerController.position = moveMsg.pos;
            playerController.rotation = moveMsg.playerRotation;
            playerController.cameratRotation = moveMsg.camerRotation;
        }
        private void Shoot(string msg)
        {

        }
        private void Snapshot()
        {

        }
        private void Structure()
        {

        }





        public void Send(Message msg)
        {
            byte[] json = System.Text.ASCIIEncoding.ASCII.GetBytes(JsonUtility.ToJson(msg));
            ClientData cli = users.ElementAt(msg.to).Value.clientData;
            server.SendMsg(cli, json);
        }
        
        
    }
}
