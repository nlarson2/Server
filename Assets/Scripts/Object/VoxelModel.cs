using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelModel : MonoBehaviour
{

    public class Cube
    {
        Color color;
        public GameObject obj;
    }


    public int size = 2;
    public Cube[,,] voxel;
    public GameObject cube;
    public float scale = 0.25f;
    int voxelArraySize;
    

    void Start()
    {
        voxelArraySize = (int)(Math.Pow(2, this.size));
        this.scale = cube.transform.localScale.x;
        voxel = new Cube[voxelArraySize, voxelArraySize, voxelArraySize];
        float firstChange = ((float)(Math.Pow(2, this.size-1)) - 0.5f) * this.scale;
        Vector3 startPos = new Vector3(firstChange, firstChange, firstChange);
        for(int w = 0; w < voxelArraySize; w++)
        {
            for (int h = 0; h < voxelArraySize; h++)
            {
                for (int d = 0; d < voxelArraySize; d++)
                {
                    voxel[w, h, d] = new Cube();
                    GameObject obj = Instantiate(cube, startPos + (new Vector3(-w,-h,-d)) * this.scale, new Quaternion());
                    voxel[w, h, d].obj = obj;
                }
            }
        }
    }


    // Update is called once per frame
    float time = 0;
    void Update()
    {
        time += Time.deltaTime;
        if (time > 4)
        {
            for (int w = 0; w < voxelArraySize; w++)
            {
                for (int h = 0; h < voxelArraySize; h++)
                {
                    for (int d = 0; d < voxelArraySize; d++)
                    {
                        if (!voxel[w, h, d].obj.activeInHierarchy)
                        {
                            voxel[w, h, d] = null;
                        }
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        float size = (float)Math.Pow(2, this.size) * this.scale;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(this.transform.position, new Vector3(size, size, size));
    }


}
