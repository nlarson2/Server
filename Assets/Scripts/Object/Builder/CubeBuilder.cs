using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeBuilder : MonoBehaviour
{
    public bool collided = false;
    float time = 0;

    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "Node")
        {
            //Debug.Log("COLLIDED ENTER");
            //Destroy(this.gameObject);

            this.gameObject.GetComponent<Collider>().enabled = false;
            collided = true;
        }
    }
    /*private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag != "Node")
        {
            Debug.Log("COLLIDED STAY");
            //Destroy(this.gameObject);
        }
    }*/
    
    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if(time > 2)
        {
            if (collided == false)
            {
                Destroy(this.gameObject);
                return;
            }
            this.gameObject.GetComponent<Collider>().enabled = false;
        }
    }
}
