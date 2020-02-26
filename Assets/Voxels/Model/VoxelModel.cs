using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SmashDomeNetwork;


namespace SmashDomeVoxel
{
    public class VoxelModel : NetworkedItem
    {

        public class Cube
        {
            public Color color;
            public GameObject obj;
            //FBLRTB
            public bool[] face = new bool[6];

            public Cube()
            {
                for (int i = 0; i < face.Length; i++)
                {
                    face[i] = false;
                }
            }
        }

        public struct MeshRange
        {
            public Vector3 begin;
            public Vector3 end;
        }



        public int size;
        public Cube[,,] voxel = null;
        public GameObject cube;
        public float scale;
        int voxelArraySize;
        public string inputfile;
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        Mesh mesh;
        bool meshed = false;

        void Start()
        {
            mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;

            string[] lines = File.ReadAllLines(this.inputfile);
            string size = lines[0], scale = lines[1];

            Int32.TryParse(size, out this.size);
            float.TryParse(scale, out this.scale);
            voxelArraySize = (int)Math.Pow(2, this.size);

            //New cube order -> [height, width, depth]
            voxel = new Cube[voxelArraySize, voxelArraySize, voxelArraySize];

            string[] sub;
            int h, w, d;
            float r, g, b;

            float firstChange = ((float)(Math.Pow(2, this.size - 1)) - 0.5f) * this.scale;
            Vector3 startPos = new Vector3(firstChange, firstChange, firstChange);

            //for (int i = 2; i < lines.Length; i++)
            for (int i = 2; i < lines.Length; i++)
            {
                sub = lines[i].Split(' ');
                int.TryParse(sub[0], out h);
                int.TryParse(sub[1], out w);
                int.TryParse(sub[2], out d);
                float.TryParse(sub[3], out r);
                float.TryParse(sub[4], out g);
                float.TryParse(sub[5], out b);

                voxel[h, w, d] = new Cube();
                voxel[h, w, d].color = new Color(r, g, b);

                /*GameObject obj = Instantiate(cube, startPos + (new Vector3(-w, -h, -d)) * this.scale, new Quaternion());
                obj.transform.localScale = Vector3.one * this.scale;
                obj.GetComponent<MeshRenderer>().material.color = voxel[h, w, d].color;*/

            }

            //func it up here
            MeshCollider myMC = GetComponent<MeshCollider>();
            checkFaces();
            mesh.Clear();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateBounds();
            myMC.sharedMesh = mesh;
            //Debug.Log(string.Format("VERTS: {0}  TRIS: {1}", mesh.vertexCount, mesh.vertexCount / 2));
            meshed = true;
        }

        void resetMesh()
        {
            mesh.Clear();
            vertices.Clear();
            triangles.Clear();
        }

        void rebuildMesh()
        {
            meshed = false;
            MeshCollider myMC = GetComponent<MeshCollider>();
            mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;
            checkFaces();
            mesh.Clear();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateBounds();
            myMC.sharedMesh = mesh;
            meshed = true;
            //Debug.Log("OH SH-! WTF HAPPENED TO THE CAPSULE BOIIIIII?!");

        }

        Vector3 getPos(int w, int h, int d)
        {
            float firstChange = ((float)(Math.Pow(2, this.size - 1)) - 0.5f) * this.scale;
            Vector3 startPos = new Vector3(firstChange, firstChange, firstChange);
            return startPos + (new Vector3(-w, -h, -d)) * this.scale;
        }
        void merge(List<MeshRange> face)
        {
            bool change;
            if (face.Count < 2)
                return;
            //1 = begin 2 = end
            Vector3 a1, b1, a2, b2;
            do
            {
                change = false;
                for (int i = 0; i < face.Count - 1; i++)
                {
                    for (int j = i + 1; j < face.Count; j++)
                    {
                        a1 = face[i].begin; a2 = face[i].end;
                        b1 = face[j].begin; b2 = face[j].end;
                        if ((a1.x == b1.x && a1.y == b1.y && a2.x == b2.x && a2.y == b2.y && (Math.Abs(a2.z - b1.z) == 1 || Math.Abs(a1.z - b2.z) == 1)) ||
                            (a1.x == b1.x && a1.z == b1.z && a2.x == b2.x && a2.z == b2.z && (Math.Abs(a2.y - b1.y) == 1 || Math.Abs(a1.y - b2.y) == 1)) ||
                            (a1.z == b1.z && a1.y == b1.y && a2.z == b2.z && a2.y == b2.y && (Math.Abs(a2.x - b1.x) == 1 || Math.Abs(a1.x - b2.x) == 1)))
                        {
                            Vector3 begin = face[i].begin;
                            Vector3 end = face[j].end;
                            MeshRange range = new MeshRange();
                            range.begin = begin;
                            range.end = end;
                            face.Remove(face[j]);
                            face.Remove(face[i]);
                            //Debug.Log(i);
                            face.Add(range);
                            change = true;
                        }

                    }
                }

            } while (change);


        }

        public void addFace(List<MeshRange> face, Vector3 pos)
        {
            MeshRange f = new MeshRange();
            f.begin = pos;
            f.end = pos;
            face.Add(f);
            merge(face);
        }

        //Check Face visibility
        //
        public void checkFaces()
        {
            float firstChange = ((float)(Math.Pow(2, this.size - 1)) - 0.5f) * this.scale;
            Vector3 startPos = new Vector3(firstChange, firstChange, firstChange);
            float adjust = this.scale / 2;
            int vertexCount = 0;
            //Vector3 cubePos = startPos  + (new Vector3(-w, -h, -d)) * this.scale;
            //loop through and turn on faces that are visible

            List<MeshRange> front = new List<MeshRange>();
            List<MeshRange> back = new List<MeshRange>();
            List<MeshRange> left = new List<MeshRange>();
            List<MeshRange> right = new List<MeshRange>();
            List<MeshRange> top = new List<MeshRange>();
            List<MeshRange> bottom = new List<MeshRange>();


            for (int d = 0; d < voxelArraySize; d++)
            {
                for (int h = 0; h < voxelArraySize; h++)
                {
                    for (int w = 0; w < voxelArraySize; w++)
                    {
                        if (voxel[h, w, d] == null)
                            continue;

                        Vector3 pos = getPos(w, h, d);
                        //Debug.Log("POSE" + pos.ToString());

                        Vector3 arrayPos = new Vector3(w, h, d);

                        //FBLRTB
                        //front / back
                        if (d - 1 >= 0 && d + 1 < voxelArraySize)
                        {
                            //Add front faces
                            voxel[h, w, d].face[0] = voxel[h, w, d - 1] != null ? false : true;
                            if (voxel[h, w, d].face[0])
                                addFace(front, arrayPos);

                            //Add back faces
                            voxel[h, w, d].face[1] = voxel[h, w, d + 1] != null ? false : true;
                            if (voxel[h, w, d].face[1])
                                addFace(back, arrayPos);

                        }
                        else
                        {
                            //Add front faces if they're below the bounds of our array
                            if (d - 1 < 0)
                                addFace(front, arrayPos);
                            //Add front faces if they're above the bounds of our array
                            else if (d + 1 >= voxelArraySize)
                                addFace(back, arrayPos);
                        }

                        //left / right
                        if (w - 1 >= 0 && w + 1 < voxelArraySize)
                        {
                            voxel[h, w, d].face[2] = voxel[h, w - 1, d] != null ? false : true;
                            if (voxel[h, w, d].face[2])
                                addFace(left, arrayPos);


                            voxel[h, w, d].face[3] = voxel[h, w + 1, d] != null ? false : true;
                            if (voxel[h, w, d].face[3])
                                addFace(right, arrayPos);
                        }
                        else
                        {
                            if (w - 1 < 0)
                                addFace(left, arrayPos);
                            else if (w + 1 == voxelArraySize)
                                addFace(right, arrayPos);
                        }

                        //top / bottom
                        if (h - 1 >= 0 && h + 1 < voxelArraySize)
                        {
                            voxel[h, w, d].face[4] = voxel[h - 1, w, d] != null ? false : true;
                            if (voxel[h, w, d].face[4])
                                addFace(top, arrayPos);
                            voxel[h, w, d].face[5] = voxel[h + 1, w, d] != null ? false : true;
                            if (voxel[h, w, d].face[5])
                                addFace(bottom, arrayPos);
                        }
                        else
                        {
                            if (h - 1 < 0)
                                addFace(top, arrayPos);
                            if (h + 1 == voxelArraySize)
                                addFace(bottom, arrayPos);
                        }
                    }
                }
            }
            foreach (MeshRange m in front)
            {
                //Debug.Log(string.Format("begin {0}  end {1}", m.begin, m.end));
                Vector3 topleft = getPos((int)m.begin.x, (int)m.begin.y, (int)m.begin.z);
                Vector3 bottomleft = getPos((int)m.begin.x, (int)m.end.y, (int)m.begin.z);
                Vector3 bottomright = getPos((int)m.end.x, (int)m.end.y, (int)m.begin.z);
                Vector3 topright = getPos((int)m.end.x, (int)m.begin.y, (int)m.begin.z);
                //Debug.Log(string.Format("TOPLEFT: {0}  BOTTOMLEFT: {1}  BOTTOMRIGHT: {2}  TOPRIGHT: {3}", topleft, bottomleft, bottomright, topright));
                vertices.Add(topleft + new Vector3(adjust, adjust, adjust)); //LUF 0
                vertices.Add(bottomleft + new Vector3(adjust, -adjust, adjust)); //LDF 1
                vertices.Add(bottomright + new Vector3(-adjust, -adjust, adjust)); //RDF 2 
                vertices.Add(topright + new Vector3(-adjust, adjust, adjust)); //RUF 3
                triangles.Add(vertexCount + 0); triangles.Add(vertexCount + 2); triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 2); triangles.Add(vertexCount + 0); triangles.Add(vertexCount + 3);
                vertexCount += 4;
                //Debug.Log("attempt");
            }
            foreach (MeshRange m in back)
            {
                //Debug.Log(string.Format("begin {0}  end {1}", m.begin, m.end));
                Vector3 topleft = getPos((int)m.begin.x, (int)m.begin.y, (int)m.begin.z);
                Vector3 bottomleft = getPos((int)m.begin.x, (int)m.end.y, (int)m.begin.z);
                Vector3 bottomright = getPos((int)m.end.x, (int)m.end.y, (int)m.begin.z);
                Vector3 topright = getPos((int)m.end.x, (int)m.begin.y, (int)m.begin.z);
                //Debug.Log(string.Format("TOPLEFT: {0}  BOTTOMLEFT: {1}  BOTTOMRIGHT: {2}  TOPRIGHT: {3}", topleft, bottomleft, bottomright, topright));
                vertices.Add(topleft + new Vector3(adjust, adjust, -adjust)); //RUB
                vertices.Add(bottomleft + new Vector3(adjust, -adjust, -adjust)); //RDB
                vertices.Add(bottomright + new Vector3(-adjust, -adjust, -adjust)); //LDB
                vertices.Add(topright + new Vector3(-adjust, adjust, -adjust)); //LUB
                triangles.Add(vertexCount + 0); triangles.Add(vertexCount + 1); triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 2); triangles.Add(vertexCount + 3); triangles.Add(vertexCount + 0);
                vertexCount += 4;
                //Debug.Log("attempt");
            }
            foreach (MeshRange m in left)
            {
                //Debug.Log(string.Format("begin {0}  end {1}", m.begin, m.end));
                Vector3 topleft = getPos((int)m.begin.x, (int)m.begin.y, (int)m.end.z);
                Vector3 bottomleft = getPos((int)m.begin.x, (int)m.end.y, (int)m.end.z);
                Vector3 bottomright = getPos((int)m.begin.x, (int)m.end.y, (int)m.begin.z);
                Vector3 topright = getPos((int)m.begin.x, (int)m.begin.y, (int)m.begin.z);
                //Debug.Log(string.Format("TOPLEFT: {0}  BOTTOMLEFT: {1}  BOTTOMRIGHT: {2}  TOPRIGHT: {3}", topleft, bottomleft, bottomright, topright));
                vertices.Add(topleft + new Vector3(adjust, adjust, -adjust)); //RUB
                vertices.Add(bottomleft + new Vector3(adjust, -adjust, -adjust)); //RDB
                vertices.Add(bottomright + new Vector3(adjust, -adjust, adjust)); //RDF
                vertices.Add(topright + new Vector3(adjust, adjust, adjust)); //RUF
                triangles.Add(vertexCount + 2); triangles.Add(vertexCount + 1); triangles.Add(vertexCount + 0);
                triangles.Add(vertexCount + 0); triangles.Add(vertexCount + 3); triangles.Add(vertexCount + 2);
                vertexCount += 4;
                //Debug.Log("RIGHT VERTICES MADE");
            }
            foreach (MeshRange m in right)
            {
                //Debug.Log(string.Format("begin {0}  end {1}", m.begin, m.end));
                Vector3 topleft = getPos((int)m.begin.x, (int)m.begin.y, (int)m.begin.z);
                Vector3 bottomleft = getPos((int)m.begin.x, (int)m.end.y, (int)m.begin.z);
                Vector3 bottomright = getPos((int)m.begin.x, (int)m.end.y, (int)m.end.z);
                Vector3 topright = getPos((int)m.begin.x, (int)m.begin.y, (int)m.end.z);
                //Debug.Log(string.Format("TOPLEFT: {0}  BOTTOMLEFT: {1}  BOTTOMRIGHT: {2}  TOPRIGHT: {3}", topleft, bottomleft, bottomright, topright));
                vertices.Add(topleft + new Vector3(-adjust, adjust, adjust)); //LUB
                vertices.Add(bottomleft + new Vector3(-adjust, -adjust, adjust)); //LDB
                vertices.Add(bottomright + new Vector3(-adjust, -adjust, -adjust)); //LDF
                vertices.Add(topright + new Vector3(-adjust, adjust, -adjust)); //LUF
                triangles.Add(vertexCount + 2); triangles.Add(vertexCount + 1); triangles.Add(vertexCount + 0);
                triangles.Add(vertexCount + 0); triangles.Add(vertexCount + 3); triangles.Add(vertexCount + 2);
                vertexCount += 4;
                //Debug.Log("RIGHT VERTICES MADE");
            }
            foreach (MeshRange m in top)
            {
                //Debug.Log(string.Format("begin {0}  end {1}", m.begin, m.end));
                Vector3 topleft = getPos((int)m.end.x, (int)m.begin.y, (int)m.end.z);
                Vector3 bottomleft = getPos((int)m.end.x, (int)m.begin.y, (int)m.begin.z);
                Vector3 bottomright = getPos((int)m.begin.x, (int)m.begin.y, (int)m.begin.z);
                Vector3 topright = getPos((int)m.begin.x, (int)m.begin.y, (int)m.end.z);
                //Debug.Log(string.Format("TOPLEFT: {0}  BOTTOMLEFT: {1}  BOTTOMRIGHT: {2}  TOPRIGHT: {3}", topleft, bottomleft, bottomright, topright));
                vertices.Add(topleft + new Vector3(-adjust, adjust, -adjust)); //RUB
                vertices.Add(bottomleft + new Vector3(-adjust, adjust, adjust));  //RUF
                vertices.Add(bottomright + new Vector3(adjust, adjust, adjust));  //LUF
                vertices.Add(topright + new Vector3(adjust, adjust, -adjust)); //LUB
                triangles.Add(vertexCount + 0); triangles.Add(vertexCount + 1); triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 2); triangles.Add(vertexCount + 3); triangles.Add(vertexCount + 0);
                vertexCount += 4;
                //Debug.Log("RIGHT VERTICES MADE");
            }
            foreach (MeshRange m in bottom)
            {
                //Debug.Log(string.Format("begin {0}  end {1}", m.begin, m.end));
                Vector3 topleft = getPos((int)m.begin.x, (int)m.end.y, (int)m.end.z);
                Vector3 bottomleft = getPos((int)m.begin.x, (int)m.end.y, (int)m.begin.z);
                Vector3 bottomright = getPos((int)m.end.x, (int)m.end.y, (int)m.begin.z);
                Vector3 topright = getPos((int)m.end.x, (int)m.end.y, (int)m.end.z);
                //Debug.Log(string.Format("TOPLEFT: {0}  BOTTOMLEFT: {1}  BOTTOMRIGHT: {2}  TOPRIGHT: {3}", topleft, bottomleft, bottomright, topright));
                vertices.Add(topleft + new Vector3(adjust, -adjust, -adjust)); //LDB
                vertices.Add(bottomleft + new Vector3(adjust, -adjust, adjust)); //LDF
                vertices.Add(bottomright + new Vector3(-adjust, -adjust, adjust)); //RDF
                vertices.Add(topright + new Vector3(-adjust, -adjust, -adjust)); //RDB
                triangles.Add(vertexCount + 0); triangles.Add(vertexCount + 1); triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 2); triangles.Add(vertexCount + 3); triangles.Add(vertexCount + 0);
                vertexCount += 4;
                //Debug.Log("RIGHT VERTICES MADE");
            }
        }

        // Update is called once per frame
        float time = 0.0f;          // Not sure what this was for so i'm not messing with it
        float waitTime = 1.0f;      // Ten seconds
        float timer = 0.0f;         // Starting time
        bool timerWentOff = false;
        bool hasran = false;
        public bool collided = false;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag != "Model")
            {
                //Debug.Log("Shit done did collided");
                foreach (ContactPoint contactLocation in collision.contacts)
                {

                    int posx = (int)((contactLocation.point.x + 2) * 3.9);
                    int posy = (int)((contactLocation.point.y + 2) * 3.9);
                    int posz = (int)((contactLocation.point.z + 2) * 3.9);
                    Debug.Log(string.Format("Y:{0}  X:{1}  Z:{2}", posy, posx, posz));
                    if (voxel[posy, posx,posz] == null)
                        Debug.Log("Already NULL");
                    voxel[15-posy, 15-posx, 15-posz] = null;


                }
                resetMesh();
                rebuildMesh();
                //Destroy(collision.gameObject);
            }
        }


        void Update()
        {
            if(this.netManager == null)
            {
                this.netManager = NetworkManager.Instance;
                return;
            }

            //if (timer < waitTime)
            //{
            //    timer += Time.deltaTime;
            //    Debug.Log(timer);
            //}

            //if (!timerWentOff)
            //{
            //    if (timer > waitTime)
            //    {
            //        for(int i = 0; i < 10; i++)
            //        {
            //            for(int j = 0; j < 10; j++)
            //            {
            //                for (int k = 0; k < 10; k++)
            //                {
            //                    voxel[i, j, k] = null;
            //                }
            //            }
            //        }
            //        resetMesh();
            //        rebuildMesh();
            //        timerWentOff = true;
            //    }
            //}

            if(!hasran && meshed)
            {

                StructureChangeMsg outMsg = new StructureChangeMsg();
                outMsg.pos = transform.position;
                outMsg.vertices = mesh.vertices;
                outMsg.triangles = mesh.triangles;
                netManager.structures.Add(outMsg);
                hasran = true;

            }

            
           
           // Debug.Log("Oh shit boi what the fuck is going on we hittin all the shits");
           
            
            //this.gameObject.GetComponent<Collider>().enabled = false;
            



        }

        public void FindCube()
        {

        }

        private void OnDrawGizmos()
        {
            float size = (float)Math.Pow(2, this.size) * this.scale;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(this.transform.position, new Vector3(size, size, size));
        }

    }
}