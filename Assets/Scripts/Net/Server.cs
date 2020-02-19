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
        static int count = 0; //test stuff
        int port;
        int idCount;            // which id to give
        int connectionCount;    // counts how many are currently connected
        public TcpListener listen;

        Queue<ClientData> newUserData = new Queue<ClientData>();
        Queue<String> msgQueue = new Queue<String>();
        
        public Server(int port)
        {
            Debug.Log("BUILDING SERVER");
            this.port = port; //50000
            listen = new TcpListener(IPAddress.Any, port);
            Debug.Log("Listening on Port 50000");
            idCount = 0; connectionCount = 0;
            count++;
            //start thread to listen
            Thread thread = new Thread(ListenForConnections);
            thread.Start();
        }

        //Listen for new connection so that they can be stored in the clients table
        public void ListenForConnections()
        {
            Debug.Log(count);
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
                    newUserData.Enqueue(clientData);
                    Debug.Log("Client Added");
                    connectionCount++;

                    //This is where the login notification will go*******************************
                    Message msg = new LoginMsg((byte)clientData.id);
                    byte[] message = msg.GetMessage();
                    stream.Write(message, 0, message.Length);

                    Thread thread = new Thread(() => ReceiveMsg(clientData));
                    thread.Start();
                }
            }
            catch (SocketException exception)
            {
                Debug.Log(String.Format("SocketException: {0}", exception.ToString()));
                listen.Stop();
            }
            
        }
        
        public void ReceiveMsg(ClientData client)
        {
            
            Byte[] buffer;
            String message;
            NetworkStream stream = client.stream;

            while (true)
            {

                while (true)
                {
                    buffer = new Byte[256];
                    message = String.Empty;


                    int bytesReceived = stream.ReadByte();
                    message += (char) bytesReceived;
                    if (message.IndexOf("}") > -1)
                    {
                        break;
                    }
                }
                //msgQueue.Enqueue(message);
                Debug.Log(message);
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

        private void OnApplicationQuit()
        {
            listen.Stop();
        }
    }
}
