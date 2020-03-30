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

        //protected DateTime time = DateTime.Now;
        public static int seq = 1;
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
            byte[] delim = { (byte)'\n', (byte)'\n', (byte)'\n', (byte)'\n', (byte)'\n', (byte)'\n', (byte)'\n', (byte)'\n' };
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
            num = num * 10;
            byte[] bytes = BitConverter.GetBytes((int)num);
            /*if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);*/
            return bytes;
        }

        public static float BytesToFloat(byte[] bytes)
        {
            //Debug.Log(String.Format("NUMBYTES: {0}", bytes.Length));
            float num = BitConverter.ToInt32(bytes, 0);
            return num / 10.0f;
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
            int index = 8;
            this.to = BytesToInt(GetSegment(index, 4, bytes)); index += 4;
            this.from = BytesToInt(GetSegment(index, 4, bytes)); index += 4;
            this.playerType = BytesToInt(GetSegment(index, 4, bytes)); index += 4;
        }
        public byte[] GetBytes()
        {
            byte[] msg = Base();
            msg = Join(msg, IntToBytes(playerType));
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
            Debug.Log("GOT MOVE BYTES");
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
            this.playerType = BytesToInt(GetSegment(index, 4, bytes)); index += 4;
            this.position = BytesToVec3(GetSegment(index, 12, bytes)); index += 12;//12 bytes (3 floats)
            this.direction = BytesToVec3(GetSegment(index, 12, bytes)); index += 12;//12 bytes (3 floats)

        }
        public byte[] GetBytes()
        {
            byte[] msg = Base();
            msg = Join(msg, Vec3ToBytes(this.position));
            msg = Join(msg, Vec3ToBytes(this.direction));
            msg = FinishMsg(msg);
            return msg;
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
        public int verticeLength = 0;
        //using setter and getter to auto set length
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
            return msg;
        }

    }

    public class AddPlayerMsg : Message
    {
        public AddPlayerMsg(int from, int playerType)
        {
            this.msgNum = seq++;
            //reset if it gets too high
            if (seq > 2000000000) { seq = 1; }
            this.msgType = 8;
            this.from = from;
            this.playerType = playerType;
        }
        public AddPlayerMsg(byte[] bytes)
        {
            //start at 8 for all because first 8 are seq num and msgtype
            int index = 8;
            this.to = BytesToInt(GetSegment(index, 4, bytes)); index += 4;//4 bytes in int
            this.from = BytesToInt(GetSegment(index, 4, bytes)); index += 4;
            this.playerType = BytesToInt(GetSegment(index, 4, bytes)); index += 4;

        }
        public byte[] GetBytes()
        {
            byte[] msg = Base();
            msg = Join(msg, IntToBytes(this.playerType));
            msg = FinishMsg(msg);
            return msg;
        }
    }
}