using System;
using System.Collections.Generic;
using System.IO;
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
        ADDPLAYER = 8,
        NETOBJECT = 9,
        RESPAWN = 10,
        RESET = 11
    }

    public class Message
    {

        //protected DateTime time = DateTime.Now;
        public static int seq = 1;
        
        public static int snapSeq = 1;
        //default to 0 to avoid errors
        public int msgNum = 0;
        public int msgType = 0;
        public int from = 0;
        public int to = 0;
        public int playerType = 0;
        //protected byte[] msg; // used later when we move from json
        char delimiter = '\0';

        public byte[] constructMsg()
        {
            return null;
        }

        public virtual byte[] GetMessage()
        {
            string json = JsonUtility.ToJson(this);
            Debug.Log(json);

            return System.Text.ASCIIEncoding.ASCII.GetBytes(json);
        }

        public static int GetMsgType(byte[] msgType)
        {

            return 0;
        }

        //get the normal first components(msgnum, msgtype, to, from)
        public byte[] Base()
        {
            byte[] ret = IntToBytes(msgNum);
            ret = Join(ret, IntToBytes(msgType));
            ret = Join(ret, IntToBytes(to));
            ret = Join(ret, IntToBytes(from));
            return ret;
        }

        // used to append the size of the message to the front of the msg
        public byte[] FinishMsg(byte[] bytes)
        {
            byte[] delim = { (byte)'\n', (byte)'\n', (byte)'\n', (byte)'\n', (byte)'\n', (byte)'\n', (byte)'\n', (byte)'\n', (byte)'\n', (byte)'\n', (byte)'\n', (byte)'\n', (byte)'\n', (byte)'\n', (byte)'\n', (byte)'\n' };
            return Join(bytes, delim);
        }

        /***************CONVERSIONS*************/
        public static byte[] IntToBytes(int num)
        {
            byte[] bytes = BitConverter.GetBytes(num);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        public static int BytesToInt(byte[] bytes)
        {
            //Debug.Log(String.Format("NUMBYTES: {0}", bytes.Length));
            return BitConverter.ToInt32(bytes, 0);
        }

        public static byte[] FloatToBytes(float num)
        {
            num = num * 100;
            byte[] bytes = BitConverter.GetBytes((int)num);
            /*if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);*/
            return bytes;
        }

        public static float BytesToFloat(byte[] bytes)
        {
            //Debug.Log(String.Format("NUMBYTES: {0}", bytes.Length));
            float num = BitConverter.ToInt32(bytes, 0);
            return num / 100.0f;
        }

        public static byte[] Vec3ToBytes(UnityEngine.Vector3 vec)
        {
            byte[] bytes = new byte[12];
            float[] floatsOfVec = { vec.x, vec.y, vec.z };
            for (int i = 0; i < 3; i++)
            {
                byte[] tmp = FloatToBytes(floatsOfVec[i]);
                bytes[i * 4] = tmp[0];
                bytes[i * 4 + 1] = tmp[1];
                bytes[i * 4 + 2] = tmp[2];
                bytes[i * 4 + 3] = tmp[3];
            }

            return bytes;
        }

        public static Vector3 BytesToVec3(byte[] bytes)
        {
            byte[] tmp = new byte[4];
            float[] floats = new float[3];
            for (int i = 0; i < 3; i++)
            {
                tmp[0] = bytes[i * 4];
                tmp[1] = bytes[i * 4 + 1];
                tmp[2] = bytes[i * 4 + 2];
                tmp[3] = bytes[i * 4 + 3];
                floats[i] = BytesToFloat(tmp);
            }
            return new Vector3(floats[0], floats[1], floats[2]);
        }

        public static byte[] QuaternionToBytes(UnityEngine.Quaternion vec)
        {
            byte[] bytes = Vec3ToBytes(vec.eulerAngles);

            return bytes;
        }

        public static Quaternion BytesToQuaternion(byte[] bytes)
        {
            Debug.Log(Quaternion.Euler(BytesToVec3(bytes)));
            return Quaternion.Euler(BytesToVec3(bytes));

        }


        /*****byte manipulation********/
        public static byte[] GetSegment(int start, int count, byte[] bytes)
        {
            byte[] ret = new byte[count];
            for (int i = 0; i < count; i++)
            {
                ret[i] = bytes[start + i];
            }
            return ret;
        }
        public static byte[] Join(byte[] a, byte[] b)
        {
            int size = a.Length + b.Length;
            byte[] ret = new byte[size];
            for (int i = 0; i < size; i++)
            {
                ret[i] = (i < a.Length) ? a[i] : b[i - a.Length];
            }
            return ret;
        }

    }

    public class LoginMsg : Message
    {
        //constructor
        public int personType = 0;
        public LoginMsg(int from, int playerType = 0)
        {
            this.msgNum = seq++;
            //reset if it gets too high
            if (seq > 2000000000) { seq = 1; }
            this.msgType = 1;
            this.from = from;
            this.playerType = playerType;
        }
        public LoginMsg(byte[] bytes)
        {
            //start at 8 for all because first 8 are seq num and msgtype
            this.msgType = 1;
            int index = 8;
            this.to = BytesToInt(GetSegment(index, 4, bytes)); index += 4;
            this.from = BytesToInt(GetSegment(index, 4, bytes)); index += 4;
            this.playerType = BytesToInt(GetSegment(index, 4, bytes)); index += 4;
            this.personType = BytesToInt(GetSegment(index, 4, bytes)); index += 4;
        }
        public byte[] GetBytes()
        {
            byte[] msg = Base();
            msg = Join(msg, IntToBytes(playerType));
            msg = Join(msg, IntToBytes(personType));
            msg = FinishMsg(msg);
            return msg;
        }

    }
    public class LogoutMsg : Message
    {
        public LogoutMsg(int from)
        {
            this.msgNum = seq++;
            //reset if it gets too high
            if (seq > 2000000000) { seq = 1; }
            this.msgType = 2;
            this.from = from;

        }
        public LogoutMsg(byte[] bytes)
        {
            //start at 8 for all because first 8 are seq num and msgtype
            int index = 8;
            this.to = BytesToInt(GetSegment(index, 4, bytes)); index += 4;
            this.from = BytesToInt(GetSegment(index, 4, bytes)); index += 4;
        }
        public byte[] GetBytes()
        {
            byte[] msg = Base();
            msg = FinishMsg(msg);
            return msg;
        }

    }
    public class MoveMsg : Message
    {
        public Vector3 pos;
        public Quaternion playerRotation;
        public Quaternion cameraRotation;

        public MoveMsg(int from)
        {
            this.msgNum = seq++;
            //reset if it gets too high
            if (seq > 2000000000) { seq = 1; }
            this.msgType = 3;
            this.from = from;
            this.playerType = 1; // 1 = PC Player
        }
        public MoveMsg(byte[] bytes)
        {
            //start at 8 for all because first 8 are seq num and msgtype
            int index = 8;
            this.to = BytesToInt(GetSegment(index, 4, bytes)); index += 4;//4 bytes in int
            this.from = BytesToInt(GetSegment(index, 4, bytes)); index += 4;
            this.playerType = BytesToInt(GetSegment(index, 4, bytes)); index += 4;
            this.pos = BytesToVec3(GetSegment(index, 12, bytes)); index += 12;//12 bytes (3 floats)
            this.playerRotation = Quaternion.Euler(BytesToVec3(GetSegment(index, 12, bytes))); index += 12;//16 bytes (4 floats)
            this.cameraRotation = Quaternion.Euler(BytesToVec3(GetSegment(index, 12, bytes))); index += 12;
        }
        public byte[] GetBytes()
        {
            byte[] msg = Base();
            msg = Join(msg, IntToBytes(playerType));
            msg = Join(msg, Vec3ToBytes(this.pos));
            msg = Join(msg, Vec3ToBytes(this.playerRotation.eulerAngles));
            msg = Join(msg, Vec3ToBytes(this.cameraRotation.eulerAngles));
            msg = FinishMsg(msg);
            Debug.Log(msg.ToString());
            Debug.Log(JsonUtility.ToJson(this));
            return msg;
        }

    }

    public class MoveVRMsg : Message
    {
        public Vector3 pos;
        public Quaternion playerRotation;
        public Quaternion cameraRotation;
        public Vector3 lHandPosition, rHandPosition;
        public Quaternion lHandRotation, rHandRotation;
        public MoveVRMsg(int from)
        {
            this.msgNum = seq++;
            //reset if it gets too high
            if (seq > 2000000000) { seq = 1; }
            this.msgType = 4;
            this.from = from;
            this.playerType = 2;//2 = VR player
        }
        public MoveVRMsg(byte[] bytes)
        {
            //start at 8 for all because first 8 are seq num and msgtype
            int index = 8;
            this.to = BytesToInt(GetSegment(index, 4, bytes)); index += 4;//4 bytes in int
            this.from = BytesToInt(GetSegment(index, 4, bytes)); index += 4;
            this.playerType = BytesToInt(GetSegment(index, 4, bytes)); index += 4;
            this.pos = BytesToVec3(GetSegment(index, 12, bytes)); index += 12;//12 bytes (3 floats)
            this.playerRotation = Quaternion.Euler(BytesToVec3(GetSegment(index, 12, bytes))); index += 12;//16 bytes (4 floats)
            this.cameraRotation = Quaternion.Euler(BytesToVec3(GetSegment(index, 12, bytes))); index += 12;
            this.lHandPosition = BytesToVec3(GetSegment(index, 12, bytes)); index += 12;//12 bytes (3 floats)
            this.rHandPosition = BytesToVec3(GetSegment(index, 12, bytes)); index += 12;//12 bytes (3 floats)
            this.lHandRotation = Quaternion.Euler(BytesToVec3(GetSegment(index, 12, bytes))); index += 12;//16 bytes (4 floats)
            this.rHandRotation = Quaternion.Euler(BytesToVec3(GetSegment(index, 12, bytes))); index += 12;//16 bytes (4 floats)
        }
        public byte[] GetBytes()
        {
            byte[] msg = Base();
            msg = Join(msg, IntToBytes(playerType));
            msg = Join(msg, Vec3ToBytes(this.pos));
            msg = Join(msg, Vec3ToBytes(this.playerRotation.eulerAngles));
            msg = Join(msg, Vec3ToBytes(this.cameraRotation.eulerAngles));
            msg = Join(msg, Vec3ToBytes(this.lHandPosition));
            msg = Join(msg, Vec3ToBytes(this.rHandPosition));
            msg = Join(msg, Vec3ToBytes(this.lHandRotation.eulerAngles));
            msg = Join(msg, Vec3ToBytes(this.rHandRotation.eulerAngles));
            msg = FinishMsg(msg);
            Debug.Log("GOT MOVE BYTES");
            return msg;
        }

    }

    public class ShootMsg : Message
    {
        public Vector3 position;
        public Vector3 direction;
        public int shootType = 0;
        public Quaternion rotation;
        public ShootMsg(int from)
        {
            this.msgNum = seq++;
            //reset if it gets too high
            if (seq > 2000000000) { seq = 1; }
            this.msgType = 5;
            this.from = from;
        }
        public ShootMsg(byte[] bytes)
        {
            //start at 8 for all because first 8 are seq num and msgtype
            int index = 8;
            this.to = BytesToInt(GetSegment(index, 4, bytes)); index += 4;//4 bytes in int
            this.from = BytesToInt(GetSegment(index, 4, bytes)); index += 4;
            this.shootType = BytesToInt(GetSegment(index, 4, bytes)); index += 4;
            //this.playerType = BytesToInt(GetSegment(index, 4, bytes)); index += 4;
            this.position = BytesToVec3(GetSegment(index, 12, bytes)); index += 12;//12 bytes (3 floats)
            this.direction = BytesToVec3(GetSegment(index, 12, bytes)); index += 12;//12 bytes (3 floats)

            // UPDATE HERE
            // Took this from player message. I would think it'd need to be 16 because it's 4 floats, but the code in player rotation has 12 bytes. not sure why.
            // It's 3 floats because of Euler angles.
            this.rotation = Quaternion.Euler(BytesToVec3(GetSegment(index, 12, bytes))); index += 12;//12 bytes (3 floats)

        }
        public byte[] GetBytes()
        {
            byte[] msg = Base();
            msg = Join(msg, IntToBytes(this.shootType));
            msg = Join(msg, Vec3ToBytes(this.position));
            msg = Join(msg, Vec3ToBytes(this.direction));
            msg = Join(msg, Vec3ToBytes(this.rotation.eulerAngles));
            msg = FinishMsg(msg);
            return msg;
        }


    }
    public class SnapshotMsg : Message
    {
    	public int numId;
        public List<int> objID = new List<int>();
        public List<int> textureType = new List<int>();
        public List<Vector3> positions = new List<Vector3>();
        public List<Quaternion> rotation = new List<Quaternion>();
        public List<Vector3> linear_speed = new List<Vector3>();
        public List<Quaternion> angular_speed = new List<Quaternion>();

        
         public SnapshotMsg(int from)
        {
            this.msgNum = snapSeq++;
            if (snapSeq > 2000000000) { snapSeq = 1; }
            this.msgType = 6;
            this.from = from; //object ID?
        }

        public SnapshotMsg(byte[] bytes)
        {
            int index = 8;
            this.to = BytesToInt(GetSegment(index, 4, bytes)); index += 4;//4 bytes in int
            this.from = BytesToInt(GetSegment(index, 4, bytes)); index += 4;

            this.numId = BytesToInt(GetSegment(index, 4 , bytes)); index += 4; //retrieves size of list

            for (int i = 0; i < numId; i++) 
            {
                objID.Add(BytesToInt(GetSegment(index, 4 , bytes))); index += 4;
                textureType.Add(BytesToInt(GetSegment(index, 4, bytes))); index += 4;
                positions.Add(BytesToVec3(GetSegment(index, 12, bytes))); index += 12;//12 bytes (3 floats)
                rotation.Add(BytesToQuaternion(GetSegment(index, 12, bytes))); index += 12;//12 bytes (3 floats)
                linear_speed.Add(BytesToVec3(GetSegment(index, 12, bytes))); index += 12;//12 bytes (3 floats)
                angular_speed.Add(BytesToQuaternion(GetSegment(index, 12, bytes))); index += 12;//12 bytes (3 floats)
            }
        }

        public byte[] GetBytes()
        {
            byte[] msg = Base();
            msg = Join(msg, IntToBytes(objID.Count));
            for (int i = 0; i < objID.Count; i++)
            {
                msg = Join(msg, IntToBytes(objID[i]));
                msg = Join(msg, IntToBytes(textureType[i]));
                msg = Join(msg, Vec3ToBytes(positions[i]));
                msg = Join(msg, QuaternionToBytes(rotation[i]));
                msg = Join(msg, Vec3ToBytes(linear_speed[i]));
                msg = Join(msg, QuaternionToBytes(angular_speed[i]));
            }
            msg = FinishMsg(msg);
            return msg;
        }
    }
    [Serializable]
    public class StructureChangeMsg : Message
    {
        public Vector3 pos;
        public int verticeLength = 0;
        public int textureType = 0; // 0 defaults texture type to stone

        //using setter and getter to auto set length
        [SerializeField]
        protected Vector3[] vertices;
        public Vector3[] Vertices
        {
            get
            {
                return this.vertices;
            }
            set
            {
                vertices = value;
                verticeLength = vertices.Length;
            }
        }
        public int triangleLength = 0;
        //using setter and getter to auto set length
        [SerializeField]
        protected int[] triangles;
        public int[] Triangles
        {
            get
            {
                return triangles;
            }
            set
            {
                triangles = value;
                triangleLength = triangles.Length;
            }
        }
        public StructureChangeMsg()
        {
            this.msgNum = seq++;
            //reset if it gets too high
            if (seq > 2000000000) { seq = 1; }
            this.msgType = 7;
        }
        public StructureChangeMsg(byte[] bytes)
        {
            //start at 8 for all because first 8 are seq num and msgtype
            int index = 8;
            this.to = BytesToInt(GetSegment(index, 4, bytes)); index += 4;//4 bytes in int
            this.from = BytesToInt(GetSegment(index, 4, bytes)); index += 4;
            this.textureType = BytesToInt(GetSegment(index, 4, bytes)); index += 4;
            this.pos = BytesToVec3(GetSegment(index, 12, bytes)); index += 12;
            this.verticeLength = BytesToInt(GetSegment(index, 4, bytes)); index += 4;
            this.vertices = new Vector3[this.verticeLength];
            for (int i = 0; i < this.verticeLength; i++)
            {
                this.vertices[i] = BytesToVec3(GetSegment(index, 12, bytes)); index += 12;
            }
            this.triangleLength = BytesToInt(GetSegment(index, 4, bytes)); index += 4;
            this.triangles = new int[this.triangleLength];
            for (int i = 0; i < this.triangleLength; i++)
            {
                this.triangles[i] = BytesToInt(GetSegment(index, 4, bytes)); index += 4;
            }
        }
        public byte[] GetBytes()
        {
            byte[] msg = Base();
            msg = Join(msg, IntToBytes(this.textureType));
            msg = Join(msg, Vec3ToBytes(this.pos));
            msg = Join(msg, IntToBytes(this.verticeLength));

            for (int i = 0; i < this.verticeLength; i++)
            {
                msg = Join(msg, Vec3ToBytes(this.vertices[i]));
            }
            msg = Join(msg, IntToBytes(this.triangleLength));
            for (int i = 0; i < this.triangleLength; i++)
            {
                msg = Join(msg, IntToBytes(this.triangles[i]));
            }
            msg = FinishMsg(msg);
            Debug.Log($"Optimized Byte Length: {msg.Length})");
            byte[] output = System.Text.ASCIIEncoding.ASCII.GetBytes(JsonUtility.ToJson(this));
            
            Debug.Log($"JSON Byte Length: {output.Length}");
            return msg;
        }

    }

    public class AddPlayerMsg : Message
    {
        public int personType;
        public AddPlayerMsg(int from, int playerType, int personType)
        {
            this.msgNum = seq++;
            //reset if it gets too high
            if (seq > 2000000000) { seq = 1; }
            this.msgType = 8;
            this.from = from;
            this.playerType = playerType;
            this.personType = personType;
        }
        public AddPlayerMsg(byte[] bytes)
        {
            //start at 8 for all because first 8 are seq num and msgtype
            int index = 8;
            this.to = BytesToInt(GetSegment(index, 4, bytes)); index += 4;//4 bytes in int
            this.from = BytesToInt(GetSegment(index, 4, bytes)); index += 4;
            this.playerType = BytesToInt(GetSegment(index, 4, bytes)); index += 4;
            this.personType = BytesToInt(GetSegment(index, 4, bytes)); index += 4;

        }
        public byte[] GetBytes()
        {
            byte[] msg = Base();
            msg = Join(msg, IntToBytes(this.playerType));
            msg = Join(msg, IntToBytes(this.personType));
            msg = FinishMsg(msg);
            return msg;
        }
    }

    public class NetObjectMsg : Message
    {
        public int numId;
        public int objID;
        public int textureType;
        public Vector3 localScale;
        public Vector3 positions;
        public Quaternion rotation;

        public NetObjectMsg(int objID)
        {
            this.msgNum = seq++;
            if (seq > 2000000000) { seq = 1; }
            this.msgType = 9;
            this.from = objID; //object ID
        }

        public NetObjectMsg(byte[] bytes)
        {
            int index = 8;
            this.to = BytesToInt(GetSegment(index, 4, bytes)); index += 4;//4 bytes in int
            this.from = BytesToInt(GetSegment(index, 4, bytes)); index += 4;

            //this.numId = BytesToInt(GetSegment(index, 4, bytes)); index += 4; //retrieves size of list

            objID = (BytesToInt(GetSegment(index, 4, bytes))); index += 4;
            textureType = (BytesToInt(GetSegment(index, 4, bytes))); index += 4;
            positions = (BytesToVec3(GetSegment(index, 12, bytes))); index += 12;//12 bytes (3 floats)
            rotation = (BytesToQuaternion(GetSegment(index, 12, bytes))); index += 12;//12 bytes (3 floats)
        }

        public byte[] GetBytes()
        {
            byte[] msg = Base();
            
            //Debug.Log(string.Format("IN THE MESSAGE {0}", this.objID));
            msg = Join(msg, IntToBytes(objID));
            msg = Join(msg, IntToBytes(textureType));
            msg = Join(msg, Vec3ToBytes(positions));
            msg = Join(msg, QuaternionToBytes(rotation));
            msg = FinishMsg(msg);
            return msg;
        }

    }

    public class RespawnMsg : Message
    {
        public Vector3 pos;

        public RespawnMsg()
        {
            this.msgNum = seq++;
            //reset if it gets too high
            if (seq > 2000000000) { seq = 1; }
            this.msgType = 10;
        }
        public RespawnMsg(byte[] bytes)
        {
            //start at 8 for all because first 8 are seq num and msgtype
            int index = 8;
            this.to = BytesToInt(GetSegment(index, 4, bytes)); index += 4;//4 bytes in int
            this.from = BytesToInt(GetSegment(index, 4, bytes)); index += 4;
            this.pos = BytesToVec3(GetSegment(index, 12, bytes)); index += 12;//12 bytes (3 floats)
        }
        public byte[] GetBytes()
        {
            byte[] msg = Base();
            msg = Join(msg, Vec3ToBytes(this.pos));
            msg = FinishMsg(msg);
            return msg;
        }
    }

    public class ResetMsg : Message
    {
        public ResetMsg()
        {
            this.msgNum = seq++;
            //reset if it gets too high
            if (seq > 2000000000) { seq = 1; }
            this.msgType = 11;
        }
        public ResetMsg(byte[] bytes)
        {
            //start at 8 for all because first 8 are seq num and msgtype
            int index = 8;
            this.to = BytesToInt(GetSegment(index, 4, bytes)); index += 4;//4 bytes in int
            this.from = BytesToInt(GetSegment(index, 4, bytes)); index += 4;
        }
        public byte[] GetBytes()
        {
            byte[] msg = Base();
            msg = FinishMsg(msg);
            return msg;
        }
    }
}

