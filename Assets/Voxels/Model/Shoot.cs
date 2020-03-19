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

        //Right click for raycast testing
        if (Input.GetMouseButtonDown(1))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 100.0f))
                {
                    //Destroy(hit.transform.gameObject);
                    // Draws raycast line in scene
                    // DrawRay   (start position,     end position,                         color,      duration of time )
                    Debug.DrawRay(transform.position, Camera.main.transform.forward * 10, Color.green, 10.0f);
                    Debug.Log("You hit the " + hit.transform.name); // ensure you picked right object
                    Vector3 pointOfCollision = hit.point;
                    Debug.Log("Hit at point: " + pointOfCollision.ToString("F4"));
                }
            }

        if (mousedown && curtime > fireRate)
        {
            Vector3 dir = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            //GameObject bull = Instantiate(bullet, transform.localPosition + transform.forward, transform.rotation);
            GameObject bull = Instantiate(bullet, Camera.main.transform.position + Camera.main.transform.forward/2, Camera.main.transform.rotation);
            Rigidbody rig = bull.GetComponent<Rigidbody>();
            rig.useGravity = false;
            //rig.AddForce(Physics.gravity * (rig.mass * rig.mass));
            //rig.AddForce((transform.forward + transform.up / 4) * 2.0f);
            rig.AddForce(cam.forward);
            curtime = 0;
        }

    }
}
