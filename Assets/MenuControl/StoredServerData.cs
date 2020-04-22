using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StoredServerData : MonoBehaviour
{
    public Button button;
    public Text portNumber;
    public int parsedPortNumber;

    // Start is called before the first frame update
    void Start()
    {
        
        DontDestroyOnLoad(this.gameObject);
        Button buttonListener = button.GetComponent<Button>();
        buttonListener.onClick.AddListener(OnBtnClick);
    }
    void OnBtnClick()
    {
        try
        {
            parsedPortNumber = Int32.Parse(portNumber.text);
            SceneManager.LoadScene(1);
            Debug.Log(portNumber.text);
        }
        catch(Exception e)
        {
            portNumber.text = "Not A Valid Port";
        }
    }
    public void Destory()
    {
        Destroy(this.gameObject);
    }
}
