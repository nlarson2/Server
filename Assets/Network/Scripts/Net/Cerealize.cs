using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text;

namespace SmashDomeNetwork
{ 
    public class Cerealize
    {
        public int[] sequence = { 0, 0, 0, 0, 0, 0, 0, 0 };
        public Cerealize()
        {

        }

        // Serialization Functions //////////////////
        public byte[] SerializeMSG(Message msg)
        {
            byte[] header = Header(msg.msgType);
            byte[] body = BuildMSG(msg.from, msg.to);
            return Combine(IntByte((Int32)4 + header.Length + body.Length), header, body);
        }
        public byte[] SerializeMSG(LoginMsg msg)
        {
            byte[] header = Header(msg.msgType);
            byte[] body = BuildMSG(msg.from);
            return Combine(IntByte((Int32)4 + header.Length + body.Length), header, body);
        }
        public byte[] SerializeMSG(LogoutMsg msg)
        {
            byte[] header = Header(msg.msgType);
            byte[] body = BuildMSG(msg.from);
            return Combine(IntByte((Int32)4 + header.Length + body.Length), header, body);
        }
        public byte[] SerializeMSG(MoveMsg msg)
        {
            byte[] header = Header(msg.msgType);
            byte[] body = BuildMSG(msg.from, msg.pos,
                                    msg.playerRotation,
                                    msg.cameraRotation);
            return Combine(IntByte((Int32)4 + header.Length + body.Length), header, body);
        }
        public byte[] SerializeMSG(MoveVRMsg msg)
        {
            byte[] header = Header(msg.msgType);
            byte[] body = BuildMSG(msg.from);
            return Combine(IntByte((Int32)4 + header.Length + body.Length), header, body);
        }
        public byte[] SerializeMSG(ShootMsg msg)
        {
            byte[] header = Header(msg.msgType);
            byte[] body = BuildMSG(msg.from);
            return Combine(IntByte((Int32)4 + header.Length + body.Length), header, body);
        }
        public byte[] SerializeMSG(SnapshotMsg msg)
        {
            byte[] header = Header(msg.msgType);
            byte[] body = BuildMSG(msg.from, msg.to, msg.positions,
                                    msg.rotation,
                                    msg.camRotation);
            return Combine(IntByte((Int32)4 + header.Length + body.Length), header, body);
        }
        public byte[] SerializeMSG(StructureChangeMsg msg)
        {
            byte[] header = Header(msg.msgType);
            byte[] body = BuildMSG(msg.pos, msg.vertices, msg.triangles);
            return Combine(IntByte((Int32)4 + header.Length + body.Length), header, body);
        }
        public byte[] SerializeMSG(AddPlayerMsg msg)
        {
            byte[] header = Header(msg.msgType);
            byte[] body = BuildMSG(msg.playerType);
            return Combine(IntByte((Int32)4 + header.Length + body.Length), header, body);
        }
        public byte[] SerializeMSG(TestMsg msg)
        {
            byte[] header = Header(msg.msgType);
            byte[] body = BuildMSG(msg.stuff);
            return Combine(IntByte((Int32)4 + header.Length + body.Length), header, body);
        }
        public byte[] SerializeMSG(BigTest msg)
        {
            byte[] header = Header(msg.msgType);
            byte[] body = BuildMSG(msg.userId, msg.positions, msg.rotation);
            return Combine(IntByte((Int32)4 + header.Length + body.Length), header, body);
        }

        // Construction Processing Functions ///////////
        private byte[] Header(Int32 msgType)
        {
            //Console.WriteLine(Convert.ToString(IntByte(msgType)[0], 2).PadLeft(8, '0'));
            //Console.WriteLine(Convert.ToString(IntByte(msgType)[1], 2).PadLeft(8, '0'));
            //Console.WriteLine(Convert.ToString(IntByte(msgType)[2], 2).PadLeft(8, '0'));
            //Console.WriteLine(Convert.ToString(IntByte(msgType)[3], 2).PadLeft(8, '0'));

            return Combine(IntByte(msgType)[0], Sequence(msgType));
        }
        private byte[] Sequence(Int32 msgType)
        {
            return IntByte(sequence[msgType++]);
        }
        //private byte[] BuildMSG(float pos, String word, Char letter)
        //{
        //    Int32 posi = (Int32)(pos * 1000);

        //    byte[] posb = BitConverter.GetBytes(posi);
        //    byte[] wordb = Encoding.ASCII.GetBytes(word);
        //    byte letterb = (byte)letter;

        //    return Combine(Combine(posb, wordb), letterb);
        //}
        private byte[] BuildMSG(Int32 from, Int32 to)
        {
            byte[] fromB = IntByte(from);
            byte[] toB = IntByte(to);
            byte[] body = Combine(fromB, toB);
            return body;
        }
        private byte[] BuildMSG(Int32 from) //also used for AddPlayerMsg
        {
            return IntByte(from);
        }
        private byte[] BuildMSG(Int32 from, Vector3 pos, Quaternion pR, Quaternion cR)
        {
            byte[] fromB = IntByte(from);
            byte[] posB = Combine(FByte(pos.x), FByte(pos.y), FByte(pos.z));
            byte[] pRB = Combine(FByte(pR.w), FByte(pR.x), FByte(pR.y), FByte(pR.z));
            byte[] cRB = Combine(FByte(cR.w), FByte(cR.x), FByte(cR.y), FByte(cR.z));
            byte[] body = Combine(fromB, posB, pRB, cRB);
            return body;
        }
        private byte[] BuildMSG(Int32 from, Int32 to, List<Vector3> pos, List<Quaternion> pR, List<Quaternion> cR) //needs work
        {
            byte[] fromB = IntByte(from);
            byte[] toB = IntByte(to);
            byte[] posB = Vec3Byte(pos);
            byte[] pRB = QuatByte(pR);
            byte[] pRBSize = IntByte(pRB.Length);
            byte[] cRB = QuatByte(cR);
            byte[] cRBSize = IntByte(cRB.Length);
            byte[] body = Combine(fromB, toB, posB, pRBSize, pRB, cRBSize, cRB);
            return body;
        }
        private byte[] BuildMSG(Vector3 pos, Vector3[] vertices, Int32[] triangles)
        {
            byte[] posB = Combine(FByte(pos.x), FByte(pos.y), FByte(pos.x));
            byte[] verticiesB = Vec3Byte(vertices);
            byte[] trianglesB = IntByte(triangles);
            byte[] body = Combine(posB,
                                  IntByte((Int32)verticiesB.Length),
                                  verticiesB,
                                  IntByte((Int32)trianglesB.Length),
                                  trianglesB);
            return body;
        }
        private byte[] BuildMSG(Int32[] stuff)
        {
            return IntByte(stuff);
        }
        private byte[] BuildMSG(List<int> userID, List<Vector3> position, List<Quaternion> rotation)
        {
            byte[] userIDB = IntByte(userID);
            byte[] positionB = Vec3Byte(position);
            byte[] rotationB = QuatByte(rotation);
            byte[] body = Combine(userIDB, positionB, rotationB);
            return body;
        }

        // Deserialization Functions
        public Message DeserializeMSG(byte[] msg)
        {
            Int32 msgSize = checkMSG(msg);
            if (msgSize != 0)
            {
                return GetMSG(msg, msgSize);
            }
            else
                return null;
        }
        public MoveMsg DeserializeMMSG(byte[] msg)
        {
            Int32 msgSize = checkMSG(msg);
            if (msgSize != 0)
            {
                return GetMMSG(msg, msgSize);
            }
            return null;
        }
        public MoveVRMsg DeserializeMVRMSG(byte[] msg)
        {
            Int32 msgSize = checkMSG(msg);
            if (msgSize != 0)
            {
                return GetMVRMSG(msg, msgSize);
            }
            return null;
        }
        public LoginMsg DeserializeLiMSG(byte[] msg)
        {
            Int32 msgSize = checkMSG(msg);
            if (msgSize != 0)
            {
                return GetLiMSG(msg, msgSize);
            }
            return null;
        }
        public LogoutMsg DeserializeLoMSG(byte[] msg)
        {
            Int32 msgSize = checkMSG(msg);
            if (msgSize != 0)
            {
                return GetLoMSG(msg, msgSize);
            }
            return null;
        }
        public ShootMsg DeserializeSMSG(byte[] msg)
        {
            Int32 msgSize = checkMSG(msg);
            if (msgSize != 0)
            {
                return GetSMSG(msg, msgSize);
            }
            return null;
        }
        public SnapshotMsg DeserializeSsMSG(byte[] msg)
        {
            Int32 msgSize = checkMSG(msg);
            if (msgSize != 0)
            {
                return GetSsMSG(msg, msgSize);
            }
            return null;
        }
        public StructureChangeMsg DeserializeSCMSG(byte[] msg)
        {
            Int32 msgSize = checkMSG(msg);
            if (msgSize != 0)
            {
                return GetSCMSG(msg, msgSize);
            }
            return null;
        }
        public AddPlayerMsg DeserializeAPMSG(byte[] msg)
        {
            Int32 msgSize = checkMSG(msg);
            if (msgSize != 0)
            {
                return GetAPMSG(msg, msgSize);
            }
            return null;
        }
        public TestMsg DeserializeTMSG(byte[] msg)
        {
            Int32 msgSize = checkMSG(msg);
            if (msgSize != 0)
            {
                return GetTMSG(msg, msgSize);
            }
            return null;
        }
        public BigTest DeserializeBTMSG(byte[] msg)
        {
            Int32 msgSize = checkMSG(msg);
            if (msgSize != 0)
            {
                return GetBTMSG(msg, msgSize);
            }
            return null;
        }
        // Deconstruction Processing Functions
        private Message GetMSG(byte[] msg, Int32 msgSize)
        {
            //Console.WriteLine(msgSize);
            Int32 type = ByteInt32(msg[8]);
            byte[] seq_num = new byte[4];
            byte[] vars = new byte[msgSize - 13];

            Array.Copy(msg, 9, seq_num, 0, 4);
            Array.Copy(msg, 13, vars, 0, msgSize - 13);

            Message MSG = MSGVars(vars);
            MSG.msgType = type;
            return MSG;
        }
        private Message MSGVars(byte[] vars)
        {
            byte[] from = new byte[4];
            byte[] to = new byte[4];

            int index = 0;
            //Console.WriteLine(vars.Length);
            Array.Copy(vars, index, from, 0, 4);
            index += 4;
            Array.Copy(vars, index, to, 0, 4);
            index += 4;

            Message msg = new Message();
            msg.from = ByteInt32(from);
            msg.to = ByteInt32(to);

            return msg;
        }
        // Move MSG
        private MoveMsg GetMMSG(byte[] msg, Int32 msgSize)
        {
            Int32 type = ByteInt32(msg[8]);
            byte[] seq_num = new byte[4];
            byte[] vars = new byte[msgSize - 13];

            Array.Copy(msg, 9, seq_num, 0, 4);
            Array.Copy(msg, 13, vars, 0, msgSize - 13);

            MoveMsg MMSG = MMSGVars(vars);
            MMSG.msgType = type;
            return MMSG;
        }
        private MoveMsg MMSGVars(byte[] vars)
        {
            byte[] from = new byte[4];
            byte[] pos = new byte[12];
            byte[] pR = new byte[16];
            byte[] cR = new byte[16];

            int index = 0;
            //Console.WriteLine(vars.Length);
            Array.Copy(vars, index, from, 0, 4);
            index += 4;
            Array.Copy(vars, index, pos, 0, 12);
            index += 12;
            Array.Copy(vars, index, pR, 0, 16);
            index += 16;
            Array.Copy(vars, index, cR, 0, 16);

            MoveMsg msg = new MoveMsg(ByteInt32(from));
            msg.pos = ByteVec3(pos);
            msg.playerRotation = ByteQuat(pR);
            msg.cameraRotation = ByteQuat(cR);

            return msg;
        }
        // Login MSG
        private LoginMsg GetLiMSG(byte[] msg, Int32 msgSize)
        {
            Int32 type = ByteInt32(msg[8]);
            byte[] seq_num = new byte[4];
            byte[] vars = new byte[msgSize - 13];

            Array.Copy(msg, 9, seq_num, 0, 4);
            Array.Copy(msg, 13, vars, 0, msgSize - 13);

            LoginMsg LiMSG = LiMSGVars(vars);
            LiMSG.msgType = type;
            return LiMSG;
        }
        private LoginMsg LiMSGVars(byte[] from)
        {
            LoginMsg msg = new LoginMsg(ByteInt32(from));

            return msg;
        }
        // Logout MSG
        private LogoutMsg GetLoMSG(byte[] msg, Int32 msgSize)
        {
            Int32 type = ByteInt32(msg[8]);
            byte[] seq_num = new byte[4];
            byte[] vars = new byte[msgSize - 13];

            Array.Copy(msg, 9, seq_num, 0, 4);
            Array.Copy(msg, 13, vars, 0, msgSize - 13);

            LogoutMsg LoMSG = LoMSGVars(vars);
            LoMSG.msgType = type;
            return LoMSG;
        }
        private LogoutMsg LoMSGVars(byte[] from)
        {
            LogoutMsg msg = new LogoutMsg(ByteInt32(from));

            return msg;
        }
        private MoveVRMsg GetMVRMSG(byte[] msg, Int32 msgSize)
        {
            Int32 type = ByteInt32(msg[8]);
            byte[] seq_num = new byte[4];
            byte[] vars = new byte[msgSize - 13];

            Array.Copy(msg, 9, seq_num, 0, 4);
            Array.Copy(msg, 13, vars, 0, msgSize - 13);

            MoveVRMsg MVRMSG = MVRMSGVars(vars);
            MVRMSG.msgType = type;
            return MVRMSG;
        }
        private MoveVRMsg MVRMSGVars(byte[] from)
        {
            MoveVRMsg msg = new MoveVRMsg(ByteInt32(from));

            return msg;
        }
        // Shoot MSG
        private ShootMsg GetSMSG(byte[] msg, Int32 msgSize)
        {
            Int32 type = ByteInt32(msg[8]);
            byte[] seq_num = new byte[4];
            byte[] vars = new byte[msgSize - 13];

            Array.Copy(msg, 9, seq_num, 0, 4);
            Array.Copy(msg, 13, vars, 0, msgSize - 13);

            ShootMsg SMSG = SMSGVars(vars);
            SMSG.msgType = type;
            return SMSG;
        }
        private ShootMsg SMSGVars(byte[] from)
        {
            ShootMsg msg = new ShootMsg(ByteInt32(from));

            return msg;
        }
        // Snapshot MSG
        private SnapshotMsg GetSsMSG(byte[] msg, Int32 msgSize)
        {
            Int32 type = ByteInt32(msg[8]);
            byte[] seq_num = new byte[4];
            byte[] vars = new byte[msgSize - 13];

            Array.Copy(msg, 9, seq_num, 0, 4);
            Array.Copy(msg, 13, vars, 0, msgSize - 13);

            SnapshotMsg SsMSG = SsMSGVars(vars);
            SsMSG.msgType = type;
            return SsMSG;
        }
        private SnapshotMsg SsMSGVars(byte[] vars) //needs work
        {
            byte[] from = new byte[4];
            byte[] pos = new byte[12];
            byte[] pR = new byte[16];
            byte[] cR = new byte[16];

            int index = 0;
            //Console.WriteLine(vars.Length);
            Array.Copy(vars, index, from, 0, 4);
            index += 4;
            Array.Copy(vars, index, pos, 0, 12);
            index += 12;
            Array.Copy(vars, index, pR, 0, 16);
            index += 16;
            Array.Copy(vars, index, cR, 0, 16);

            SnapshotMsg msg = new SnapshotMsg();

            return msg;
        }
        // Structure Change MSG
        private StructureChangeMsg GetSCMSG(byte[] msg, Int32 msgSize)
        {
            Int32 type = ByteInt32(msg[4]);
            byte[] seq_num = new byte[4];
            byte[] vars = new byte[msgSize - 13];

            Array.Copy(msg, 9, seq_num, 0, 4);
            Array.Copy(msg, 13, vars, 0, msgSize - 13);

            StructureChangeMsg SCMSG = SCMSGVars(vars);
            SCMSG.msgType = type;
            return SCMSG;
        }
        private StructureChangeMsg SCMSGVars(byte[] vars) //needs testing
        {
            byte[] size = new byte[4];
            byte[] pos = new byte[12];

            int index = 0;
            //Console.WriteLine(vars.Length);

            Array.Copy(vars, index, pos, 0, 12);
            index += 12;

            Array.Copy(vars, index, size, 0, 4);
            index += 4;

            Int32 vertSize = ByteInt32(size);
            byte[] vertices = new byte[vertSize];

            Array.Copy(vars, index, vertices, 0, vertSize);
            index += vertSize;

            Array.Copy(vars, index, size, 0, 4);
            index += 4;

            Int32 triSize = ByteInt32(size);
            byte[] triangles = new byte[triSize];

            Array.Copy(vars, index, triangles, 0, triSize);
            index += triSize;

            StructureChangeMsg msg = new StructureChangeMsg();

            msg.pos = ByteVec3(pos);

            msg.vertices = ByteVec3Array(vertices);


            byte[][] tri = new byte[vars.Length / 4][];

            //Console.WriteLine(vars.Length);
            for (int i = 0; i < vars.Length / 4; i += 4)
            {
                Array.Copy(vars, i, tri[i], 0, 4);
            }

            msg.triangles = ByteInt32(tri);
            return msg;
        }
        // Add Player MSG
        private AddPlayerMsg GetAPMSG(byte[] msg, Int32 msgSize)
        {
            Int32 type = ByteInt32(msg[8]);
            byte[] seq_num = new byte[4];
            byte[] vars = new byte[msgSize - 13];

            Array.Copy(msg, 9, seq_num, 0, 4);
            Array.Copy(msg, 13, vars, 0, msgSize - 13);

            AddPlayerMsg APMSG = APMSGVars(vars);
            APMSG.msgType = type;
            return APMSG;
        }
        private AddPlayerMsg APMSGVars(byte[] playerType)
        {
            AddPlayerMsg msg = new AddPlayerMsg(ByteInt32(playerType));

            return msg;
        }
        // Test MSG
        private TestMsg GetTMSG(byte[] msg, Int32 msgSize)
        {
            Int32 type = ByteInt32(msg[8]);
            byte[] seq_num = new byte[4];
            byte[] vars = new byte[msgSize - 13];

            Array.Copy(msg, 9, seq_num, 0, 4);
            Array.Copy(msg, 13, vars, 0, msgSize - 13);

            TestMsg TMSG = TMSGVars(vars);
            TMSG.msgType = type;
            return TMSG;
        }
        private TestMsg TMSGVars(byte[] vars) //needs testing
        {
            byte[][] stuff = new byte[vars.Length / 4][];

            //Console.WriteLine(vars.Length);
            for (int index = 0; index < vars.Length / 4; index += 4)
            {
                Array.Copy(vars, index, stuff[index], 0, 4);
            }

            TestMsg msg = new TestMsg();
            msg.stuff = ByteInt32(stuff);
            return msg;
        }
        // Big Test MSG
        private BigTest GetBTMSG(byte[] msg, Int32 msgSize)
        {
            Int32 type = ByteInt32(msg[8]);
            byte[] seq_num = new byte[4];
            byte[] vars = new byte[msgSize - 13];

            Array.Copy(msg, 9, seq_num, 0, 4);
            Array.Copy(msg, 13, vars, 0, msgSize - 13);

            BigTest BTMSG = BTMSGVars(vars);
            BTMSG.msgType = type;
            return BTMSG;
        }
        private BigTest BTMSGVars(byte[] vars)
        {
            byte[] from = new byte[4];
            byte[] pos = new byte[12];
            byte[] pR = new byte[16];
            byte[] cR = new byte[16];

            int index = 0;
            //Console.WriteLine(vars.Length);
            Array.Copy(vars, index, from, 0, 4);
            index += 4;
            Array.Copy(vars, index, pos, 0, 12);
            index += 12;
            Array.Copy(vars, index, pR, 0, 16);
            index += 16;
            Array.Copy(vars, index, cR, 0, 16);

            BigTest msg = new BigTest();

            return msg;
        }

        // Check Message For Correct Length
        private Int32 checkMSG(byte[] msg)
        {
            byte[] size = new byte[4];
            Array.Copy(msg, 0, size, 0, 4);
            Int32 msgSize = ByteInt32(size);
            if (msg.Length == msgSize)
                return msgSize;
            else
                return 0;
        }

        // Byte Concatinating Functions ///////////////////
        //
        // Bytes = Byte . Byte
        public byte[] Combine(byte byte1, byte byte2, byte byte3, byte byte4)
        {
            byte[] newArray = new byte[4];
            newArray[0] = byte1;
            newArray[1] = byte2;
            newArray[2] = byte3;
            newArray[3] = byte4;
            return newArray;
        }
        // Bytes = Bytes . Byte
        public byte[] Combine(byte[] bArray, byte newByte)
        {
            byte[] newArray = new byte[bArray.Length + 1];
            bArray.CopyTo(newArray, 0);
            newArray[newArray.Length - 1] = newByte;
            return newArray;
        }
        // Bytes = Byte . Bytes
        public byte[] Combine(byte newByte, byte[] bArray)
        {
            byte[] newArray = new byte[bArray.Length + 1];
            bArray.CopyTo(newArray, 1);
            newArray[0] = newByte;
            return newArray;
        }
        // Bytes = Bytes . Bytes
        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }
        public static byte[] Combine(byte[] first, byte[] second, byte[] third)
        {
            byte[] ret = new byte[first.Length + second.Length + third.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            Buffer.BlockCopy(third, 0, ret, first.Length + second.Length,
                             third.Length);
            return ret;
        }
        public static byte[] Combine(byte[] first, byte[] second, byte[] third, byte[] fourth)
        {
            byte[] ret = new byte[first.Length + second.Length + third.Length + fourth.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            Buffer.BlockCopy(third, 0, ret, first.Length + second.Length,
                             third.Length);
            Buffer.BlockCopy(fourth, 0, ret, first.Length + second.Length + third.Length,
                              fourth.Length);
            return ret;
        }
        public static byte[] Combine(byte[] first, byte[] second, byte[] third, byte[] fourth, byte[] fifth)
        {
            byte[] ret = new byte[first.Length + second.Length + third.Length + fourth.Length + fifth.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            Buffer.BlockCopy(third, 0, ret, first.Length + second.Length,
                             third.Length);
            Buffer.BlockCopy(fourth, 0, ret, first.Length + second.Length + third.Length,
                              fourth.Length);
            Buffer.BlockCopy(fifth, 0, ret, first.Length + second.Length + third.Length +
                              fourth.Length, fifth.Length);
            return ret;
        }
        public static byte[] Combine(byte[] first, byte[] second, byte[] third, byte[] fourth, byte[] fifth, byte[] sixth, byte[] seventh)
        {
            byte[] ret = new byte[first.Length + second.Length + third.Length + fourth.Length + fifth.Length + sixth.Length + seventh.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            Buffer.BlockCopy(third, 0, ret, first.Length + second.Length,
                             third.Length);
            Buffer.BlockCopy(fourth, 0, ret, first.Length + second.Length + third.Length,
                              fourth.Length);
            Buffer.BlockCopy(fifth, 0, ret, first.Length + second.Length + third.Length +
                              fourth.Length, fifth.Length);
            Buffer.BlockCopy(sixth, 0, ret, first.Length + second.Length + third.Length +
                               fourth.Length + fifth.Length, sixth.Length);
            Buffer.BlockCopy(seventh, 0, ret, first.Length + second.Length + third.Length +
                               fourth.Length + fifth.Length + sixth.Length, seventh.Length);
            return ret;
        }

        // Byte Convertion Functions
        //
        // VarType to Byte Functions
        public byte[] IntByte(Int16 num)
        {
            return BitConverter.GetBytes(num);
        }
        public byte[] IntByte(Int32 num)
        {
            return BitConverter.GetBytes(num);
        }
        public byte[] IntByte(Int64 num)
        {
            return BitConverter.GetBytes(num);
        }
        public byte[] IntByte(Int32[] num)
        {
            byte[] numB = new byte[num.Length];
            for (int i = 0; i < numB.Length; i += 4)
            {
                numB = Combine(numB, IntByte(num.Range(i, i + 3)));
            }

            return numB;
        }
        public byte[] IntByte(List<Int32> num)
        {
            //get list size
            byte[] numB = new byte[num.Count * 4];

            foreach (Int32 n in num)
            {
                numB = Combine(numB, IntByte(n));
            }

            return numB;
        }
        public byte[] FByte(float num)
        {
            Int32 numi = (Int32)(num * 1000);
            return BitConverter.GetBytes(numi);
        }
        public byte CharByte(Char letter)
        {
            return (byte)letter;
        }
        public byte[] CharByte(Char[] letters)
        {
            return Encoding.ASCII.GetBytes(letters);
        }
        public byte[] Vec3Byte(Vector3 vec)
        {
            byte[] vecB = Combine(FByte(vec.x), FByte(vec.y), FByte(vec.z));

            return vecB;
        }
        public byte[] Vec3Byte(Vector3[] vec)
        {
            byte[] vecB = new byte[vec.Length * 12];
            for (int index = 0; index < vecB.Length; index++)
            {
                vecB = Combine(FByte(vec[index].x),
                               FByte(vec[index].y),
                               FByte(vec[index].z));
            }
            return vecB;
        }
        public byte[] Vec3Byte(List<Vector3> vecList)
        {
            byte[] vecB = new byte[vecList.Count * 12];
            foreach (Vector3 vec in vecList)
            {
                vecB = Combine(vecB, Vec3Byte(vec));
            }
            return vecB;
        }
        public byte[] QuatByte(Quaternion quat)
        {
            byte[] quatB = Combine(FByte(quat.w),
                                   FByte(quat.x),
                                   FByte(quat.y),
                                   FByte(quat.z));
            return quatB;
        }
        public byte[] QuatByte(List<Quaternion> quatList)
        {
            byte[] quatB = new byte[quatList.Count * 16];
            foreach (Quaternion quat in quatList)
            {
                quatB = Combine(quatB, QuatByte(quat));
            }
            return quatB;
        }

        // Byte to VarType Functions
        public Int32 ByteInt32(byte bite)
        {
            Int32 type = 0x00 << 24 |
                         0x00 << 16 |
                         0x00 << 8 | bite;
            return type;
        }
        public Int32 ByteInt32(byte bit3, byte bit2, byte bit1, byte bit0)
        {
            return BitConverter.ToInt32(Combine(bit3, bit2, bit1, bit0), 0);
        }
        public Int32 ByteInt32(byte[] bite)
        {
            return BitConverter.ToInt32(bite, 0);
        }
        public Int32[] ByteInt32(byte[][] bite)
        {
            Int32[] Int32Array = new Int32[bite.Length];
            for (int i = 0; i < bite.Length / 4; i++)
            {
                Int32Array[i] = ByteInt32(bite[i][3], bite[i][2], bite[i][1], bite[i][0]);
            }

            return Int32Array;
        }
        public Int64 ByteInt64(byte[] bite)
        {
            return BitConverter.ToInt64(bite, 0);
        }
        public float ByteFloat(byte[] bite)
        {
            //Console.WriteLine(bite.Length);
            return (float)BitConverter.ToInt32(bite, 0) / 1000;
        }
        public Char ByteChar(byte bite)
        {
            return (Char)bite;
        }
        public Char[] ByteChar(byte[] bite)
        {
            return Encoding.ASCII.GetString(bite).ToCharArray();
        }
        public Vector3 ByteVec3(byte[] bite)
        {
            //Console.WriteLine(bite.Length);
            return new Vector3(ByteFloat(bite.Range(8, 11)),
                               ByteFloat(bite.Range(4, 7)),
                               ByteFloat(bite.Range(0, 3)));
        }
        public Vector3[] ByteVec3Array(byte[] bite)
        {
            Vector3[] Vec3Array = new Vector3[bite.Length / 12];
            int vecIndex = 0;
            for (int index = 0; index < bite.Length; index += 12)
            {
                Vec3Array[vecIndex] = new Vector3(ByteFloat(Combine(bite[index + 3],
                                                                    bite[index + 2],
                                                                    bite[index + 1],
                                                                    bite[index])),
                                                  ByteFloat(Combine(bite[index + 7],
                                                                    bite[index + 6],
                                                                    bite[index + 5],
                                                                    bite[index + 4])),
                                                  ByteFloat(Combine(bite[index + 11],
                                                                    bite[index + 10],
                                                                    bite[index + 9],
                                                                    bite[index + 8])));
            }

            return Vec3Array;
        }
        public Quaternion ByteQuat(byte[] bite)
        {
            return new Quaternion(ByteFloat(bite.Range(12, 15)),
                               ByteFloat(bite.Range(8, 11)),
                               ByteFloat(bite.Range(4, 7)),
                               ByteFloat(bite.Range(0, 3)));
        }


    }

    //https://www.dotnetperls.com/array-slice
    public static class Extensions
    {
        // Array Range Function
        public static T[] Range<T>(this T[] source, int start, int end)
        {
            if (end < 0)
            {
                end = source.Length + end;
            }
            int len = end - start + 1;

            T[] res = new T[len];

            for (int i = 0; i < len; i++)
            {
                res[i] = source[i + start];
            }
            return res;
        }
    }

}