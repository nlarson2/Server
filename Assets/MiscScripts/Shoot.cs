using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    public GameObject bullet;
    public Transform cam;
    public bool hasGravity = true;
    public float fireRate = 0.5f;
    float curtime = 0.0f;
    bool mousedown = false;
    // Update is called once per frame
    void Update()
    {
        curtime += Time.deltaTime;
        if(Input.GetMouseButtonDown(0))
        {
            mousedown = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            mousedown = false;
        }
        if (mousedown && curtime > fireRate)
        {
            Vector3 dir = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            GameObject bull = Instantiate(bullet, transform.localPosition + (transform.forward + cam.forward)/4, transform.rotation);
            Rigidbody rig = bull.GetComponent<Rigidbody>();
            rig.useGravity = false;
            //rig.AddForce(Physics.gravity * (rig.mass * rig.mass));
            //rig.AddForce((transform.forward + transform.up / 4) * 2.0f);
            rig.AddForce(cam.forward * 2.0f);
            curtime = 0;
        }

    }
}
