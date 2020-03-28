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

        public Cerealize cc = new Cerealize();

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
                AddPlayerMsg addPlayerMsg = new AddPlayerMsg(player.clientData.id);
                KeyValuePair<int, PlayerData>[] players = users.ToArray();
                Debug.Log(players.Length);
                foreach (KeyValuePair<int,PlayerData> playerData in players)
                {
                    if (playerData.Value.clientData.id == player.clientData.id)
                        continue;
                    Debug.Log("ADD USER SENT");
                    addPlayerMsg.from = player.clientData.id;
                    addPlayerMsg.to = playerData.Value.clientData.id;
                    byte[] msg = cc.SerializeMSG(addPlayerMsg);
                    //Send((Message)addPlayerMsg);
                    Send(msg, addPlayerMsg.to);

                    addPlayerMsg.from = playerData.Value.clientData.id;
                    addPlayerMsg.to = player.clientData.id;
                    msg = cc.SerializeMSG(addPlayerMsg);
                    //Send((Message)addPlayerMsg);
                    Send(msg, addPlayerMsg.to);
                }
                foreach (StructureChangeMsg structMsg in structures)
                {
                    structMsg.to = player.clientData.id;

                    byte[] MSG = cc.SerializeMSG(structMsg);

                    Debug.Log("StructMsg.vertices + triangles");
                    Debug.Log(structMsg.vertices.Length);
                    Debug.Log(structMsg.triangles.Length);

                    Send(MSG, structMsg.to);
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
                        byte[] msg = cc.SerializeMSG(logoutMsg);
                        //Send((Message)logoutMsg);
                        Send(msg, logoutMsg.to);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
            
            while(bulletQ.Count > 0)
            {
                ShootMsg shootMsg = bulletQ.Dequeue();
                //waiting on bullets
                GameObject bull = Instantiate(bulletPrefab,shootMsg.position, transform.rotation);
                Rigidbody rig = bull.GetComponent<Rigidbody>();
                rig.useGravity = false;
                //rig.AddForce(Physics.gravity * (rig.mass * rig.mass));
                //rig.AddForce((transform.forward + transform.up / 4) * 2.0f);
                rig.AddForce(shootMsg.direction);
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
            Message msg;
            while (true)
            {
                
                while (server.msgQueue.Count > 0)
                {
                    newMsg = server.msgQueue.Dequeue();

                    Debug.Log("msgType: ");
                    Debug.Log(cc.ByteInt32(newMsg[4]));
                    try
                    {
                        
                        //msg = cc.DeserializeMSG(newMsg);
                    }
                    catch (ArgumentException e) //used to test what errors occur with Json messaging
                    {
                        Debug.Log(e);
                        Debug.Log(newMsg);
                        continue;
                    }
                    int type = cc.ByteInt32(newMsg[4]);
                    switch((MsgType)type)
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
            LoginMsg loginMsg = cc.DeserializeLiMSG(msg);
            ClientData clientData = server.connecting[loginMsg.from];
            server.connecting.Remove(loginMsg.from);
            instantiatePlayerQ.Enqueue(clientData);
            

        }
        private void Logout(byte[] msg)
        {
            LogoutMsg logout = cc.DeserializeLoMSG(msg);
            removePlayerQ.Enqueue(logout.from);
        }
        private void Move(byte[] msg)
        {
            
            MoveMsg moveMsg = cc.DeserializeMMSG(msg);
            Player playerController = users[moveMsg.from].playerControl;
            playerController.position = moveMsg.pos;
            playerController.rotation = moveMsg.playerRotation;
            playerController.cameratRotation = moveMsg.cameraRotation;

            foreach (PlayerData player in users.Values)
            {
                if (player.clientData.id == moveMsg.from)
                    continue;
                moveMsg.to = player.clientData.id;

                byte[] MSG = cc.SerializeMSG(moveMsg);

                Send(MSG, moveMsg.to);
                
            }
                                    



        }
        private void Shoot(byte[] msg)
        {
            ShootMsg shoot = cc.DeserializeSMSG(msg);
            bulletQ.Enqueue(shoot);
            foreach(PlayerData playerData in users.Values)
            {
                if(playerData.clientData.id != shoot.from)
                {
                    try
                    {
                        shoot.to = playerData.clientData.id;
                        
                        byte[] MSG = cc.SerializeMSG(shoot);

                        Send(MSG, shoot.to);
                    }
                    catch(Exception e)
                    {
                        //Debug.Log(e);
                    }
                }
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

                        Send(clients.ToArray(), snapshot);
                        time = 0;
                    }
                    prevTime = DateTime.Now;
                }

                SpinWait.SpinUntil(() => users.Count > 1);
            }

        }





        public void Send(byte[] msg, int to)
        {
            //byte[] MSG = cc.SerializeMSG(msg);
            ClientData cli = users[to].clientData;
            server.SendMsg(cli, msg);
        }
        public void Send(ClientData[] clients, Message msg)
        {
            byte[] MSG = cc.SerializeMSG(msg);
            server.SendMsg(clients, MSG);
        } 

    }
}
