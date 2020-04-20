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
        public NetObjectMsg ThisObj;
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
                ThisObj.objID.Add(objID);
                ThisObj.localScale.Add(scale);
                ThisObj.positions.Add(pos);
                ThisObj.rotation.Add(rot);
                netManager.NetObject(ThisObj);
                first = false;
            }



            if (pos != transform.position)
            {/*
                if (Vector3.Distance(pos, transform.position) > 10.0f)
                    transform.position = pos;
                else
                    transform.position = Vector3.MoveTowards(transform.position, pos, Time.deltaTime * speed);*/
                pos = transform.position;
                ChangeSnapshot();
            }
            if (rot != transform.rotation)
            {
                //transform.eulerAngles = rot.eulerAngles;
                //transform.rotation = Quaternion.Slerp(transform.rotation, rot, 0.1f);
                rot = transform.rotation;
                ChangeSnapshot();
            }
        }

        void ChangeSnapshot()
        {
            Debug.Log("SnapshotChange");
            Debug.Log(string.Format("Pos: {0}", this.pos));
            netManager.SnapshotOut(this);
        }
    }
}