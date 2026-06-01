using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour // 1. Corregido 'MonoBehaviour'
{
    // Dejamos este nombre que tenías arriba
    public GameObject virtualCamera; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 3. Modificado para que detecte si entra Player, Pulpo O Babosa
        if ((other.CompareTag("Player") || other.CompareTag("Pulpo") || other.CompareTag("Babosa")) && !other.isTrigger)
        {
            // 2. Corregido a 'virtualCamera' para que coincida con lo de arriba
            virtualCamera.SetActive(true); 
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 3. Lo mismo para cuando el personaje sale del cuarto
        if ((other.CompareTag("Player") || other.CompareTag("Pulpo") || other.CompareTag("Babosa")) && !other.isTrigger)
        {
            // 2. Corregido a 'virtualCamera'
            virtualCamera.SetActive(false); 
        }
    }
}
