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
        MOVEVR = 4,
        SHOOT = 5,
        SNAPSHOT = 6,
        STRUCTURE = 7,
        ADDPLAYER = 8
    }
    public class Message
    {

        protected DateTime time = DateTime.Now;
        public int from;
        public int to;
        public int msgType;
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
        public LoginMsg(int from)
        {
            this.msgType = 1;
            this.from = from;
        }

    }
    public class LogoutMsg : Message
    {
        public LogoutMsg(int from)
        {
            this.msgType = 2;
            this.from = from;

        }

    }
    public class MoveMsg : Message
    {
        public float x, y, z;
        public float xr, yr, zr, wr;

        public MoveMsg(int from)
        {
            this.msgType = 3;
            this.from = from;
        }

    }

    public class MoveVRMsg : Message
    {
        public MoveVRMsg(int from)
        {
            this.msgType = 4;
            this.from = from;
        }

    }

    public class ShootMsg : Message
    {
        public ShootMsg(int from)
        {
            this.msgType = 5;
            this.from = from;
        }


    }
    public class SnapshotMsg : Message
    {
        public List<int> userId = new List<int>();
        public List<Vector3> positions = new List<Vector3>();
        public List<Quaternion> rotation = new List<Quaternion>();
        public SnapshotMsg()
        {
            this.msgType = 6;
        }

    }
    public class StructureChange : Message
    {
        public StructureChange()
        {
            this.msgType = 7;
        }

    }

    public class AddPlayer : Message
    {
        public int playerType;
        public AddPlayer(int playerType)
        {
            this.msgType = 8;
            this.playerType = playerType;
        }
    }



    public class TestMsg : Message
    {
        public int[] stuff = new int[50];

        public TestMsg() { }

        public void Setup()
        {
            for(int i = 0; i < 50; i++)
            {
                stuff[i] = i;
            }
        }
        public void print()
        {
            for (int i = 0; i < 50; i++)
            {
                Debug.Log(stuff[i]);
            }
        }
    }

    public class BigTest : Message
    {
        //public List<Vector3> msgs = new List<Vector3>();
        public List<int> userId = new List<int>();
        public List<Vector3> positions = new List<Vector3>();
        public List<Quaternion> rotation = new List<Quaternion>();
        public BigTest() { }

        public void Setup()
        {
            userId.Add(5);
            positions.Add(new Vector3(1, 1, 1));
            rotation.Add(Quaternion.identity);

            userId.Add(3);
            positions.Add(new Vector3(3, 3, 3));
            rotation.Add(Quaternion.identity);
            
        }
        public void print()
        {
            for(int i = 0; i < userId.Count; i++)
            {
                Debug.Log(userId[i]);
            }
        }


    }


}