using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SmashDomeNetwork;
public class ExitScript : MonoBehaviour
{
    public Button exitBtn;
    // Start is called before the first frame update
    public NetworkManager netManager;
    public GameObject netManagerOBJ;
    void Start()
    {
        Button btn = exitBtn.GetComponent<Button>();
        btn.onClick.AddListener(Exit);
        netManager = NetworkManager.Instance;
    }
    private void Update()
    {
        if (netManager == null)
            netManager = NetworkManager.Instance;
    }

    void Exit()
    {
        netManager.CloserServer();
        SceneManager.LoadScene(0);
    }
}
