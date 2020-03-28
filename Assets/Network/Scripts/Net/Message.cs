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
        public Cerealize cc = new Cerealize();
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
            return cc.SerializeMSG(this);
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
        public Vector3 pos;
        public Quaternion playerRotation;
        public Quaternion cameraRotation;

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
        public Vector3 position;
        public Vector3 direction;
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
        public List<Quaternion> camRotation = new List<Quaternion>();
        public SnapshotMsg()
        {
            this.msgType = 6;
        }

    }
    public class StructureChangeMsg : Message
    {
        public Vector3 pos;
        public Vector3[] vertices;// = new List<Vector3>();
        public int[] triangles;// = new List<Vector3>();
        public StructureChangeMsg()
        {
            this.msgType = 7;
        }

    }

    public class AddPlayerMsg : Message
    {
        public int playerType;
        public AddPlayerMsg(int from)
        {
            this.msgType = 8;
            this.from = from;
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