using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DetectPlayerCollision : NetworkBehaviour
{
    [SerializeField] private AudioClip pickupSound; // Sonido al recoger la moneda

    private void OnTriggerEnter(Collider other)
    {
        if(!IsServer)return; //Solo el server se va a encargar de comprobar el trigger y despues de mandar a clientes que hacer
        if (other.CompareTag("PlayableCharacter")) // Verifica si el jugador toc� la moneda
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && !player.isZombie) // Verifica si el jugador no es un zombie
            {
                player.CoinCollected();
                PlaySoundClientRpc(transform.position);

                Destroy(gameObject); // Elimina la moneda de la escena
            }
        }
    }
    [ClientRpc]
    public void PlaySoundClientRpc(Vector3 position){
        AudioSource.PlayClipAtPoint(pickupSound, transform.position);
    }
}

