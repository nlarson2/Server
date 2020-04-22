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
using UnityEngine.SceneManagement;

namespace SmashDomeNetwork
{
    class Server
    {
        int port;
        int idCount;            // which id to give
        int connectionCount;    // counts how many are currently connected
        public TcpListener listen;

        //public Queue<ClientData> newUserData = new Queue<ClientData>();
        public Queue<byte[]> msgQueue = new Queue<byte[]>();
        public Dictionary<int, ClientData> connecting = new Dictionary<int, ClientData>();

        private List<Thread> threads = new List<Thread>();
        
        public Server(int port)
        {
            //Debug.Log("BUILDING SERVER");
            this.port = port; //50000
            try
            {
                listen = new TcpListener(IPAddress.Any, port);
            }
            catch
            {
                SceneManager.LoadScene(0);
            }
            //Debug.Log("Listening on Port "+port.ToString());
            idCount = 1; connectionCount = 0;
            
            //start thread to listen
            Thread thread = new Thread(ListenForConnections);
            thread.Start();
        }

        //Listen for new connection so that they can be stored in the clients table
        public void ListenForConnections()
        {
            listen.Start();

            TcpClient client;
            Socket socket;
            NetworkStream stream;
            IPAddress ipAddress;
            ClientData clientData;

            try
            {
                while (true)
                {
                    //Debug.Log("Waiting on clients");
                    client = listen.AcceptTcpClient();
                    socket = client.Client;
                    stream = client.GetStream();

                    ipAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
                    clientData = new ClientData(idCount++, socket, stream, ipAddress);
                    
                    connecting.Add(clientData.id, clientData);
                    //Debug.Log("Client Added");
                    connectionCount++;

                    //This is where the login notification will go*******************************
                    LoginMsg msg = new LoginMsg(clientData.id);
                    Debug.Log(string.Format("In Server: {0}", msg.from));
                    byte[] message = msg.GetBytes();
                    stream.Write(message, 0, message.Length);

                    threads.Add( new Thread(() => ReceiveMsg(clientData)));
                    threads[threads.Count-1].Start();
                }
            }
            catch (SocketException exception)
            {
                //Debug.Log(String.Format("SocketException: {0}", exception.ToString()));
                listen.Stop();
            }
            
        }
        
        protected void ReceiveMsg(ClientData client)
        {
            
            Byte[] buffer;
            String message;
            NetworkStream stream = client.stream;

            while (true)
            {
                try
                {
                    /*message = String.Empty;
                    int brackets = 0;
                    while (true)
                    {
                        buffer = new Byte[256];


                        int bytesReceived = stream.ReadByte();

                        if ((char)bytesReceived == '{')
                        {
                            brackets++;
                        }
                        if (brackets > 0)
                        {
                            message += (char)bytesReceived;
                        }
                        if ((char)bytesReceived == '}')
                        {
                            brackets--;
                            if (brackets == 0)
                                break;
                        }
                    }
                    msgQueue.Enqueue(message);
                    ////Debug.Log(message);*/
                    /*byte[] sizeInBytes =
                    {
                        (byte)stream.ReadByte(),
                        (byte)stream.ReadByte(),
                        (byte)stream.ReadByte(),
                        (byte)stream.ReadByte()

                    };*/
                    ////Debug.Log("GOT SIZE");
                    //int size = Message.BytesToInt(sizeInBytes);
                    ////Debug.Log(String.Format("SIZE: {0}", size));
                    //if (size < 0) continue;
                    //byte[] msg = new byte[size];
                    List<byte> byteList = new List<byte>();
                    int delimCount = 0;
                    /*for (int i = 0; i < size; i++)
                    {
                        msg[i] = (byte)stream.ReadByte();
                        //Debug.Log("READING");
                    }*/
                    while(delimCount<16)
                    {
                        byte inByte = (byte)stream.ReadByte();
                        byteList.Add(inByte);
                        delimCount = (char)inByte == '\n' ? delimCount + 1 : 0;                       
                    }
                    if(byteList.Count > 16)
                        msgQueue.Enqueue(byteList.ToArray());
                    //Debug.Log("ENQUEUED");
                }
                catch (SocketException e)
                {
                    //Debug.Log(e);
                }
                
            }

        }
        // Send message to single client
        public void SendMsg(ClientData client, byte[] msg)
        {
            try
            {
                client.stream.Write(msg, 0, msg.Length);
            }
            catch (SocketException e)
            {
                //Debug.Log(e.ToString());
            }
        }
        
        // Used for broadcasting messages regarding clients in their respective Quadrants (for future use)
        public void SendMsg(ClientData[] client, byte[] msg)
        {
            try
            {
                foreach (ClientData c in client)
                {
                    c.stream.Write(msg, 0, msg.Length);
                }
            }
            catch (SocketException e)
            {
                //Debug.Log(e.ToString());
            }
        }

        public bool MsgInQueue()
        {
            return msgQueue.Count > 0;
        }

        private void OnApplicationQuit()
        {
            foreach(Thread t in threads)
            {
                t.Interrupt();
                t.Abort();
            }
            listen.Stop();
        }
    }
}
