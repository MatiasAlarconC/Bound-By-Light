using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour 
{
    public GameObject virtualCamera; 

    // === NUEVO: Funciones para que el GameManager pueda controlar los cuartos ===
    public void ActivarCamaraManualmente()
    {
        if (virtualCamera != null)
        {
            virtualCamera.SetActive(true);
        }
    }

    public void DesactivarCamaraManualmente()
    {
        if (virtualCamera != null)
        {
            virtualCamera.SetActive(false);
        }
    }
    // ===========================================================================

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((other.CompareTag("Player") || other.CompareTag("Pulpo") || other.CompareTag("Babosa")) && !other.isTrigger)
        {
            virtualCamera.SetActive(true); 
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if ((other.CompareTag("Player") || other.CompareTag("Pulpo") || other.CompareTag("Babosa")) && !other.isTrigger)
        {
            virtualCamera.SetActive(false); 
        }
    }
}
