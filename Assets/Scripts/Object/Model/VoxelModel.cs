using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class VoxelModel : MonoBehaviour
{

    public class Cube
    {
        public Color color;
        public GameObject obj;
        //FBLRTB
        public bool[] face = new bool[6];

        public Cube()
        {
            for(int i = 0; i < face.Length; i++)
            {
                face[i] = false;
            }
        }
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
        checkFaces();
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

    }

    Vector3 getPos(int w, int h, int d)
    {
        float firstChange = ((float)(Math.Pow(2, this.size - 1)) - 0.5f) * this.scale;
        Vector3 startPos = new Vector3(firstChange, firstChange, firstChange);
        return startPos + (new Vector3(-w, -h, -d)) * this.scale;
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
        for (int d = 0; d < voxelArraySize; d++)
        {
            for (int h = 0; h < voxelArraySize; h++)
            {
                for (int w = 0; w < voxelArraySize; w++)
                {
                    if (voxel[h, w, d] == null)
                        continue;

                    Vector3 pos = getPos(w, h, d);
                    //FBLRTB
                    //front / back
                    if(d - 1 > 0 && d + 1 < voxelArraySize)
                    {

                        voxel[h, w, d].face[0] = voxel[h, w, d - 1] != null ? false : true;
                        if (voxel[h,w,d].face[0])
                        {

                            vertices.Add(pos + new Vector3(-adjust, adjust, adjust)); //LUF 0
                            vertices.Add(pos + new Vector3(-adjust, -adjust, adjust)); //LDF 1
                            vertices.Add(pos + new Vector3(adjust, -adjust, adjust)); //RDF 2 
                            vertices.Add(pos + new Vector3(adjust, adjust, adjust)); //RUF 3
                            triangles.Add(vertexCount + 0); triangles.Add(vertexCount + 1); triangles.Add(vertexCount + 2);
                            triangles.Add(vertexCount + 2); triangles.Add(vertexCount + 3); triangles.Add(vertexCount + 0);
                            vertexCount += 4;
                            Debug.Log("FRONT VERTICES MADE");
                        }
                        voxel[h, w, d].face[1] = voxel[h, w, d + 1] != null ? false : true;
                        if (voxel[h, w, d].face[1])
                        {
                            vertices.Add(pos + new Vector3(adjust, adjust, -adjust)); //RUB
                            vertices.Add(pos + new Vector3(adjust, -adjust, -adjust)); //RDB
                            vertices.Add(pos + new Vector3(-adjust, -adjust, -adjust)); //LDB
                            vertices.Add(pos + new Vector3(-adjust, adjust, -adjust)); //LUB
                            triangles.Add(vertexCount + 0); triangles.Add(vertexCount + 1); triangles.Add(vertexCount + 2);
                            triangles.Add(vertexCount + 2); triangles.Add(vertexCount + 3); triangles.Add(vertexCount + 0);
                            vertexCount += 4;
                            Debug.Log("BACK VERTICES MADE");
                        }

                    }
            
                    //left / right
                    if (w - 1 > 0 && w + 1 < voxelArraySize)
                    {
                        voxel[h, w, d].face[2] = voxel[h, w - 1, d] != null ? false : true;
                        if (voxel[h, w, d].face[2])
                        {
                            vertices.Add(pos + new Vector3(adjust, adjust,-adjust)); //RUB
                            vertices.Add(pos + new Vector3(adjust,-adjust,-adjust)); //RDB
                            vertices.Add(pos + new Vector3(adjust,-adjust, adjust)); //RDF
                            vertices.Add(pos + new Vector3(adjust, adjust, adjust)); //RUF
                            triangles.Add(vertexCount + 2); triangles.Add(vertexCount + 1); triangles.Add(vertexCount + 0);
                            triangles.Add(vertexCount + 0); triangles.Add(vertexCount + 3); triangles.Add(vertexCount + 2);
                            vertexCount += 4;
                            Debug.Log("RIGHT VERTICES MADE");
                        }
                        voxel[h, w, d].face[3] = voxel[h, w + 1, d] != null ? false : true;
                        if (voxel[h, w, d].face[3])
                        {
                            vertices.Add(pos + new Vector3(-adjust, adjust, adjust)); //LUB
                            vertices.Add(pos + new Vector3(-adjust, -adjust, adjust)); //LDB
                            vertices.Add(pos + new Vector3(-adjust, -adjust, -adjust)); //LDF
                            vertices.Add(pos + new Vector3(-adjust, adjust, -adjust)); //LUF
                            triangles.Add(vertexCount + 2); triangles.Add(vertexCount + 1); triangles.Add(vertexCount + 0);
                            triangles.Add(vertexCount + 0); triangles.Add(vertexCount + 3); triangles.Add(vertexCount + 2);
                            vertexCount += 4;
                            Debug.Log("LEFT VERTICES MADE");
                        }
                    }
                    
                    //top / bottom
                    if (h - 1 >= 0 && h + 1 < voxelArraySize)
                    {
                        voxel[h, w, d].face[4] = voxel[h - 1, w, d] != null ? false : true;
                        if (voxel[h, w, d].face[4])
                        {
                            vertices.Add(pos + new Vector3(adjust, adjust, -adjust)); //RUB
                            vertices.Add(pos + new Vector3(adjust, adjust, adjust));  //RUF
                            vertices.Add(pos + new Vector3(-adjust, adjust, adjust));  //LUF
                            vertices.Add(pos + new Vector3(-adjust, adjust, -adjust)); //LUB
                            triangles.Add(vertexCount + 2); triangles.Add(vertexCount + 1); triangles.Add(vertexCount + 0);
                            triangles.Add(vertexCount + 0); triangles.Add(vertexCount + 3); triangles.Add(vertexCount + 2);
                            vertexCount += 4;
                            Debug.Log("TOP VERTICES MADE");
                        }
                        voxel[h, w, d].face[5] = voxel[h + 1, w, d] != null ? false : true;
                        if (voxel[h, w, d].face[5])
                        {
                            vertices.Add(pos + new Vector3(adjust, -adjust, -adjust)); //LDB
                            vertices.Add(pos + new Vector3(adjust, -adjust, adjust));  //LDF
                            vertices.Add(pos + new Vector3(-adjust, -adjust, adjust));  //RDF
                            vertices.Add(pos + new Vector3(-adjust, -adjust, -adjust)); //RDB
                            triangles.Add(vertexCount + 0); triangles.Add(vertexCount + 1); triangles.Add(vertexCount + 2);
                            triangles.Add(vertexCount + 2); triangles.Add(vertexCount + 3); triangles.Add(vertexCount + 0);
                            vertexCount += 4;
                            Debug.Log("BOTTOM VERTICES MADE");
                        }
                    } else {
                        if(h - 1 < 0) {
                            vertices.Add(pos + new Vector3(adjust, adjust, -adjust)); //RUB
                            vertices.Add(pos + new Vector3(adjust, adjust, adjust));  //RUF
                            vertices.Add(pos + new Vector3(-adjust, adjust, adjust));  //LUF
                            vertices.Add(pos + new Vector3(-adjust, adjust, -adjust)); //LUB
                            triangles.Add(vertexCount + 2); triangles.Add(vertexCount + 1); triangles.Add(vertexCount + 0);
                            triangles.Add(vertexCount + 0); triangles.Add(vertexCount + 3); triangles.Add(vertexCount + 2);
                            vertexCount += 4;
                            Debug.Log("TOP VERTICES MADE");
                        }
                        if (h + 1 == voxelArraySize) {
                            vertices.Add(pos + new Vector3(adjust, -adjust, -adjust)); //LDB
                            vertices.Add(pos + new Vector3(adjust, -adjust, adjust));  //LDF
                            vertices.Add(pos + new Vector3(-adjust, -adjust, adjust));  //RDF
                            vertices.Add(pos + new Vector3(-adjust, -adjust, -adjust)); //RDB
                            triangles.Add(vertexCount + 0); triangles.Add(vertexCount + 1); triangles.Add(vertexCount + 2);
                            triangles.Add(vertexCount + 2); triangles.Add(vertexCount + 3); triangles.Add(vertexCount + 0);
                            vertexCount += 4;
                            Debug.Log("BOTTOM VERTICES MADE");
                        }
                    }
                }
            }
        }
    }

    // Update is called once per frame
    float time = 0;
    bool hasran = false;
    void Update()
    {
       
    }
    


    private void OnDrawGizmos()
    {
        float size = (float)Math.Pow(2, this.size) * this.scale;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(this.transform.position, new Vector3(size, size, size));
    }


}
