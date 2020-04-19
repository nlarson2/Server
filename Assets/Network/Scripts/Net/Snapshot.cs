using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SmashDomeNetwork {

    public class Snapshot : MonoBehaviour
    {

        NetworkManager netManager = NetworkManager.Instance;

        public float speed = 10.0f;
        public GameObject obj;
        public Vector3 scale;
        public Vector3 pos;
        public Quaternion rot;
        public Vector3 linear_speed;
        public Quaternion angular_speed;
        public int objID;

        // Start is called before the first frame update
        void Start()
        {
            this.obj = gameObject.transform.gameObject; //later will be detailed specs
            this.scale = gameObject.transform.localScale;
            this.pos = gameObject.transform.position;
            this.rot = gameObject.transform.rotation;

            NetObjectMsg ThisObj = new NetObjectMsg(0);
            ThisObj.localScale.Add(scale);
            ThisObj.positions.Add(pos);
            ThisObj.rotation.Add(rot);

            netManager.NetObject(ThisObj);
        }

        // Update is called once per frame
        void Update()
        {
            //if pos or rot change
            // snapshotUpdate();

            if (pos != transform.position)
            {
                if (Vector3.Distance(pos, transform.position) > 10.0f)
                    transform.position = pos;
                else
                    transform.position = Vector3.MoveTowards(transform.position, pos, Time.deltaTime * speed);
            }
            if (rot != transform.rotation)
            {
                transform.eulerAngles = rot.eulerAngles;
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, 0.1f);
            }
        }

        void ChangeSnapshot()
        {
            SnapshotMsg msg = new SnapshotMsg(0);
            msg.positions.Add(pos);
            msg.rotation.Add(rot);
            msg.linear_speed.Add(linear_speed);
            msg.angular_speed.Add(angular_speed);
            netManager.Snapshot(msg);
        }
    }
}