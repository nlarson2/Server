using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmashDomeNetwork;
public class GameManager : MonoBehaviour
{
    static int count = 0;
    NetworkManager netManager;
    bool worldBreakable = false;
    double currentTime;
    public double waitTime = 10.0f;
    public double gameTime = 30.0f;
    bool resetCalled = false;
    double resetTimer = 0;

    public Transform[] respawnPoints;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(count++);
        currentTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (netManager == null)
        {
            netManager = NetworkManager.Instance;
        }
        if (!worldBreakable)
        {
            currentTime += Time.deltaTime;
            if(currentTime > waitTime)
            {
                Debug.Log("Allowing to be broken");
                worldBreakable = true;
                resetCalled = false;
            }
        }
        else if(!resetCalled)
        {
            currentTime += Time.deltaTime;
            resetTimer += Time.deltaTime;
            if (currentTime > gameTime)
            {
                    this.currentTime = 0.0f;
                    worldBreakable = false;
                    Debug.Log($"RESETTING FROM HERE {currentTime}");
                    ResetGame();
                    resetCalled = true;
            }
        }


    }

    public Vector3 GetRespawnPoint()
    {
        return respawnPoints[Random.Range(0, respawnPoints.Length)].position;
    }
    public bool IsWorldBreakable()
    {
        return worldBreakable;
    }
    public void ResetGame()
    {
        Debug.Log("RESETTING SERVER");
        //send out a refresh to everything
        worldBreakable = false;
        GameObject[] models = GameObject.FindGameObjectsWithTag("Model");
        foreach (GameObject model in models)
        {
            model.GetComponent<SmashDomeVoxel.VoxelModel>().ResetVoxel();
        }
        netManager.ResetGame();
        currentTime = 0;
    }
}
