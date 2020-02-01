using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Message
    {
        protected static int id = 0;
        protected List<byte> message;
        protected byte[] textmessage;

        public Message()
        {
            message = new List<byte>();

            //store the id in the list
            message.Add((byte)(id >> 24));
            message.Add((byte)(id >> 16));
            message.Add((byte)(id >> 8));
            message.Add((byte)(id >> 0));
            id++;

        }

        public byte[] GetMessage()
        {

            foreach (byte b in textmessage)
            {
                this.message.Add(b);
            }
            addDelimiter();
            return message.ToArray();
        }

        public void SetMessage(string msg)
        {
            textmessage = System.Text.Encoding.ASCII.GetBytes(msg);
        }

        protected void addDelimiter()
        {
            this.message.Add(0);
            this.message.Add(0);
            this.message.Add(0);
            this.message.Add(0);
            this.message.Add(0);
            this.message.Add(0);
        }
    }

}