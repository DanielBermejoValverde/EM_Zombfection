using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerLobbyManager : MonoBehaviour
{

    public TMP_Text playerNameText;
    public TMP_Text readyText;


    public void SetInfo(string name, bool isReady)
    {
        playerNameText.text = name;
        readyText.text = isReady ? "Ready" : "Not Ready";
    }
}
