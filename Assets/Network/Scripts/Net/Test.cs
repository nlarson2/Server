using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmashDomeNetwork;


public class Test : MonoBehaviour
{
    BigTest msg = new BigTest();
    // Start is called before the first frame update
    void Start()
    {
        msg.Setup();
        string json = JsonUtility.ToJson(msg);
        Debug.Log(json);
        Message check = JsonUtility.FromJson<Message>(json);
        Debug.Log(check);
       // Debug.Log(check.msgType);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
