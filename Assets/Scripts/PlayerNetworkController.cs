using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class PlayerNetworkController : NetworkBehaviour
{
    public NetworkVariable<FixedString128Bytes> UniqueId = new NetworkVariable<FixedString128Bytes>();
    public NetworkVariable<bool> IsReady = new NetworkVariable<bool>(false);

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            var generator = FindObjectOfType<UniqueIdGenerator>();
            if (generator != null)
            {
                UniqueId.Value = generator.GenerateUniqueID();
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }

}
