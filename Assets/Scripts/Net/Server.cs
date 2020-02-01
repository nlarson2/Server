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

namespace Server
{
    class Server
    {
        int port;
        TcpListener listen;

        int idCount; // which id to give
        int connectionCount; // counts how many are currently connected


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
   
        public Server(int port)
        {
            this.port = port;
            listen = new TcpListener(IPAddress.Any, port);
            idCount = 0; connectionCount = 0;
            clients = new Dictionary<Socket, ClientData>();
            messages = new Queue<Message>();
            
        }
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
                    Console.WriteLine("Waiting on clients");
                    client = listen.AcceptTcpClient();
                    socket = client.Client;
                    stream = client.GetStream();
                    ipAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
                    clientData = new ClientData(idCount++, socket, stream, ipAddress);
                    clients.Add(socket, clientData);
                    Console.WriteLine("Client Added");

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
                Console.WriteLine("SocketException: {0}", exception);
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
                        //Console.WriteLine("Reset Listener");
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
                        NetworkStream stream = clients[(Socket)listOfClients[i]].stream;
                        try {
                            while ((inData = stream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                message = System.Text.Encoding.ASCII.GetString(buffer, 0, inData);

                                Console.WriteLine("Received: {0}", message);
                                //message = message.ToUpper();

                                /************** FOR TESTING PURPOSES************************************************
                                Byte[] byteMessage = System.Text.Encoding.ASCII.GetBytes(message);
                                if (message.Length > 0)
                                {
                                    foreach (KeyValuePair<Socket, ClientData> data in clients)
                                    {
                                        //Console.WriteLine("Reset Listener");
                                        data.Value.stream.Write(byteMessage, 0, byteMessage.Length);
                                        Console.WriteLine("Sent: {0}", message);
                                    }
                                }***********************************************************************************/

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
                            Console.WriteLine("Socket Removed ID: {0}", id);
                            //Console.WriteLine("Exception {0}", exception);
                        }
                    }
                }
               
            }

        }


        static void Main(string[] args)
        {
            //Console.WriteLine("Hellow world 1");
            Server serv = new Server(55555);
            serv.StartServer();



            Console.Read();
        }
    }
}
