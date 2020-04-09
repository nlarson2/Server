using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace SmashDomeVoxel
{
    public class VoxelModelBuilder : MonoBehaviour
    {

        public class CubeBuilder
        {
            public Color color;
            public GameObject obj;
        }


        public int size = 2;
        public CubeBuilder[,,] voxel;
        public GameObject cube;
        public float scale = 0.25f;
        int voxelArraySize;
        public string outputfile;
        Vector3 pos;

        void Start()
        {
            pos = this.transform.position;
            voxelArraySize = (int)(Math.Pow(2, this.size));
            this.scale = cube.transform.localScale.x;
            voxel = new CubeBuilder[voxelArraySize, voxelArraySize, voxelArraySize];
            float firstChange = ((float)(Math.Pow(2, this.size - 1)) - 0.5f) * this.scale;
            Vector3 startPos = new Vector3(firstChange, firstChange, firstChange);
            for (int w = 0; w < voxelArraySize; w++)
            {
                for (int h = 0; h < voxelArraySize; h++)
                {
                    for (int d = 0; d < voxelArraySize; d++)
                    {
                        voxel[w, h, d] = new CubeBuilder();
                        GameObject obj = Instantiate(cube, pos + startPos + (new Vector3(-w, -h, -d)) * this.scale, new Quaternion());
                        voxel[w, h, d].obj = obj;
                        voxel[w, h, d].color = Color.red;
                        obj.GetComponent<MeshRenderer>().material.color = voxel[w, h, d].color;
                    }
                }
            }
        }


        // Update is called once per frame
        float time = 0;
        bool hasran = false;
        void Update()
        {
            time += Time.deltaTime;
            if (time > 4 && !hasran)
            {
                for (int w = 0; w < voxelArraySize; w++)
                {
                    for (int h = 0; h < voxelArraySize; h++)
                    {
                        for (int d = 0; d < voxelArraySize; d++)
                        {
                            try
                            {
                                if (voxel[w, h, d].obj.activeInHierarchy) ;
                            }
                            catch (MissingReferenceException e)
                            {
                                voxel[w, h, d] = null;
                            }
                        }
                    }
                }
                hasran = true;
                genFile(this.outputfile);
            }
        }

        void genFile(string filename)
        {

            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.WriteLine(this.size);
                writer.WriteLine(this.scale);

                for (int h = 0; h < voxelArraySize; h++)
                {
                    for (int w = 0; w < voxelArraySize; w++)
                    {
                        for (int d = 0; d < voxelArraySize; d++)
                        {

                            CubeBuilder cube = voxel[w, h, d];
                            if (cube != null)
                                writer.WriteLine(string.Format("{0} {1} {2} {3} {4} {5}", h, w, d, cube.color.r, cube.color.g, cube.color.b));
                        }
                    }
                }
            }
            Debug.Log("DONE");
        }




        private void OnDrawGizmos()
        {
            float size = (float)Math.Pow(2, this.size) * this.scale;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(this.transform.position, new Vector3(size, size, size));
        }


    }
}