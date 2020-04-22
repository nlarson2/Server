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

        GameManager gameManger;
        Server server;// = new Server(50000);
        protected Dictionary<int, PlayerData> users = new Dictionary<int, PlayerData>();            //hashtable of users
                                                                                                    // protected Dictionary<int, ClientData> connectingUsers =  new Dictionary<int, ClientData>();  //hashtable of users that havent finished connecting
        public Dictionary<int, StructureChangeMsg> structures = new Dictionary<int, StructureChangeMsg>();

        public Dictionary<int, Snapshot> netobjects = new Dictionary<int, Snapshot>();

        public GameObject playerPrefab;   //Networked player model
        public GameObject bulletPrefab;
        public Transform parent;          //Location in hierarchy
        public Transform spawnpoint;       //Spawn point in world
        public GameObject netCube;

        Queue<ClientData> instantiatePlayerQ = new Queue<ClientData>();
        Queue<int> removePlayerQ = new Queue<int>();
        Queue<ShootMsg> bulletQ = new Queue<ShootMsg>();



        Thread msgThread; //thread to receive messages continuously
        Thread snapShot;


        public StoredServerData serverData;
        public static int port;
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

                        instance.server = new Server(port);
                    }
                }

            }

        }


        private void Start()
        {
            gameManger = GameObject.Find("Game Manager").GetComponent<GameManager>();
            serverData = GameObject.Find("PassingData").GetComponent<StoredServerData>();
            port = serverData.parsedPortNumber;
            serverData.Destory();

            Instance = this;
            msgThread = new Thread(ReceiveMessages);
            msgThread.Start();
            snapShot = new Thread(SendSnapshot);
            snapShot.Start();
        }

        private void Update()
        {


            while (instantiatePlayerQ.Count > 0)
            {
                //instatiate net player object in the game and store object and client info in users dictionary
                PlayerData player = new PlayerData(instantiatePlayerQ.Dequeue());
                player.obj = Instantiate(playerPrefab, spawnpoint.position, spawnpoint.rotation, parent);
                player.playerControl = player.obj.GetComponent<Player>();
                player.playerControl.id = player.clientData.id;
                users.Add(player.clientData.id, player);
                player.obj.name = "NET_PLAYER_" + player.clientData.id;
                AddPlayerMsg addPlayerMsg = new AddPlayerMsg(player.clientData.id, (int)player.GetPlayerType(), (int)player.GetPersonType());
                //addPlayerMsg = new AddPlayerMsg(addPlayerMsg.GetBytes());
                //Debug.Log(string.Format("Instantiate player: {0}", addPlayerMsg.playerType));



                KeyValuePair<int, PlayerData>[] players = users.ToArray();
                ////Debug.Log(String.Format("PLAYERS LENGTH: {0}: ", players.Length));
                foreach (KeyValuePair<int, PlayerData> playerData in players)
                {
                    if (playerData.Value.clientData.id == player.clientData.id)
                        continue;
                    ////Debug.Log("ADD USER SENT");
                    addPlayerMsg.from = player.clientData.id;
                    addPlayerMsg.to = playerData.Value.clientData.id;
                    Send(addPlayerMsg.GetBytes(), addPlayerMsg.to);

                    addPlayerMsg.from = playerData.Value.clientData.id;
                    addPlayerMsg.to = player.clientData.id;
                    addPlayerMsg.personType = playerData.Value.clientData.personType;//test
                    Send(addPlayerMsg.GetBytes(), addPlayerMsg.to);
                }
                foreach (StructureChangeMsg structMsg in structures.Values)
                {
                    structMsg.to = player.clientData.id;
                   // //Debug.Log(String.Format("STRUCT SENT TO: {0}", structMsg.to));
                    Send(structMsg.GetBytes(), structMsg.to);
                }
                foreach (Snapshot snap in netobjects.Values)
                {
                    //Debug.Log(snap)

                    NetObjectMsg netObjMsg = snap.ThisObj;
                    Debug.Log(netObjMsg.objID);
                    //Debug.Log(netObjMsg.to);
                    //Debug.Log(player.clientData.id);
                    //netObjMsg.to = player.clientData.id;
                    //Debug.Log(String.Format("STRUCT SENT TO: {0}", netObjMsg.to));
                    //Send(netObjMsg.GetBytes(), netObjMsg.to);
                    Send(netObjMsg.GetBytes(), player.clientData.id);
                }
            }

            while (removePlayerQ.Count > 0)
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
                    //Debug.Log(e);
                }
            }

            while (bulletQ.Count > 0)
            {
                ////Debug.Log("Gets here");
                ////Debug.Log(bulletQ.Count);
                ShootMsg shootMsg = bulletQ.Dequeue();
                if (!gameManger.IsWorldBreakable())
                    continue;
                //waiting on bullets
                //GameObject bull = Instantiate(bulletPrefab, shootMsg.position, Quaternion.identity);

                // UPDATE HERE

                //GameObject bull = Instantiate(bulletPrefab, shootMsg.position, shootMsg.rotation);

                /*Rigidbody rig = bull.GetComponent<Rigidbody>();
                rig.useGravity = false;
                //rig.AddForce(Physics.gravity * (rig.mass * rig.mass));
                //rig.AddForce((transform.forward + transform.up / 4) * 2.0f);
                int speed = 6;

                //rig.AddForce(shootMsg.direction * speed);
                rig.AddForce(shootMsg.direction * speed);
                */

                Vector3 rot = shootMsg.rotation.eulerAngles;
                Vector3 fwd = shootMsg.position;
                //fwd -= Vector3.forward;
                //fwd = shootMsg.rotation * fwd;

                fwd = shootMsg.rotation * Vector3.forward;
                //fwd += shootMsg.position;




                /*Vector3 lookToPosition = transform.position;
                lookToPosition.y = (Mathf.Sin((rot.x % 360) * Mathf.Deg2Rad) * 100000);
                lookToPosition.x = (Mathf.Sin((rot.y % 360) * Mathf.Deg2Rad) * 100000);*/
                RaycastHit hit;
                Physics.Raycast(shootMsg.position, fwd, out hit, 100.0f);
                Debug.DrawRay(shootMsg.position, fwd * 20, Color.green, 5, false);
                //Debug.Log(string.Format("hit something? {0}", hit.transform.name));
                GameObject hitObj = hit.collider.gameObject;
                if (hitObj.tag == "Model")
                {
                    int textType = hitObj.GetComponent<SmashDomeVoxel.VoxelModel>().Collide(hit.point, shootMsg.shootType);
                    GameObject newCube = Instantiate(netCube, hit.point + new Vector3(0.1f, 0.1f, 0.1f), Quaternion.identity);
                    newCube.GetComponent<Snapshot>().textureType = textType;
                    if(shootMsg.shootType == 1)
                    {
                        newCube = Instantiate(netCube, hit.point + new Vector3(0.1f, 0.1f, 0.1f), Quaternion.identity);
                        newCube.GetComponent<Snapshot>().textureType = textType;
                        newCube = Instantiate(netCube, hit.point + new Vector3(0.1f, 0.1f, 0.1f), Quaternion.identity);
                        newCube.GetComponent<Snapshot>().textureType = textType;
                        newCube = Instantiate(netCube, hit.point + new Vector3(0.1f, 0.1f, 0.1f), Quaternion.identity);
                        newCube.GetComponent<Snapshot>().textureType = textType;
                    }

                }
                //Debug.Log(hitObj.tag);
                if (hitObj.tag == "NetPlayer")
                {
                    //Debug.Log("Player Got Shot In Network Manager");
                    hitObj.GetComponent<Player>().Shot();
                }
               
                //Debug.Log("FIRED");
            }
        }

        private void OnApplicationQuit()
        {

            try
            {
                msgThread.Abort();
                snapShot.Abort();
            }
            catch (Exception e)
            {
                //Debug.Log(e);
            }
            foreach (PlayerData p in users.Values)
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
                    //Debug.Log(string.Format("MSG TYPE: {0}", new LoginMsg(newMsg).from));
                    try
                    {
                        int msgType = Message.BytesToInt(Message.GetSegment(4, 4, newMsg));
                   
                        Debug.Log(string.Format("MSG TYPE: {0}", msgType));
                        switch ((MsgType)msgType)
                        {
                            case MsgType.LOGIN:
                                Login(newMsg);      
                                break;
                            case MsgType.LOGOUT:
                                Logout(newMsg);
                                //Debug.Log("LOGGOUT");
                                break;
                            case MsgType.MOVE:
                                Move(newMsg);
                                break;
                            case MsgType.MOVEVR:
                                //Debug.Log("MOVEVR");
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
                    catch (Exception e)
                    {
                        //Debug.Log(newMsg);
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
            //Debug.Log(string.Format("In NetMan: {0}", loginMsg.playerType));
            ClientData clientData = server.connecting[loginMsg.from];
            clientData.playerType = loginMsg.playerType;
            clientData.personType = loginMsg.personType;
            //Debug.Log(string.Format("LOGIN FROM: {0}", loginMsg.from));
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
            try
            {
                // MoveMsg moveMsg = JsonUtility.FromJson<MoveMsg>(msg);
                MoveMsg moveMsg = new MoveMsg(msg);
                //Debug.Log(string.Format("MOVE FROM: {0}", moveMsg.from));
                Player playerController = users[moveMsg.from].playerControl;
                playerController.position = moveMsg.pos;
                playerController.rotation = moveMsg.playerRotation;
                playerController.cameratRotation = moveMsg.cameraRotation;

                KeyValuePair<int, PlayerData>[] players = users.ToArray();
                //Debug.Log(String.Format("PLAYERS LENGTH: {0}: ", players.Length));
                int countTest = 0;
                foreach (KeyValuePair<int, PlayerData> playerData in players)
                {
                    //Debug.Log("Movement");
                    moveMsg.to = playerData.Value.clientData.id;
                    Send(msg, playerData.Value.clientData.id);

                }
            } catch (Exception e)
            {
                Debug.Log("SKIPPED");
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
            //Debug.Log(String.Format("PLAYERS LENGTH: {0}: ", players.Length));
            int countTest = 0;
            foreach (KeyValuePair<int, PlayerData> playerData in players)
            {
                //Debug.Log("Movement");
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
            //Debug.Log(String.Format("PLAYERS LENGTH: {0}: ", players.Length));
            int countTest = 0;
            foreach (KeyValuePair<int, PlayerData> playerData in players)
            {
                //Debug.Log("Movement");
                shoot.to = playerData.Value.clientData.id;
                Send(msg, playerData.Value.clientData.id);

            }

        }
        public void SnapshotOut(Snapshot snap)
        {
            netobjects[snap.objID] = snap;
            
            //SnapshotMsg msg = new SnapshotMsg(snap.objID);
            /*msg.objID = snap.objID;
            msg.textureType = snap.textureType;
            msg.positions = snap.pos;
            msg.rotation = snap.rot;
            msg.linear_speed = snap.linear_speed;
            msg.angular_speed = snap.angular_speed;*/

            /*KeyValuePair<int, PlayerData>[] players = users.ToArray();
            foreach (KeyValuePair<int, PlayerData> playerData in players)
            {
                //make changes to netobjects here to pos and rot only based on snapshot
                //Debug.Log("NetObject");
                msg.to = playerData.Value.clientData.id;
                //Send(msg.GetBytes(), playerData.Value.clientData.id);
            }*/
        }

        public void NetObject(NetObjectMsg msg)
        {
            //change to save many, hint: foreach like below
            LoadNetObjects(msg);
            
            //Debug.Log(msg.positions);

            KeyValuePair<int, PlayerData>[] players = users.ToArray();
            foreach (KeyValuePair<int, PlayerData> playerData in players)
            {
                //Debug.Log("NetObject");
                msg.to = playerData.Value.clientData.id;
                Send(msg.GetBytes(), playerData.Value.clientData.id);
            }
        }
        private void LoadNetObjects(NetObjectMsg msg)
        {
            //Debug.Log(msg.objID.Count);
            
            Snapshot snap = new Snapshot();
            snap.objID = msg.objID;
            snap.pos = msg.positions;
            snap.rot = msg.rotation;
            netobjects.Add(snap.objID, snap);
            
        }
        private void Structure()
        {

        }
        private int structCount = 0;

        public int AddModel(StructureChangeMsg msg)
        {
            msg.from = structCount;
            structures.Add(structCount, msg);
            return structCount++;
        }
        public void ChangeModel(StructureChangeMsg msg)
        {
            structures[msg.from] = msg;
            KeyValuePair<int, PlayerData>[] players = users.ToArray();
            foreach (KeyValuePair<int, PlayerData> playerData in players)
            {
                //Debug.Log("StructChange");
                msg.to = playerData.Value.clientData.id;
                Send(msg.GetBytes(), playerData.Value.clientData.id);
            }
        }

        private void SendSnapshot()
        {
            Debug.Log("thread started");
            while (true)
            {
                Dictionary<int, Vector3> lastPosition = new Dictionary<int, Vector3>(); //check if mem leak
                Dictionary<int, Quaternion> lastAngle = new Dictionary<int, Quaternion>(); //check if mem leak

                DateTime prevTime = DateTime.Now;
                DateTime curTime;
                double time = 0;
                while (netobjects.Count > 1)
                {
                    curTime = DateTime.Now;
                    time += (curTime - prevTime).TotalSeconds;
                    //Debug.Log(time);
                    if (time > 0.005f)
                    {
                        Debug.Log("IN TIME");
                        SnapshotMsg snapshot = new SnapshotMsg(0);

                        try
                        {
                            KeyValuePair<int, Snapshot>[] objs = netobjects.ToArray();
                        





                            foreach (KeyValuePair<int, Snapshot> obj in objs)
                            {
                                if (obj.Value.hasMoved)
                                {
                                    Debug.Log("HAS MOVED");
                                    snapshot.objID.Add(obj.Value.objID);
                                    snapshot.textureType.Add(obj.Value.textureType);
                                    Vector3 pos = obj.Value.pos;
                                    snapshot.positions.Add(pos);
                                    snapshot.angular_speed.Add(Quaternion.identity);
                                    snapshot.linear_speed.Add(Vector3.zero);

                                    Quaternion rot = obj.Value.rot;
                                    snapshot.rotation.Add(rot);

                                    //snapshot.linear_speed.Add((pos - lastPosition[obj.Value.objID]) / (float)time);
                                    //snapshot.linear_speed.Add(AngleShift(rot, lastAngle[obj.Value.objID]).eulerAngles / (float)time);


                                    //lastPosition.Add(obj.Value.objID, pos);
                                    //lastAngle.Add(obj.Value.objID, rot);
                                    obj.Value.hasMoved = false;
                                }
                            }
                            if (snapshot.objID.Count > 0)
                            {
                                KeyValuePair<int, PlayerData>[] players = users.ToArray();
                                foreach (KeyValuePair<int, PlayerData> playerData in players)
                                {
                                    //Debug.Log("StructChange");
                                    snapshot.to = playerData.Value.clientData.id;
                                    Send(snapshot.GetBytes(), playerData.Value.clientData.id);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.Log(string.Format("ERROR IN SNAPSHOT {0}", netobjects.Count));
                        }
                        //Snapshot(snapshot);
                        time = 0;
                    }
                    prevTime = DateTime.Now;


                    SpinWait.SpinUntil(() => netobjects.Count > 1);
                }

            }
        }
        private void SendCurrentSnapshot(int to)
        {
            SnapshotMsg snapshot = new SnapshotMsg(0);
            KeyValuePair<int, Snapshot>[] objs = netobjects.ToArray();
            foreach (KeyValuePair<int, Snapshot> obj in objs)
            {
                
                Debug.Log("HAS MOVED");
                snapshot.objID.Add(obj.Value.objID);
                snapshot.textureType.Add(obj.Value.textureType);
                Vector3 pos = obj.Value.pos;
                snapshot.positions.Add(pos);
                snapshot.angular_speed.Add(Quaternion.identity);
                snapshot.linear_speed.Add(Vector3.zero);

                Quaternion rot = obj.Value.rot;
                snapshot.rotation.Add(rot);

                //snapshot.linear_speed.Add((pos - lastPosition[obj.Value.objID]) / (float)time);
                //snapshot.linear_speed.Add(AngleShift(rot, lastAngle[obj.Value.objID]).eulerAngles / (float)time);


                //lastPosition.Add(obj.Value.objID, pos);
                //lastAngle.Add(obj.Value.objID, rot);
                obj.Value.hasMoved = false;
            }
            if (snapshot.objID.Count > 0)
            {
                KeyValuePair<int, PlayerData>[] players = users.ToArray();
                foreach (KeyValuePair<int, PlayerData> playerData in players)
                {
                    //Debug.Log("StructChange");
                    snapshot.to = playerData.Value.clientData.id;
                    Send(snapshot.GetBytes(), playerData.Value.clientData.id);
                }
            }
        }
        public Quaternion AngleShift(Quaternion prev, Quaternion now)
        {
            return (Quaternion.Inverse(prev) * now);
        }

        public void print(string output)
        {

            //Debug.Log(String.Format(output));

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

        public void CloserServer()
        {
            try
            {
                msgThread.Abort();
                snapShot.Abort();
            }
            catch (Exception e)
            {
                //Debug.Log(e);
            }
            foreach (PlayerData p in users.Values)
            {
                p.clientData.socket.Close();
            }
            server.listen.Stop();
        }

        public void ResetGame()
        {
            ResetMsg resetMsg = new ResetMsg();
            byte[] output = resetMsg.GetBytes();
            KeyValuePair<int, PlayerData>[] players = users.ToArray();
            foreach (KeyValuePair<int, PlayerData> playerData in players)
            {
                playerData.Value.playerControl.Shot();
                Send(output, playerData.Value.clientData.id);
                Debug.Log("RESET MSG SENT");
            }
            GameObject[] cubes = GameObject.FindGameObjectsWithTag("Net Cube");
            foreach (GameObject cube in cubes)
            {
                GameObject.Destroy(cube);
                //Destroy(cube);
                //Destroy(netobject.gameObject);
                //netobjects.Remove(netobject.objID);
            }
            netobjects = new Dictionary<int, Snapshot>();
        }
    }
}
