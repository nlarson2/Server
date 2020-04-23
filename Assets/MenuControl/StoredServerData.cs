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
    public Text pause;
    public int pauseLimit;
    public Text round;
    public int roundLimit;
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
            if (portNumber.text == "" || pause.text == "" || round.text == "")
            {
                Debug.Log("ERROR NULL VALUE");
                return;
            }

            parsedPortNumber = Int32.Parse(portNumber.text);
            pauseLimit = Int32.Parse(pause.text);
            roundLimit = Int32.Parse(round.text) * 60;

            if (pauseLimit <= 0 || roundLimit <= 0)
            {
                Debug.Log("ERROR NULL VALUE");
                return;
            }

            SceneManager.LoadScene(1);
            Debug.Log(portNumber.text);
        }
        catch(Exception e)
        {
            portNumber.text = "Not A Valid Port";
            Debug.Log(e);
        }
    }
    public void Destory()
    {
        Destroy(this.gameObject);
    }
}
