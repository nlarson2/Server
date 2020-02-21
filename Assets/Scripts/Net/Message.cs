using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SmashDomeNetwork
{
    public enum MsgType
    {
        LOGIN = 1,
        LOGOUT = 2,
        MOVE = 3,
        SHOOT = 4,
        SNAPSHOT = 5,
        STRUCTURE = 6
    }
    public abstract class Message
    {

        protected DateTime time = DateTime.Now;
        public byte from;
        public byte to;
        public byte msgType;
        //protected byte[] msg; // used later when we move from json
        char delimiter = '\0';

        public byte[] constructMsg()
        {
            return null;
        }

        public byte[] GetMessage()
        {
            string json = JsonUtility.ToJson(this);
            Debug.Log(json);

            return System.Text.ASCIIEncoding.ASCII.GetBytes(json);
        }
    }

    public class LoginMsg : Message
    {
        //constructor
        public LoginMsg(byte to)
        {
            this.msgType = 1;
            this.to = to;
        }

    }
    public class LogoutMsg : Message
    {
        public LogoutMsg()
        {
            this.msgType = 2;

        }

    }
    public class MoveMsg : Message
    {
        public MoveMsg()
        {
            this.msgType = 3;
        }

    }
    public class ShootMsg : Message
    {
        public ShootMsg()
        {
            this.msgType = 4;
        }

 
    }
    public class SnapshotMsg : Message
    {
        public SnapshotMsg()
        {
            this.msgType = 5;
        }

    }
    public class StructureChange : Message
    {
        public StructureChange()
        {
            this.msgType = 6;
        }
        
    }

}