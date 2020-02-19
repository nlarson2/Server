using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace SmashDomeNetwork
{
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
}