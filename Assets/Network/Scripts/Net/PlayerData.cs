using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SmashDomeNetwork {

    
    
    public class PlayerData
    {
        public ClientData clientData;
        public string name { get { return name; }  set { name = value; } }

        public GameObject obj;
        public Player playerControl;

        public PlayerData(ClientData clientData)
        {
            this.clientData = clientData;
        }
        public int GetPlayerType()
        {
            return clientData.playerType;
        }
        
    }
}
