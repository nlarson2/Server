using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SmashDomeNetwork {

    public class Snapshot : MonoBehaviour
    {

        NetworkManager netManager = NetworkManager.Instance;
        static int count = 1;
        public float speed = 10.0f;
        public GameObject obj;
        public Vector3 scale;
        public Vector3 pos;
        public Quaternion rot;
        public Vector3 linear_speed;
        public Quaternion angular_speed;
        public int objID;
        public int textureType = 0;
        public NetObjectMsg ThisObj;
        public bool hasMoved = false;
        // Start is called before the first frame update
        void Start()
        {  
            
            //this.obj = gameObject.transform.gameObject; //later will be detailed specs
            this.scale = gameObject.transform.localScale;
            this.pos = gameObject.transform.position;
            this.rot = gameObject.transform.rotation;
            this.linear_speed = new Vector3(0f,0f,0f);
            this.angular_speed = new Quaternion(0f,0f,0f,0f);
           
            Debug.Log("Does it get to start - NetObject?");
            //netManager.NetObject(ThisObj);
        
            // Update is called once per frame

        }
        private bool first = true;
        void Update()
        {
            //if pos or rot change
            // snapshotUpdate();
            if(this.netManager == null)
            {
                this.netManager = NetworkManager.Instance;
                return;
            }
            else if (first)
            {
                this.objID = count++;//netManager.netobjects.Count;
                ThisObj = new NetObjectMsg(objID);
                ThisObj.objID = (objID);
                ThisObj.textureType = (this.textureType);
                ThisObj.positions = (pos);
                ThisObj.rotation = (rot);
                netManager.NetObject(ThisObj);
                first = false;
            }



            if (Moved(pos, transform.position))
            {/*
                if (Vector3.Distance(pos, transform.position) > 10.0f)
                    transform.position = pos;
                else
                    transform.position = Vector3.MoveTowards(transform.position, pos, Time.deltaTime * speed);*/
                hasMoved = true;
                pos = transform.position;
                ThisObj.positions = pos;
                ChangeSnapshot();
            }
            if (rot != transform.rotation)
            {
                //transform.eulerAngles = rot.eulerAngles;
                //transform.rotation = Quaternion.Slerp(transform.rotation, rot, 0.1f);
                rot = transform.rotation;
                ThisObj.rotation = rot;
                ChangeSnapshot();
            }
        }

        void ChangeSnapshot()
        {
            //Debug.Log("SnapshotChange");
            //Debug.Log(string.Format("Pos: {0}", this.pos));
            netManager.SnapshotOut(this);
        }
        private float eps = 0.0000001f;
        bool Moved(Vector3 p1, Vector3 p2)
        {   
            if (Mathf.Abs(p1.x - p2.x) > eps)
                return true;
            if (Mathf.Abs(p1.y - p2.y) > eps)
                return true;
            if (Mathf.Abs(p1.z - p2.z) > eps)
                return true;
            return false;
        }
    }
}