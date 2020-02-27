using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decay : MonoBehaviour
{
    public float liveTime = 4.0f;
    float curTime = 0.0f;
    // Update is called once per frame
    void Update()
    {
        curTime += Time.deltaTime;
        if(curTime > liveTime)
        {
            Destroy(this.gameObject);
        }
    }
}
