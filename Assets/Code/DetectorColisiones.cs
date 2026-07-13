using UnityEngine;

public class DetectorColisiones : MonoBehaviour
{
    private GameManager gameManager;

    void Start()
    {
        // Buscamos el GameManager automáticamente en la escena al iniciar
        gameManager = Object.FindFirstObjectByType<GameManager>();
    }

    // Se ejecuta cuando el personaje entra en un objeto que es "Is Trigger"
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (gameManager == null) return;

        // 1. Si chocamos con la burbuja checkpoint
        if (collision.CompareTag("Checkpoint"))
        {
            // === FILTRO ABSOLUTO SEGÚN EL PERSONAJE ===
            // Le preguntamos a este objeto exacto: "¿Tu etiqueta de personaje es 'Player' (Babosa)?"
            // Si NO somos la babosa (ej: somos el Ángel con etiqueta Pulpo), cancelamos la colisión inmediatamente
            if (!gameObject.CompareTag("Player"))
            {
                Debug.Log("<color=orange>¡DetectorColisiones: El Ángel de Mar tocó el checkpoint pero fue rechazado!</color>");
                return; 
            }

            // Si el código continúa aquí abajo, es porque SÍ somos la Babosa ("Player"):
            
            // Le enviamos la posición de la burbuja al GameManager para que la guarde
            gameManager.GuardarNuevoCheckpoint(collision.transform.position);

            // Buscamos la animación de la burbuja y la hacemos explotar
            Animator animBurbuja = collision.GetComponent<Animator>();
            if (animBurbuja != null)
            {
                // Usamos tu lógica original por nombre de animación
                animBurbuja.Play("Burbuja_Explotar");
            }

            // Desactivamos el colisionador de la burbuja para que no vuelva a procesarse
            collision.enabled = false;
        }

        // 2. Si chocamos con pinchos o vacío (Esto lo siguen haciendo ambos personajes)
        if (collision.CompareTag("DeadZone"))
        {
            // Babosa colgada del tentáculo o montada en Angel → inmune al geiser
            BabosaControl babosa = GetComponent<BabosaControl>();
            if (babosa != null && (babosa.EstaColgada || babosa.EstaMontada)) return;

            gameManager.MuerteYRespawnCooperativo();
        }
    }
}
