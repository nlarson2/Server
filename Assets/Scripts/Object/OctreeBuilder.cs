using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class OctreeBuilder : MonoBehaviour
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
        public int level;
    }
   
    public class LeafNode : Node
    {
        public Color color;
        public int size;
        public Vector3 pos;
        public GameObject obj;
        public String pathToNode = "";

        public override string ToString()
        {
            string ret = pathToNode;
            ret += " " + color.r + " " + color.g + " " + color.b;
            Debug.Log(ret);
            return ret;
        }
    }


    Node root = new Node();
    Vector3[] changePos = new Vector3[8];
    Vector3 pos;
    public int size = 1;
    public int maxDepth = 2;
    public GameObject cube;
    public Color color;
    Transform tran;

    Queue<LeafNode> nodes = new Queue<LeafNode>();

    
    
    // Start is called before the first frame update
    void Start()
    {        
        SetChangePos();
        tran = this.transform;
        pos = transform.position;
        addNodes(root, maxDepth);
        
        setup(root, (float)Math.Pow(2, this.size), pos);
    }

    public void addNodes(Node node, int depth, string path = "")
    {
        node.node = new Node[8];

        if(depth - 1 == 0)
        {
            for (int i = 0; i < 8; i++)
            {
                //node.node[i] = new LeafNode();
                LeafNode tmp = new LeafNode();
                tmp.pathToNode = path + i.ToString();
                Debug.Log(tmp.pathToNode);
                tmp.color = Color.red;
                node.node[i] = tmp;
                if (node.GetType() == typeof(LeafNode))
                {
                   // Debug.Log("WORKED");
                }
            }
            return;
        }

        for(int i = 0; i < 8; i++)
        {
            node.node[i] = new Node();
            addNodes(node.node[i], depth - 1, path + i.ToString());
        }
        
    }

    public void setup(Node node, float size, Vector3 pos)
    {
        if(node.GetType() == typeof(LeafNode))
        {
            LeafNode tmp = (LeafNode)node;
            //Debug.Log("Worked again");
            GameObject c = Instantiate<GameObject>(cube, pos, transform.rotation, tran);
            tmp.obj = c;
            nodes.Enqueue(tmp);
            c.name = "NAME";
            c.transform.localScale = new Vector3(size, size, size);
            c.GetComponent<MeshRenderer>().material.color = this.color;
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
                    setup(node.node[i], size / 2, newPos);
                    
                }
            }
        }
    }
    float time = 0;
    bool filegenerated = false;
    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if (time > 12 && !filegenerated)
        {
            GenFile("Assets/Test.txt");
            filegenerated = true;
        }
    }

    private void OnDrawGizmos()
    {
        float size = (float)Math.Pow(2, this.size);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(this.transform.position, new Vector3(size, size, size));
    }

    //Iterate through all leaf nodes, dequeue all nodes, and place values into file?
    public void GenFile(string outputname) {
        LeafNode tmp;        
        //input size
        int inputSize = this.size;
        
        //input depth
        int inputDepth = this.maxDepth;
        bool firstloop = true;
        
        // Write file using StreamWriter  
        using (StreamWriter writer = new StreamWriter(outputname))  
        {
            while (nodes.Count > 0)
            {
                if (firstloop)
                {
                    writer.WriteLine(inputSize);
                    writer.WriteLine(inputDepth);
                    firstloop = false;
                }
                tmp = nodes.Dequeue();
                try
                {
                    if (tmp.obj.GetComponent<CubeBuilder>().collided)
                        writer.WriteLine(tmp.ToString());
                }
                catch (Exception e) { }
            }
        }  
    }  
}