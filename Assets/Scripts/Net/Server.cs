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

namespace Server
{
    class Server : MonoBehaviour
    {
        int port;
        TcpListener listen;


        [SerializeField] int idCount; // which id to give
        [SerializeField] int connectionCount; // counts how many are currently connected


        protected Dictionary<Socket, ClientData> clients; //hashtable of clients
        //this data makes is so we know who is who and have ways of communication with them
        public class ClientData
        {
            public int id;
            public Socket socket;
            public NetworkStream stream;
            public IPAddress ipAddress;

            public ClientData(int id, Socket socket, NetworkStream stream, IPAddress ipAddress)
            {
                this.id = id; this.socket = socket; this.stream = stream; this.ipAddress = ipAddress;
            }
        }

        Queue<Message> messages;
   
        private void Start()
        {
            this.port = 50000;
            listen = new TcpListener(IPAddress.Any, port);
            Debug.Log("Listening on Port 55555");
            idCount = 0; connectionCount = 0;
            clients = new Dictionary<Socket, ClientData>();
            messages = new Queue<Message>();
            StartServer();
            
        }
        private void update() { }

        public void StartServer()
        {
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
            bool canRecieve =  false;
            try
            {
                while (true)
                {
                    if (connectionCount == 0) canRecieve = false;
                    Debug.Log("Waiting on clients");
                    client = listen.AcceptTcpClient();
                    socket = client.Client;
                    stream = client.GetStream();
                    ipAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
                    clientData = new ClientData(idCount++, socket, stream, ipAddress);
                    clients.Add(socket, clientData);
                    Debug.Log("Client Added");

                    //This is where the login notification will go*******************************
                    Message msg = new Message();
                    msg.SetMessage("Hello there friend");
                    byte[] message = msg.GetMessage();
                    stream.Write(message, 0, message.Length);

                    /*******************************************************************************/


                    connectionCount++;
                    //start thread to read in new data
                    if (canRecieve == false)
                    {
                        Thread thread = new Thread(ListenForMessage);
                        thread.Start();
                        canRecieve = true;
                    }

                }
            }
            catch (SocketException exception)
            {
                Debug.Log(String.Format("SocketException: {0}", exception.ToString()));
                listen.Stop();
            }
            
        }
        
        public void ListenForMessage()
        {
            
            Byte[] buffer;
            String message;
            //this of clients that we are listening for a message from
            ArrayList listOfClients;

            while (true)
            {

                if (clients.Count != 0)
                {
                    buffer = new Byte[256];
                    message = null;
                    listOfClients = new ArrayList();

                    foreach (KeyValuePair<Socket, ClientData> data in clients)
                    {
                        listOfClients.Add(data.Value.socket);
                        //Debug.Log("Reset Listener");
                    }


                    //this function works much like the select() from C/C++
                    //https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.select?view=netframework-4.8

                    Socket.Select(listOfClients, null, null, 100000);

                    //after select the only ClientDatas left are the ones that had sent a message
                    for (int i = 0; i < listOfClients.Count; i++)
                    {

                        //process Message
                        //https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcplistener?view=netframework-4.8

                        int inData;
                        try
                        {
                            NetworkStream stream = clients[(Socket)listOfClients[i]].stream;
                        
                            try
                            { 
                                while ((inData = stream.Read(buffer, 0, buffer.Length)) != 0)
                                {
                                    message = System.Text.Encoding.ASCII.GetString(buffer, 0, inData);

                                    Debug.Log(String.Format("Received: {0}", message));
                                    //message = message.ToUpper();

                                    /************** FOR TESTING PURPOSES************************************************/
                                    Byte[] byteMessage = System.Text.Encoding.ASCII.GetBytes(message);
                                    Queue<ClientData> remove = new Queue<ClientData>();
                                    if (message.Length > 0)
                                    {
                                        foreach (KeyValuePair<Socket, ClientData> data in clients)
                                        {
                                            try
                                            {
                                                //Debug.Log("Reset Listener");
                                                data.Value.stream.Write(byteMessage, 0, byteMessage.Length);
                                                Debug.Log(String.Format("Sent: {0}", message));
                                            }
                                            catch (Exception exception)
                                            {
                                                remove.Enqueue(data.Value);
                                                //clients.Remove(data.Value.socket);
                                            }
                                        }
                                        while (remove.Count > 0)
                                        {
                                            ClientData cd = remove.Dequeue();
                                            Debug.Log(String.Format("Socket Removed from Foreach ID: {0}", cd.id));
                                            clients.Remove(cd.socket);

                                            connectionCount--;
                                        }
                                    }/***********************************************************************************/

                                    //if there is not more data to be read exit
                                    if (!stream.DataAvailable)
                                    {
                                        //store message in queue
                                        break;
                                    }
                                }
                            }
                            catch (Exception exception)
                            {
                                int id = clients[(Socket)listOfClients[i]].id;
                                clients.Remove((Socket)listOfClients[i]);
                                Debug.Log(String.Format("Socket Removed from last catch ID: {0} {1}", id, exception));
                                //Debug.Log("Exception {0}", exception);
                            }
                        }
                        catch (KeyNotFoundException exception)
                        {
                            /*Debug.Log(String.Format("Socket Removed from Foreach ID: {0}", clients[(Socket)listOfClients[i]].id));
                            clients.Remove(clients[(Socket)listOfClients[i]].socket);*/
                        }
                    }
                }
               
            }

        }
        private void OnApplicationQuit()
        {
            listen.Stop();
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
