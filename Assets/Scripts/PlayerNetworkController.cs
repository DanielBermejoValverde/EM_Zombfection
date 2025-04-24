using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerNetworkController : NetworkBehaviour
{
    public ulong playerId;
    public bool isReady;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            playerId = NetworkManager.Singleton.LocalClientId;
            isReady = false;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }

}
