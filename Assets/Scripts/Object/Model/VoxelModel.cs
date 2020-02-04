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
    }


    public int size;
    public Cube[,,] voxel;
    public GameObject cube;
    public float scale;
    int voxelArraySize;
    public string inputfile;
    Vector3[] vertices;
    int[] triangles;

    void Start()
    {

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
        for(int i = 2; i < lines.Length; i++)
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
            float firstChange = ((float)(Math.Pow(2, this.size - 1)) - 0.5f) * this.scale;
            Vector3 startPos = new Vector3(firstChange, firstChange, firstChange);
            GameObject obj = Instantiate(cube, startPos + (new Vector3(-w, -h, -d)) * this.scale, new Quaternion());
            obj.transform.localScale = Vector3.one * this.scale;
            obj.GetComponent<MeshRenderer>().material.color = voxel[h, w, d].color;
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
