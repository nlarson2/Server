using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Octree : MonoBehaviour
{


    private void SetChangePos()
    {
        changePos[0] = new Vector3(-1.0f, 1.0f, -1.0f);//LUF
        changePos[1] = new Vector3(-1.0f, 1.0f, 1.0f);//LUB
        changePos[2] = new Vector3(-1.0f, -1.0f, -1.0f);//LDF
        changePos[3] = new Vector3(-1.0f, -1.0f, 1.0f);//LDB
        changePos[4] = new Vector3(1.0f, 1.0f, -1.0f);//RUF
        changePos[5] = new Vector3(1.0f, 1.0f, 1.0f);//RUB
        changePos[6] = new Vector3(1.0f, -1.0f, -1.0f);//RDF
        changePos[7] = new Vector3(1.0f, -1.0f, 1.0f);//RDB
    }


    public class Node
    {
        public Node[] node;
    }

    public class LeafNode : Node
    {
        public Color color;
        public int size;
        public Vector3 pos;
        public String pathToNode = "";
    }

    /* constructor variables*/
    Node root = new Node();
    Vector3[] changePos = new Vector3[8];
    Vector3 pos;
    public int size = 1;
    public int maxDepth = 2;
    public GameObject cube;
    public Color color;
    Transform tran;

    // Start is called before the first frame update
    void Start()
    {
        SetChangePos();
        tran = this.transform;
        pos = transform.position;
        //addNodes(root, maxDepth);
       // root.node = new Node[8];
        setup("Assets/Test.txt");
        build(root, (float)Math.Pow(2, this.size), pos);
    }

   
    public void setup(string path)
    {
        string[] lines = File.ReadAllLines(path);
        string size = lines[0], depth = lines[1];

        Int32.TryParse(size, out this.size);
        Int32.TryParse(depth, out this.maxDepth);

        //set up 
        Node pnode;
        for (int i = 2; i < lines.Length; i++)
        {

            pnode = root;
            
            //Debug.Log(lines[i]);
            string[] subs = lines[i].Split(' ');
            //path in the string that leads to the leaf node
            string pathStr = subs[0];

            int pathNum;

            //Pass through all nodes, and stop before leaf
            for(int j = 0; j < pathStr.Length - 1; j++)
            {
                Int32.TryParse(pathStr[j].ToString(), out pathNum);
                //check to see if the node needs its array
                if(pnode.node == null)
                    pnode.node = new Node[8];
                //check to see if it has not been created already
                if(pnode.node[pathNum] == null)
                    pnode.node[pathNum] = new Node(); //creation
                pnode = pnode.node[pathNum]; //transfer
            }
            //verify if current nodes has childred alreday
            if(pnode.node == null)
                pnode.node = new Node[8];

            //generate a leafnode
            Int32.TryParse(pathStr[pathStr.Length - 1].ToString(), out pathNum);
            pnode.node[pathNum] = new LeafNode();

            //add color from file to the leafnode
            LeafNode tmp = (LeafNode)pnode.node[pathNum];
            float.TryParse(subs[1], out tmp.color.r);
            float.TryParse(subs[2], out tmp.color.g);
            float.TryParse(subs[3], out tmp.color.b);


        }
    }


    public void build(Node node, float size, Vector3 pos, string path = "0")
    {
        if (node.GetType() == typeof(LeafNode))
        {
            LeafNode tmp = (LeafNode)node;
            GameObject c = Instantiate<GameObject>(cube, pos, transform.rotation, tran);
            c.name = "NAME";
            c.transform.localScale = new Vector3(size, size, size);
            c.GetComponent<MeshRenderer>().material.color = tmp.color;
            return;
        }
        
        if (node.node != null)
        {
            for (int i = 0; i < 8; i++)
            {
                if (node.node[i] != null)
                {

                    Vector3 newPos = changePos[i];
                    newPos *= size / 4;
                    newPos += pos;
                    build(node.node[i], size / 2, newPos, path + i.ToString());

                }
            }
        }
    }



    void octreeToArray()
    {

    }










    // Update is called once per frame
    void Update()
    {

    }
  

    private void OnDrawGizmos()
    {
        float size = (float)Math.Pow(2, this.size);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(this.transform.position, new Vector3(size, size, size));
    }
}

