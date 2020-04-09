﻿//For documentation on Net and Net.Socket (TcpClients, IPaddress, etc...)
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
    public class NetworkManager : MonoBehaviour
    {

        
        Server server;// = new Server(50000);
        protected Dictionary<int, PlayerData> users =  new Dictionary<int, PlayerData>();            //hashtable of users
                                                                                                     // protected Dictionary<int, ClientData> connectingUsers =  new Dictionary<int, ClientData>();  //hashtable of users that havent finished connecting
        public List<StructureChangeMsg> structures = new List<StructureChangeMsg>();

        public GameObject playerPrefab;   //Networked player model
        public GameObject bulletPrefab;
        public Transform parent;          //Location in hierarchy
        public Transform spawnpoint;       //Spawn point in world

        Queue<ClientData> instantiatePlayerQ = new Queue<ClientData>();
        Queue<int> removePlayerQ = new Queue<int>();
        Queue<ShootMsg> bulletQ = new Queue<ShootMsg>();

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
                        instance.server = new Server(44444);
                    }
                }

            }

        }


        private void Start()
        {
            Instance = this;
            msgThread = new Thread(ReceiveMessages);
            msgThread.Start();
            /*snapShot = new Thread(SendSnapshot);
            snapShot.Start();*/
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
                AddPlayerMsg addPlayerMsg = new AddPlayerMsg(player.clientData.id, (int)player.GetPlayerType());
                KeyValuePair<int, PlayerData>[] players = users.ToArray();
                Debug.Log(String.Format("PLAYERS LENGTH: {0}: ", players.Length));
                foreach (KeyValuePair<int,PlayerData> playerData in players)
                {
                    if (playerData.Value.clientData.id == player.clientData.id)
                        continue;
                    Debug.Log("ADD USER SENT");
                    addPlayerMsg.from = player.clientData.id;
                    addPlayerMsg.to = playerData.Value.clientData.id;
                    Send(addPlayerMsg.GetBytes(), addPlayerMsg.to);
                    addPlayerMsg.from = playerData.Value.clientData.id;
                    addPlayerMsg.to = player.clientData.id;
                    Send(addPlayerMsg.GetBytes(), addPlayerMsg.to);
                }
                foreach (StructureChangeMsg structMsg in structures)
                {
                    structMsg.to = player.clientData.id;
                    Debug.Log(String.Format("STRUCT SENT TO: {0}", structMsg.to));
                    Send(structMsg.GetBytes(), structMsg.to);
                }
            }

            while(removePlayerQ.Count > 0)
            {
                int player = removePlayerQ.Dequeue();
                try
                {
                    //destory object and remove them from users
                    PlayerData playerD = users[player];
                    Destroy(playerD.obj);
                    playerD.clientData.socket.Close();
                    users.Remove(player);
                    LogoutMsg logoutMsg = new LogoutMsg(player);
                    KeyValuePair<int, PlayerData>[] players = users.ToArray();
                    foreach (KeyValuePair<int, PlayerData> playerData in players)
                    {
                        if (playerData.Value.clientData.id == player)
                            continue;
                        logoutMsg.to = playerData.Value.clientData.id;
                        Send(logoutMsg.GetBytes(), logoutMsg.to);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
            
            while(bulletQ.Count > 0)
            {
                Debug.Log(bulletQ.Count);
                ShootMsg shootMsg = bulletQ.Dequeue();
                //waiting on bullets
                GameObject bull = Instantiate(bulletPrefab, shootMsg.position, Quaternion.identity);
                Rigidbody rig = bull.GetComponent<Rigidbody>();
                rig.useGravity = false;
                //rig.AddForce(Physics.gravity * (rig.mass * rig.mass));
                //rig.AddForce((transform.forward + transform.up / 4) * 2.0f);
                int speed = 1;
                rig.AddForce(shootMsg.direction.normalized * speed);
                Debug.Log("FIRED");
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
            byte[] newMsg;
           // Message msg;
            while (true)
            {
                newMsg = null;
                while (server.msgQueue.Count > 0)
                {
                    newMsg = server.msgQueue.Dequeue();
                    
                    int msgType = Message.BytesToInt(Message.GetSegment(4, 4, newMsg));
                    switch ((MsgType)msgType)
                    {
                        case MsgType.LOGIN:
                            Login(newMsg);
                            
                            break;
                        case MsgType.LOGOUT:
                            Logout(newMsg);
                            Debug.Log("LOGGOUT");
                            break;
                        case MsgType.MOVE:
                            Move(newMsg);
                            break;
                        case MsgType.MOVEVR:
                            Debug.Log("MOVEVR");
                            MoveVR(newMsg);
                            break;
                        case MsgType.SHOOT:
                            Shoot(newMsg);                            
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

        private void Login(byte[] msg)
        {
            //pull new player off connectingUsers on the server and instantiate them into the game
            //LoginMsg loginMsg = JsonUtility.FromJson<LoginMsg>(msg);
            LoginMsg loginMsg = new LoginMsg(msg);
            ClientData clientData = server.connecting[loginMsg.from];
            clientData.playerType = loginMsg.playerType;
            server.connecting.Remove(loginMsg.from);
            instantiatePlayerQ.Enqueue(clientData);
        }
        private void Logout(byte[] msg)
        {
            //LogoutMsg logout = JsonUtility.FromJson<LogoutMsg>(msg);
            LogoutMsg logout = new LogoutMsg(msg);
            removePlayerQ.Enqueue(logout.from);
        }
        private void Move(byte[] msg)
        {
            // MoveMsg moveMsg = JsonUtility.FromJson<MoveMsg>(msg);
            MoveMsg moveMsg = new MoveMsg(msg);
            Player playerController = users[moveMsg.from].playerControl;
            playerController.position = moveMsg.pos;
            playerController.rotation = moveMsg.playerRotation;
            playerController.cameratRotation = moveMsg.cameraRotation;

            KeyValuePair<int, PlayerData>[] players = users.ToArray();
            Debug.Log(String.Format("PLAYERS LENGTH: {0}: ", players.Length));
            int countTest = 0;
            foreach (KeyValuePair<int, PlayerData> playerData in players)
            {
                Debug.Log("Movement");
                moveMsg.to = playerData.Value.clientData.id;
                Send(msg, playerData.Value.clientData.id);
                
            }
        }
        private void MoveVR(byte[] msg)
        {
            // MoveMsg moveMsg = JsonUtility.FromJson<MoveMsg>(msg);
            MoveVRMsg moveMsg = new MoveVRMsg(msg);
            Player playerController = users[moveMsg.from].playerControl;
            playerController.position = moveMsg.pos;
            playerController.rotation = moveMsg.playerRotation;
            playerController.cameratRotation = moveMsg.cameraRotation;
            playerController.lHandPos = moveMsg.lHandPosition;
            playerController.rHandPos = moveMsg.rHandPosition;
            playerController.lHandRot = moveMsg.lHandRotation;
            playerController.rHandRot = moveMsg.rHandRotation;

            KeyValuePair<int, PlayerData>[] players = users.ToArray();
            Debug.Log(String.Format("PLAYERS LENGTH: {0}: ", players.Length));
            int countTest = 0;
            foreach (KeyValuePair<int, PlayerData> playerData in players)
            {
                Debug.Log("Movement");
                moveMsg.to = playerData.Value.clientData.id;
                Send(msg, playerData.Value.clientData.id);

            }
        }
        private void Shoot(byte[] msg)
        {
            //ShootMsg shoot = JsonUtility.FromJson<ShootMsg>(msg);
            ShootMsg shoot = new ShootMsg(msg);
            bulletQ.Enqueue(shoot);
            KeyValuePair<int, PlayerData>[] players = users.ToArray();
            Debug.Log(String.Format("PLAYERS LENGTH: {0}: ", players.Length));
            int countTest = 0;
            foreach (KeyValuePair<int, PlayerData> playerData in players)
            {
                /*if (playerData.Value.clientData.id == moveMsg.from)
                {
                    print(String.Format("TEST {0}",countTest));
                    countTest++;
                    continue;
                }*/
                Debug.Log("Movement");
                shoot.to = playerData.Value.clientData.id;
                Send(msg, playerData.Value.clientData.id);

            }

        }
        private void Snapshot()
        {

        }
        private void Structure()
        {

        }

        private void SendSnapshot()
        {
            while (true) {
                DateTime prevTime = DateTime.Now;
                DateTime curTime;
                double time = 0;
                while (users.Count > 1)
                {
                    curTime = DateTime.Now;
                    time += (curTime - prevTime).TotalSeconds;
                    Debug.Log(time);
                    if (time > 0.2f)
                    {
                        SnapshotMsg snapshot = new SnapshotMsg();
                        List<ClientData> clients = new List<ClientData>();
                        KeyValuePair<int, PlayerData>[] players = users.ToArray();
                        foreach (KeyValuePair<int, PlayerData> playerKey in players)
                        {
                            PlayerData playerData = playerKey.Value;
                            Player playerController = playerData.playerControl;
                            snapshot.userId.Add(playerData.clientData.id);
                            snapshot.positions.Add(playerController.position);
                            snapshot.rotation.Add(playerController.rotation);
                            snapshot.camRotation.Add(playerController.cameratRotation);
                            clients.Add(playerData.clientData);
                        }

                        //Send(clients.ToArray(), snapshot);
                        time = 0;
                    }
                    prevTime = DateTime.Now;
                }

                SpinWait.SpinUntil(() => users.Count > 1);
            }

        }



        public void print(string output)
        {
            
            Debug.Log(String.Format(output));
        
        }

        public void Send(byte[] msg, int to)
        {
            //byte[] json = System.Text.ASCIIEncoding.ASCII.GetBytes(JsonUtility.ToJson(msg));
            ClientData cli = users[to].clientData;
            server.SendMsg(cli, msg);
        }
        public void Send(ClientData[] clients, byte[] msg)
        {
           // byte[] json = System.Text.ASCIIEncoding.ASCII.GetBytes(JsonUtility.ToJson(msg));
            server.SendMsg(clients, msg);
        }

    }
}
