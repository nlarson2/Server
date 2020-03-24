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

        public Cerealize cc = new Cerealize();
        
        public Server(int port)
        {
            Debug.Log("BUILDING SERVER");
            this.port = port; //50000
            listen = new TcpListener(IPAddress.Any, port);
            Debug.Log("Listening on Port "+port.ToString());
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
                    Debug.Log("Waiting on clients");
                    client = listen.AcceptTcpClient();
                    socket = client.Client;
                    stream = client.GetStream();

                    ipAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
                    clientData = new ClientData(idCount++, socket, stream, ipAddress);
                    
                    connecting.Add(clientData.id, clientData);
                    Debug.Log("Client Added");
                    connectionCount++;

                    //This is where the login notification will go*******************************
                    LoginMsg msg = new LoginMsg((byte)clientData.id);
                    byte[] message = msg.GetMessage();
                    stream.Write(message, 0, message.Length);

                    threads.Add( new Thread(() => ReceiveMsg(clientData)));
                    threads[threads.Count-1].Start();
                }
            }
            catch (SocketException exception)
            {
                Debug.Log(String.Format("SocketException: {0}", exception.ToString()));
                listen.Stop();
            }
            
        }
        
        protected void ReceiveMsg(ClientData client)
        {
            
            byte[] buffer;
            byte[] message;
            NetworkStream stream = client.stream;

            while (true)
            {
                try
                {
                    while (true)
                    {
                        buffer = new byte[8];
                        //reads first 8 bytes
                        for (int i = 0; i < 8; i++)
                        {
                            buffer[i] = (byte)(char)stream.ReadByte();
                        }
                        //saves 8 bytes into Int64/long
                        Int64 msgSize = cc.ByteInt64(buffer);
                        //creates buffer with msg size minus 8 bytes which we add later
                        buffer = new byte[msgSize - 8];

                        //this adds the first 8 bytes into the buffer at the beginning
                        buffer = Cerealize.Combine(cc.IntByte(msgSize), buffer);

                        //this reads the rest of the message into new buffer
                        for (int j = 8; j < msgSize; j++)
                        {
                            buffer[j] = (byte)(char)stream.ReadByte();
                        }
                        message = buffer;
                        break;
                    }
                    msgQueue.Enqueue(message);
                    //Debug.Log(message);
                }
                catch (SocketException e)
                {
                    Debug.Log(e);
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
                Debug.Log(e.ToString());
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
                Debug.Log(e.ToString());
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
