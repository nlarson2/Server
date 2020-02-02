using System;
using System.Collections;
using System.Collections.Generic;
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
    }


    Node root = new Node();
    Vector3[] changePos = new Vector3[8];
    Vector3 pos;
    float size = 1;
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
        addNodes(root, maxDepth);
        setup(root, this.size, pos);
    }

    public void addNodes(Node node, int depth)
    {
        node.node = new Node[8];

        if(depth - 1 == 0)
        {
            for (int i = 0; i < 8; i++)
            {
                node.node[i] = new LeafNode();
                if (node.GetType() == typeof(LeafNode))
                {
                    Debug.Log("WORKED");
                }
            }
            return;
        }

        for(int i = 0; i < 8; i++)
        {
            node.node[i] = new Node();
            addNodes(node.node[i], depth - 1);
        }
        
    }

    public void setup(Node node, float size, Vector3 pos)
    {
        if(node.GetType() == typeof(LeafNode))
        {
            Debug.Log("Worked again");
            GameObject c = Instantiate<GameObject>(cube, pos, transform.rotation, tran);
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
