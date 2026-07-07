using UnityEngine;

public class ObjetoCheckpoint1 : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // SECTOR DE SEGURIDAD ABSOLUTA: 
        // Si el objeto que entra NO tiene la etiqueta "Player", la función se destruye aquí mismo.
        // El pulpo (Ángel) chocará físicamente, pero el código morirá en esta línea y no hará NADA.
        if (!other.CompareTag("Player"))
        {
            return; 
        }

        // Si el código pasa de la línea anterior, significa que SÍ es la Babosa (Player)
        GameManager elCerebro = FindFirstObjectByType<GameManager>();
        if (elCerebro != null)
        {
            Animator miAnimator = GetComponent<Animator>();
            
            // Le enviamos los datos al GameManager para que guarde y active la burbuja
            elCerebro.GuardarNuevoCheckpoint(transform.position, miAnimator);
        }
    }
}
